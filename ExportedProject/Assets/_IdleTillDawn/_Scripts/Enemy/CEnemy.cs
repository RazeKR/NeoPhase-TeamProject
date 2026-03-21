using System;
using UnityEngine;

/// <summary>
/// 일반 몬스터의 이동, 피격, 사망 처리를 담당하는 컴포넌트
/// 오브젝트 풀링과 완전히 호환되도록 Start 대신 Initialize로 활성화 시점 초기화를 수행한다
/// 사망 시 OnDied 이벤트를 발행하고 실제 비활성화는 CSpawnManager에 위임하여 책임을 분리한다
/// 스테이지 배율은 Initialize에서 주입받아 ScriptableObject를 직접 참조하지 않아도 되도록 설계한다
/// </summary>
public class CEnemy : MonoBehaviour
{
    #region Events

    public event Action<CEnemy> OnDied; // 사망 시 발행 — CSpawnManager가 구독하여 풀 반환 처리

    #endregion

    #region Inspector Variables

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 2f; // 플레이어 추적 이동 속도 (단위: Unity 유닛/초)

    [Header("스탯 데이터")]
    [SerializeField] private CEnemyStatData _statData; // 기본 스탯 SO (기본 HP, 이동속도 기준값)

    [Header("전투")]
    [SerializeField] private float _attackDamage   = 5f;  // 플레이어에게 입힐 데미지
    [SerializeField] private float _attackCooltime = 1f;  // 공격 쿨타임 (초)
    [SerializeField] private float _attackRange    = 0.8f; // 공격 가능 거리

    #endregion

    #region Private Variables

    private Rigidbody2D rb;               // 물리 이동 처리를 위해 캐싱한 Rigidbody2D 컴포넌트
    private Transform   target;           // 추적 대상인 플레이어의 Transform
    private float       currentHp;        // 현재 체력 (피격 시 감소)
    private float       maxHp;            // 최대 체력 (스테이지 배율 적용 후 확정값)
    private bool        isDead;           // 사망 여부 플래그 (중복 사망 이벤트 방지용)
    private float       _lastAttackTime;  // 마지막 공격 시각 (쿨타임 계산)

    #endregion

    #region Unity Methods

    /// <summary>
    /// 최초 1회 호출된다
    /// Rigidbody2D를 캐싱한다
    /// </summary>
    private void Awake() => rb = GetComponent<Rigidbody2D>();

    /// <summary>
    /// 오브젝트가 비활성화될 때(풀 반환 시) 호출된다
    /// velocity를 0으로 초기화하여 재활성화 시 이전 관성이 남지 않도록 한다
    /// isDead 플래그도 초기화하여 다음 활성화 사이클에서 정상 동작하도록 보장한다
    /// </summary>
    private void OnDisable()
    {
        if (rb != null) rb.velocity = Vector2.zero;
        isDead         = false;
        _lastAttackTime = 0f; // 풀 반환 시 공격 타이머 초기화
    }

    /// <summary>
    /// 매 FixedUpdate에서 플레이어 방향으로 이동한다
    /// FixedUpdate를 사용하여 물리 연산과 동기화하고 프레임레이트 독립적인 이동을 보장한다
    /// 플레이어 참조가 없거나 사망 상태이면 즉시 반환하여 불필요한 연산을 차단한다
    /// </summary>
    private void FixedUpdate()
    {
        if (target == null) return;
        if (isDead) return;

        float dist = Vector2.Distance(transform.position, target.position);
        Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;

        // 공격 사거리 밖이면 추적 이동
        if (dist > _attackRange)
        {
            rb.velocity = dir * _moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
            AttackPlayer();
        }
    }

    /// <summary>
    /// 쿨타임마다 플레이어에게 근접 데미지를 입힌다
    /// </summary>
    private void AttackPlayer()
    {
        if (Time.time < _lastAttackTime + _attackCooltime) return;

        IDamageable player = target.GetComponent<IDamageable>();
        if (player == null) return;

        _lastAttackTime = Time.time;
        player.TakeDamage(_attackDamage);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 풀에서 꺼낼 때 CSpawnManager가 호출하는 초기화 메서드
    /// 스테이지 배율을 외부에서 주입받아 적용하므로 ScriptableObject를 직접 읽지 않아도 된다
    /// </summary>
    /// <param name="playerTarget">추적할 플레이어 Transform</param>
    /// <param name="hpMultiplier">스테이지 체력 배율 (CStageData._hpMultiplier)</param>
    public void Initialize(Transform playerTarget, float hpMultiplier)
    {
        target    = playerTarget;
        maxHp     = _statData != null
            ? _statData.CalculateHp(0) * hpMultiplier // StatData 연결 시 배율 적용
            : 10f * hpMultiplier;                      // StatData 미연결 시 기본값 사용
        currentHp = maxHp;
    }

    /// <summary>
    /// 외부(플레이어 공격, 스킬, 범위기 등)에서 호출하는 피격 처리 메서드
    /// 체력이 0 이하가 되면 즉시 Die를 호출하며 이미 사망 중인 경우 중복 처리를 방지한다
    /// </summary>
    /// <param name="damage">받는 피해량</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return; // 중복 사망 방지

        currentHp -= damage;
        if (currentHp <= 0f) Die();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 사망 처리를 수행한다
    /// 이동을 즉시 정지하고 isDead 플래그를 세운 뒤 OnDied 이벤트를 발행한다
    /// 실제 비활성화(SetActive(false))는 CSpawnManager.ReturnToPool에서 처리한다
    /// </summary>
    private void Die()
    {
        isDead      = true;
        rb.velocity = Vector2.zero;
        OnDied?.Invoke(this); // CSpawnManager가 구독하여 풀 반환 처리
    }

    #endregion
}
