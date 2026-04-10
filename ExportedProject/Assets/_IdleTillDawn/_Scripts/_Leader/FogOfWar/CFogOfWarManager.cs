using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fog of War 매니저 — 싱글턴
///
/// [Visibility RenderTexture 방식]
/// 구버전의 광원 배열(MAX_LIGHTS 고정 상한) 방식을 폐기하고
/// 매 프레임 GL 즉시 모드로 Visibility RenderTexture에 원을 직접 그린다
/// 셰이더는 이 RT를 샘플링만 하므로 광원 수에 제한이 없다
///
/// [처리 흐름]
/// 1. LateUpdate: 카메라 월드 경계 계산
/// 2. Visibility RT를 black으로 클리어
/// 3. 등록된 모든 광원을 GL 쿼드(FogCircle 셰이더)로 RT에 가산 혼합
/// 4. FogMaterial._VisibilityTex에 RT 주입
/// 5. CFogRenderFeature.OnRenderImage에서 단일 Blit으로 합성
/// </summary>
public class CFogOfWarManager : MonoBehaviour
{
    #region Singleton

    public static CFogOfWarManager Instance { get; private set; }

    #endregion

    #region Inspector Variables

    [Header("카메라 참조")]
    [Tooltip("비워두면 Camera.main(MainCamera 태그)을 자동 탐색")]
    [SerializeField] private Camera _mainCameraOverride;

    [Header("포그 비주얼")]
    [SerializeField] private Color _fogColor    = new Color(0.04f, 0f, 0.08f, 1f);
    [SerializeField] [Range(0f, 1f)] private float _fogDensity = 0.95f;

    [Header("머티리얼 참조 (필수)")]
    [SerializeField] private Material _fogMaterial;    // FogOfWar.shader 로 만든 머티리얼
    [SerializeField] private Material _circleMaterial; // FogCircle.shader 로 만든 머티리얼

    #endregion

    #region Private Variables

    private Camera _mainCamera;

    private readonly List<CFogLightSource> _sources = new List<CFogLightSource>(128);

    private RenderTexture _visibilityRT; // 가시성 맵 — FogCircle 셰이더가 매 프레임 갱신

    // 셰이더 프로퍼티 ID 캐시
    private static readonly int ID_VisibilityTex = Shader.PropertyToID("_VisibilityTex");
    private static readonly int ID_FogColor      = Shader.PropertyToID("_FogColor");
    private static readonly int ID_FogDensity    = Shader.PropertyToID("_FogDensity");

    #endregion

    #region Properties

    /// <summary>FogOfWar 머티리얼 — CFogRenderFeature가 OnRenderImage Blit에 사용한다</summary>
    public Material FogMaterial => _fogMaterial;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance    = this;
        _mainCamera = _mainCameraOverride != null ? _mainCameraOverride : Camera.main;

        if (_mainCamera == null)
            CDebug.LogError("[CFogOfWarManager] 카메라를 찾을 수 없습니다. _mainCameraOverride에 카메라를 직접 연결하거나 Main Camera 태그를 확인하세요.");

        // FogCircle 머티리얼 미연결 시 자동 생성 시도
        if (_circleMaterial == null)
        {
            Shader circleShader = Shader.Find("Hidden/FogCircle");
            if (circleShader != null)
                _circleMaterial = new Material(circleShader) { hideFlags = HideFlags.HideAndDontSave };
            else
                CDebug.LogError("[CFogOfWarManager] Hidden/FogCircle 셰이더를 찾을 수 없습니다. FogCircle.shader가 프로젝트에 포함되어 있는지 확인하세요.");
        }
    }

    /// <summary>
    /// LateUpdate에서 처리 — 모든 오브젝트의 Transform 갱신이 끝난 후 광원 위치를 수집한다
    /// </summary>
    private void LateUpdate() => UpdateVisibility();

    private void OnDestroy()
    {
        ReleaseRT();
        if (Instance == this) Instance = null;
    }

    #endregion

    #region Public Methods

    /// <summary>광원을 시스템에 등록 — CFogLightSource.OnEnable/Start에서 호출</summary>
    public void Register(CFogLightSource source)
    {
        if (!_sources.Contains(source)) _sources.Add(source);
    }

    /// <summary>광원을 시스템에서 제거 — CFogLightSource.OnDisable에서 호출</summary>
    public void Unregister(CFogLightSource source) => _sources.Remove(source);

    #endregion

    #region Private Methods

    /// <summary>
    /// 화면 해상도에 맞는 Visibility RenderTexture를 생성/재생성한다
    /// 해상도 변경 시 자동으로 새 RT를 만든다
    /// </summary>
    private void EnsureVisibilityRT()
    {
        int w = Screen.width;
        int h = Screen.height;

        if (_visibilityRT != null && _visibilityRT.width == w && _visibilityRT.height == h)
            return;

        ReleaseRT();

        // R8: 단일 채널 8비트 — 가시성 값(0~1) 저장에 최적, 메모리 절약
        _visibilityRT             = new RenderTexture(w, h, 0, RenderTextureFormat.R8);
        _visibilityRT.filterMode  = FilterMode.Bilinear;
        _visibilityRT.wrapMode    = TextureWrapMode.Clamp;
        _visibilityRT.Create();
    }

    private void ReleaseRT()
    {
        if (_visibilityRT == null) return;
        _visibilityRT.Release();
        _visibilityRT = null;
    }

    /// <summary>
    /// Visibility RT에 모든 광원을 원(circle)으로 그리고 FogMaterial에 주입한다
    ///
    /// [광원 수 무제한 원리]
    /// 셰이더 배열 대신 GL 즉시 모드로 RT에 직접 그리므로
    /// 수백~수천 개의 광원이 등록되어도 처리 가능하다
    /// GPU는 가산 혼합된 원들을 자연스럽게 합산한다
    /// </summary>
    private void UpdateVisibility()
    {
        if (_fogMaterial == null || _mainCamera == null) return;

        EnsureVisibilityRT();

        // 카메라 월드 경계 계산 (직교 카메라 기준)
        float orthoH = _mainCamera.orthographicSize;
        float orthoW = orthoH * _mainCamera.aspect;
        Vector3 cam  = _mainCamera.transform.position;
        float minX   = cam.x - orthoW;
        float minY   = cam.y - orthoH;
        float camW   = orthoW * 2f;
        float camH   = orthoH * 2f;

        // ── Visibility RT 렌더링 ──────────────────────────────────────────────
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = _visibilityRT;

        // 이전 프레임 데이터 완전 초기화 (Stateless 보장)
        GL.Clear(false, true, Color.black);

        if (_circleMaterial != null && _sources.Count > 0)
        {
            _circleMaterial.SetPass(0);

            GL.PushMatrix();
            GL.LoadOrtho(); // 0~1 직교 투영 — 좌하단(0,0) ~ 우상단(1,1)

            GL.Begin(GL.QUADS);
            foreach (CFogLightSource src in _sources)
            {
                if (src == null) continue;

                Vector3 p    = src.transform.position;

                // 월드 좌표 → 카메라 UV(0~1) 변환
                float uvX = (p.x - minX) / camW;
                float uvY = (p.y - minY) / camH;

                // 반경도 UV 스케일로 변환
                float rX  = src.OuterRadius / camW;
                float rY  = src.OuterRadius / camH;

                // innerRatio: FogCircle 셰이더의 smoothstep 시작점
                float inner  = src.OuterRadius > 0f ? src.InnerRadius / src.OuterRadius : 0f;
                float intens = src.Intensity;

                // GL.Color로 innerRatio(r)과 intensity(g)를 셰이더에 전달
                var col = new Color(inner, intens, 0f, 0f);

                // 쿼드 4 꼭짓점 (반시계 방향, Cull Off이므로 무관)
                // TexCoord: 원 로컬 UV (-1~+1), 셰이더에서 length로 거리 계산
                GL.Color(col); GL.TexCoord2(-1f, -1f); GL.Vertex3(uvX - rX, uvY - rY, 0f);
                GL.Color(col); GL.TexCoord2( 1f, -1f); GL.Vertex3(uvX + rX, uvY - rY, 0f);
                GL.Color(col); GL.TexCoord2( 1f,  1f); GL.Vertex3(uvX + rX, uvY + rY, 0f);
                GL.Color(col); GL.TexCoord2(-1f,  1f); GL.Vertex3(uvX - rX, uvY + rY, 0f);
            }
            GL.End();

            GL.PopMatrix();
        }

        RenderTexture.active = prev;
        // ─────────────────────────────────────────────────────────────────────

        // FogOfWar 셰이더에 Visibility RT 및 시각 파라미터 주입
        _fogMaterial.SetTexture(ID_VisibilityTex, _visibilityRT);
        _fogMaterial.SetColor(  ID_FogColor,      _fogColor);
        _fogMaterial.SetFloat(  ID_FogDensity,     _fogDensity);
    }

    #endregion
}
