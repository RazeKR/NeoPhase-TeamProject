using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일시적으로 밝아졌다가 페이드 아웃되는 포그 광원
///
/// [최적화 포인트]
/// - 코루틴 대신 Update 기반 타이밍 — 연사 시 코루틴 Start/Stop 알로케이션 없음
/// - 피격 플래시(SpawnImpact)는 정적 오브젝트 풀 재사용 — 연사 시 GameObject 신규 생성 없음
///
/// [사용 패턴 1 — 총구 화염]
/// CWeaponEquip이 총구 오브젝트에 AddComponent 후 InitializeFlash()로 설정하고
/// 발사마다 Trigger()를 호출한다.
///
/// [사용 패턴 2 — 투사체 피격]
/// CBullet/Projectile의 충돌 핸들러에서 SpawnImpact()를 호출한다.
/// 풀에서 꺼낸 오브젝트로 플래시를 재생하고 완료 후 풀에 반환한다.
/// </summary>
[AddComponentMenu("Fog of War/CFogFlashSource")]
public class CFogFlashSource : CFogLightSource
{
    #region Inspector Variables

    [Header("플래시 반경·강도")]
    [Tooltip("플래시 최대 외곽 반경 (월드 유닛) — 플레이어 고유 반경보다 크게 설정 권장")]
    [SerializeField] private float _flashOuterRadius = 6f;

    [Tooltip("플래시 내부 완전 밝음 비율 (0~1)")]
    [SerializeField] [Range(0f, 1f)] private float _flashInnerRatio = 0.35f;

    [Tooltip("플래시 최대 밝기 (0~1) — 플레이어 고유 광원보다 높게 설정 권장")]
    [SerializeField] [Range(0f, 1f)] private float _flashPeakIntensity = 1f;

    [Header("플래시 타이밍")]
    [Tooltip("최대 밝기에서 0으로 페이드 아웃되는 시간(초)")]
    [SerializeField] private float _fadeOutDuration = 0.25f;

    #endregion

    #region Pool

    // 피격 플래시 전용 정적 풀 — SpawnImpact/ReturnToPool이 사용
    private static readonly Queue<CFogFlashSource> _pool = new Queue<CFogFlashSource>();

    // 이 인스턴스가 풀 관리 대상인지 여부 (true = 완료 후 Destroy 대신 풀 반환)
    private bool _isPooled;

    #endregion

    #region Update-based Timing (코루틴 없음)

    private float _flashStartTime = float.MinValue; // Trigger() 호출 시각
    private bool  _flashing       = false;          // 현재 페이드 진행 중 여부

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // 초기 상태는 완전히 꺼진 채로 등록 — Trigger() 전까지 포그에 영향 없음
        SetIntensity(0f);
    }

    private void Update()
    {
        if (!_flashing) return;

        float elapsed = Time.time - _flashStartTime;

        if (elapsed >= _fadeOutDuration)
        {
            // 페이드 완료
            SetIntensity(0f);
            _flashing = false;

            if (_isPooled)
                ReturnToPool();

            return;
        }

        // 선형 페이드 아웃 — 프레임당 float 연산만 수행 (알로케이션 없음)
        SetIntensity(Mathf.Lerp(_flashPeakIntensity, 0f, elapsed / _fadeOutDuration));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 런타임에서 플래시 파라미터를 일괄 설정한다.
    /// AddComponent 직후 코드로 생성할 때 사용 (인스펙터 대체).
    /// </summary>
    public void InitializeFlash(float outerRadius, float innerRatio, float peakIntensity, float fadeOutDuration)
    {
        _flashOuterRadius   = outerRadius;
        _flashInnerRatio    = Mathf.Clamp01(innerRatio);
        _flashPeakIntensity = Mathf.Clamp01(peakIntensity);
        _fadeOutDuration    = Mathf.Max(0.01f, fadeOutDuration);
    }

    /// <summary>
    /// 총구화염 등 반복 재사용 광원에서 플래시를 1회 재생한다.
    /// 연사 시 이미 페이드 중이라면 처음부터 재시작해 강도를 갱신한다.
    /// 코루틴 없이 Update에서 처리하므로 연사 시 알로케이션이 없다.
    /// </summary>
    public void Trigger()
    {
        SetOuterRadius(_flashOuterRadius);
        SetInnerRatio(_flashInnerRatio);
        SetIntensity(_flashPeakIntensity);
        _flashStartTime = Time.time;
        _flashing       = true;
    }

    /// <summary>
    /// 투사체 피격 위치에 풀에서 광원을 꺼내 플래시를 재생한다.
    /// 완료 후 자동으로 풀에 반환된다. GameObject 신규 생성은 풀 소진 시에만 발생한다.
    /// CFogOfWarManager가 씬에 없으면 아무것도 하지 않는다.
    /// </summary>
    /// <param name="worldPos">피격 월드 좌표</param>
    /// <param name="outerRadius">플래시 반경 (월드 유닛)</param>
    /// <param name="innerRatio">내부 완전 밝음 비율 (0~1)</param>
    /// <param name="peakIntensity">최대 밝기 (0~1)</param>
    /// <param name="fadeOutDuration">페이드 아웃 시간 (초)</param>
    public static void SpawnImpact(
        Vector3 worldPos,
        float   outerRadius     = 4f,
        float   innerRatio      = 0.3f,
        float   peakIntensity   = 1f,
        float   fadeOutDuration = 0.3f)
    {
        if (CFogOfWarManager.Instance == null) return;

        CFogFlashSource flash = GetFromPool();
        flash.transform.position = worldPos;

        // 같은 클래스 내부이므로 private 필드 직접 접근 가능
        flash._flashOuterRadius   = outerRadius;
        flash._flashInnerRatio    = Mathf.Clamp01(innerRatio);
        flash._flashPeakIntensity = Mathf.Clamp01(peakIntensity);
        flash._fadeOutDuration    = Mathf.Max(0.01f, fadeOutDuration);
        flash._isPooled           = true;

        flash.Trigger();
    }

    #endregion

    #region Pool Implementation

    /// <summary>
    /// 풀에서 인스턴스를 꺼낸다.
    /// 풀이 비어 있거나 씬 전환으로 null이 된 항목만 남은 경우 새로 생성한다.
    /// </summary>
    private static CFogFlashSource GetFromPool()
    {
        while (_pool.Count > 0)
        {
            CFogFlashSource candidate = _pool.Dequeue();
            if (candidate != null) // 씬 전환으로 Destroy된 경우 스킵
            {
                candidate.gameObject.SetActive(true); // OnEnable → Register 자동 호출
                return candidate;
            }
        }

        // 풀 소진 — 새 오브젝트 생성 (연사가 빠를수록 초반에만 발생)
        var go    = new GameObject("FogImpactFlash");
        var flash = go.AddComponent<CFogFlashSource>();
        return flash;
    }

    /// <summary>
    /// 플래시 완료 후 풀에 반환한다. Destroy 대신 SetActive(false)로 비용 절감.
    /// OnDisable → Unregister가 자동 호출되어 포그 시스템에서 제거된다.
    /// </summary>
    private void ReturnToPool()
    {
        _isPooled = false;
        gameObject.SetActive(false); // OnDisable → Unregister 자동 호출
        _pool.Enqueue(this);
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, _flashOuterRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _flashOuterRadius * _flashInnerRatio);
    }
#endif
}
