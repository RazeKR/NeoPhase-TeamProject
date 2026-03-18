using UnityEngine;

/// <summary>
/// 적 오브젝트의 기본 동작을 담당하는 클래스
/// 활성화될 때마다 플레이어를 자동으로 탐색하고 매 프레임 플레이어 방향으로 이동한다
/// Rigidbody2D velocity를 직접 제어하여 물리 기반으로 추적하며
/// 오브젝트 풀링과 연동되어 OnEnable / OnDisable 시점에 상태를 초기화한다
/// </summary>
public class CEnemy : MonoBehaviour
{
    #region Inspector Variables

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 2f; // 플레이어 추적 이동 속도 (단위: Unity 유닛/초)

    #endregion

    #region Private Variables

    private Rigidbody2D rb;      // 물리 이동 처리를 위해 캐싱한 Rigidbody2D 컴포넌트
    private Transform   player;  // 추적 대상인 플레이어의 Transform (OnEnable마다 재탐색)

    #endregion

    #region Unity Methods

    /// <summary>
    /// 최초 1회 호출된다
    /// Rigidbody2D를 캐싱한다
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 오브젝트가 활성화될 때마다 호출된다
    /// 풀에서 꺼낼 때마다 플레이어 Transform을 재탐색하여 참조를 최신 상태로 유지한다
    /// "Player" 태그를 기준으로 탐색하므로 플레이어 오브젝트에 태그 설정이 필요하다
    /// </summary>
    private void OnEnable()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    /// <summary>
    /// 오브젝트가 비활성화될 때(풀 반환 시) 호출된다
    /// velocity를 0으로 초기화하여 재활성화 시 이전 관성이 남지 않도록 한다
    /// </summary>
    private void OnDisable()
    {
        if (rb != null) rb.velocity = Vector2.zero;
    }

    /// <summary>
    /// 매 프레임 호출된다
    /// 플레이어 방향 단위 벡터를 계산하여 Rigidbody2D velocity에 적용한다
    /// 플레이어 참조가 없으면 즉시 반환하여 NullReference를 방지한다
    /// </summary>
    private void Update()
    {
        if (player == null) return;

        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized; // 플레이어 방향 단위 벡터
        rb.velocity = dir * _moveSpeed;
    }

    #endregion
}
