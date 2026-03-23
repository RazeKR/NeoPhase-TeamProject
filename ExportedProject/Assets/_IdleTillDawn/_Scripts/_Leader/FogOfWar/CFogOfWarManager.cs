using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fog of War 매니저 — 싱글턴
///
/// [Stateless 설계]
/// 이전 구현(누적형)과 달리 RenderTexture 상태를 유지하지 않는다
/// 매 프레임 광원 데이터를 FogOfWar 셰이더에 직접 전달하고
/// 셰이더가 현재 광원만으로 가시성을 즉시 계산한다
/// 따라서 광원이 이동해도 이전 위치의 잔상(트레이서)이 전혀 남지 않는다
///
/// [처리 흐름]
/// 1. 카메라 월드 경계 계산 (셰이더의 UV→WorldPos 변환 기준)
/// 2. 등록된 광원 위치/반경을 배열로 수집
/// 3. FogMaterial 에 파라미터 주입
/// 4. CFogRenderFeature.OnRenderImage 에서 단일 Blit으로 합성
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
    [SerializeField] private Color _fogColor         = new Color(0.04f, 0f, 0.08f, 1f); // 어두운 보라 계열
    [SerializeField] [Range(0f, 1f)] private float _fogDensity = 0.95f;                 // 포그 최대 불투명도

    [Header("머티리얼 참조 (필수)")]
    [SerializeField] private Material _fogMaterial; // FogOfWar.shader 로 만든 머티리얼

    #endregion

    #region Private Variables

    private Camera _mainCamera; // UV→WorldPos 변환 기준 카메라

    private readonly List<CFogLightSource> _sources = new List<CFogLightSource>(32); // 등록된 광원 목록

    // 셰이더 전달용 광원 배열
    private const    int       MAX_LIGHTS   = 32;
    private readonly Vector4[] _lightBuffer = new Vector4[MAX_LIGHTS]; // xy=월드좌표, z=outerRadius, w=innerRadius
    private readonly Vector4[] _lightParams = new Vector4[MAX_LIGHTS]; // x=intensity

    // 셰이더 프로퍼티 ID 캐시
    private static readonly int ID_LightSources  = Shader.PropertyToID("_LightSources");
    private static readonly int ID_LightParams   = Shader.PropertyToID("_LightParams");
    private static readonly int ID_LightCount    = Shader.PropertyToID("_LightCount");
    private static readonly int ID_CameraBounds  = Shader.PropertyToID("_CameraBounds");
    private static readonly int ID_FogColor      = Shader.PropertyToID("_FogColor");
    private static readonly int ID_FogDensity    = Shader.PropertyToID("_FogDensity");

    #endregion

    #region Properties

    /// <summary>FogOfWar 머티리얼 — CFogRenderFeature 가 OnRenderImage Blit 에 사용한다</summary>
    public Material FogMaterial => _fogMaterial;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance    = this;
        _mainCamera = _mainCameraOverride != null ? _mainCameraOverride : Camera.main;

        if (_mainCamera == null)
            Debug.LogError("[CFogOfWarManager] 카메라를 찾을 수 없습니다. _mainCameraOverride 에 카메라를 직접 연결하거나 Main Camera 태그를 확인하세요.");
    }

    /// <summary>
    /// 매 프레임 셰이더 파라미터를 갱신한다
    /// 이전 상태(RT)를 읽지 않으므로 광원 이동 시 잔상이 발생하지 않는다
    /// </summary>
    private void Update() => UpdateFogMaterial();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    #endregion

    #region Public Methods

    /// <summary>광원을 시스템에 등록 — CFogLightSource.OnEnable 에서 호출</summary>
    public void Register(CFogLightSource source)
    {
        if (!_sources.Contains(source)) _sources.Add(source);
    }

    /// <summary>광원을 시스템에서 제거 — CFogLightSource.OnDisable 에서 호출</summary>
    public void Unregister(CFogLightSource source) => _sources.Remove(source);

    #endregion

    #region Private Methods

    /// <summary>
    /// 셰이더에 현재 프레임의 카메라 경계와 광원 데이터를 주입한다
    ///
    /// [Stateless 핵심]
    /// 이 메서드는 이전 프레임 데이터를 전혀 참조하지 않는다
    /// 셰이더도 _MainTex(이전 RT) 없이 오직 광원 배열만으로 계산한다
    /// 결과적으로 매 프레임 독립적으로 포그가 계산되어 잔상이 원천 차단된다
    /// </summary>
    private void UpdateFogMaterial()
    {
        if (_fogMaterial == null) return;
        if (_mainCamera  == null) return;

        // 카메라 실제 뷰 영역을 월드 좌표 경계로 계산
        // orthographicSize: 화면 절반 높이 (월드 유닛)
        // aspect: 가로/세로 비율 → 절반 너비 = orthoH * aspect
        float orthoH  = _mainCamera.orthographicSize;
        float orthoW  = orthoH * _mainCamera.aspect;
        Vector3 cam   = _mainCamera.transform.position;
        Vector4 bounds = new Vector4(
            cam.x - orthoW, // minX
            cam.y - orthoH, // minY
            cam.x + orthoW, // maxX
            cam.y + orthoH  // maxY
        );

        // 등록된 광원 데이터 수집
        int count = Mathf.Min(_sources.Count, MAX_LIGHTS);
        for (int i = 0; i < count; i++)
        {
            CFogLightSource s = _sources[i];
            if (s == null) continue;

            Vector3 p       = s.transform.position;
            _lightBuffer[i] = new Vector4(p.x, p.y, s.OuterRadius, s.InnerRadius);
            _lightParams[i] = new Vector4(s.Intensity, 0f, 0f, 0f);
        }

        // 셰이더 파라미터 주입
        _fogMaterial.SetVectorArray(ID_LightSources, _lightBuffer);
        _fogMaterial.SetVectorArray(ID_LightParams,  _lightParams);
        _fogMaterial.SetInt(        ID_LightCount,   count);
        _fogMaterial.SetVector(     ID_CameraBounds, bounds);
        _fogMaterial.SetColor(      ID_FogColor,     _fogColor);
        _fogMaterial.SetFloat(      ID_FogDensity,   _fogDensity);
    }

    #endregion
}
