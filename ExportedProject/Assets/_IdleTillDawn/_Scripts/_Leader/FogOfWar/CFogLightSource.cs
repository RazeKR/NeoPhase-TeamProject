using UnityEngine;

/// <summary>
/// 시야를 제공하는 광원 컴포넌트 — 플레이어, 스킬 이펙트, 투사체 등 시야를 발생시키는 오브젝트에 부착한다
/// OnEnable/OnDisable 시 CFogOfWarManager에 자동 등록/해제되므로
/// 투사체처럼 수명이 짧은 오브젝트도 생명주기에 맞게 풀 반환 전 자동 해제된다
/// </summary>
[AddComponentMenu("Fog of War/CFogLightSource")]
public class CFogLightSource : MonoBehaviour
{
    #region Inspector Variables

    [Header("시야 반경")]
    [SerializeField] private float _outerRadius = 5f;             // 시야 외곽 반경 (월드 유닛) — 이 거리 밖은 완전 어둠
    [SerializeField] [Range(0f, 1f)] private float _innerRatio = 0.5f; // 완전히 밝은 내부 반경 비율 (0.5 = 외곽 반경의 50% 이내 완전 밝음)

    [Header("시야 강도")]
    [SerializeField] [Range(0f, 1f)] private float _intensity = 1f;   // 광원 밝기 (0=없음, 1=최대)

    #endregion

    #region Properties

    public float OuterRadius => _outerRadius;                        // 외곽 반경 — CFogOfWarManager가 셰이더 버퍼 구성 시 사용
    public float InnerRadius => _outerRadius * _innerRatio;          // 실제 내부 반경 (미리 계산)
    public float Intensity   => _intensity;

    #endregion

    #region Unity Methods

    /// <summary>
    /// 활성화 시 매니저에 등록 시도
    /// 씬에 미리 배치된 오브젝트는 CFogOfWarManager.Awake()보다 먼저 호출되어
    /// Instance가 null일 수 있다 — Start()에서 재시도로 보완한다
    /// </summary>
    private void OnEnable()  => CFogOfWarManager.Instance?.Register(this);

    /// <summary>
    /// OnEnable 시점에 Instance가 null이었던 경우의 재시도 등록
    /// Start()는 모든 Awake()가 끝난 뒤 호출되므로 Instance가 반드시 존재한다
    /// Register() 내부에 중복 방지가 있으므로 이중 호출해도 안전하다
    /// </summary>
    private void Start()     => CFogOfWarManager.Instance?.Register(this);

    /// <summary>비활성화 시 매니저에서 제거 — 풀 반환, Destroy 모두 대응</summary>
    private void OnDisable() => CFogOfWarManager.Instance?.Unregister(this);

    #endregion

    #region Gizmos

#if UNITY_EDITOR
    /// <summary>씬 뷰에서 시야 내부/외부 반경을 노란색 원으로 시각화한다</summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, _outerRadius);         // 외곽 반경 (시야 끝)

        Gizmos.color = new Color(1f, 0.9f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _outerRadius * _innerRatio); // 내부 반경 (완전 밝음 영역)
    }
#endif

    #endregion
}
