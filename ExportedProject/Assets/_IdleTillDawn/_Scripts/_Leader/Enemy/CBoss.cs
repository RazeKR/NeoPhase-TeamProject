using System;
using UnityEngine;

/// <summary>
/// 보스 몬스터의 이동, 공격, 피격, 사망 처리를 담당하는 컴포넌트
/// 일반 몬스터(CEnemy)와 달리 공격력 스케일링이 적용되며 플레이어에게 직접 데미지를 입힌다
/// CBossManager가 생명주기를 관리하고 이 클래스는 순수하게 보스의 행동 로직에만 집중한다
/// </summary>
public class CBoss : MonoBehaviour, IDamageable
{
    #region Events

    public event Action OnDefeated;    // 보스 처치 완료 — CBossManager가 구독
    public event Action OnPlayerKilled; // 플레이어 사망 처리 — CBossManager가 구독

    #endregion

    #region Inspector Variables

    [Header("기본 스탯")]
    [SerializeField] private float _baseHp;        // 배율 적용 전 기본 체력
    [SerializeField] private float _baseAtk;       // 배율 적용 전 기본 공격력
    [SerializeField] private float _moveSpeed;     // 플레이어 추적 이동 속도
    [SerializeField] private float _attackRange;   // 공격 가능 거리 (단위: Unity 유닛)
    [SerializeField] private float _attackInterval; // 공격 쿨타임 (초)

    #endregion

    #region Private Variables

    private Rigidbody2D  rb;               // 물리 이동 컴포넌트
    private Transform    target;           // 추적 대상 (플레이어)
    private IDamageable  _playerDamageable; // 플레이어 피격 인터페이스
    private float        currentHp;        // 현재 체력
    private float        maxHp;            // 최대 체력 (배율 적용 후)
    private float        atk;              // 최종 공격력 (배율 적용 후)
    private float        attackTimer;      // 마지막 공격 이후 경과 시간
    private bool         isDead;           // 사망 여부 플래그 (중복 사망 방지용)

    #endregion

    #region Unity Methods

    /// <summary>
    /// Rigidbody2D를 캐싱한다
    /// CBossManager.SpawnBoss에서 Instantiate 직후 Initialize가 호출되므로
    /// Awake에서 컴포넌트 캐싱을 먼저 완료해야 한다
    /// </summary>
    private void Awake() => rb = GetComponent<Rigidbody2D>();

    /// <summary>
    /// 매 FixedUpdate에서 플레이어를 추적하고 공격 범위 내이면 공격을 시도한다
    /// 사망 상태이거나 타겟이 없으면 즉시 반환하여 불필요한 연산을 차단한다
    /// </summary>
    private void FixedUpdate()
    {
        if (isDead) return;
        if (target == null) return;

        float dist = Vector2.Distance(transform.position, target.position); // 플레이어와의 거리

        if (dist > _attackRange)
            Chase();   // 사거리 밖 → 추적
        else
            Attack();  // 사거리 내 → 공격 시도
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// CBossManager가 스폰 직후 호출하는 초기화 메서드
    /// 체력과 공격력에 스테이지 배율을 적용하고 플레이어 Transform을 탐색한다
    /// "Player" 태그로 플레이어를 탐색하므로 플레이어 오브젝트에 태그 설정이 필요하다
    /// </summary>
    /// <param name="hpMultiplier">보스 체력 배율 (CStageData._bossHpMultiplier)</param>
    /// <param name="atkMultiplier">보스 공격력 배율 (CStageData._bossAtkMultiplier)</param>
    /// <param name="playerTransform">플레이어 Transform — CBossManager에서 직접 주입</param>
    public void Initialize(float hpMultiplier, float atkMultiplier, Transform playerTransform)
    {
        maxHp     = _baseHp  * hpMultiplier;  // 스테이지 배율 적용 체력
        atk       = _baseAtk * atkMultiplier; // 스테이지 배율 적용 공격력
        currentHp = maxHp;

        if (playerTransform != null)
        {
            target             = playerTransform;
            _playerDamageable  = playerTransform.GetComponent<IDamageable>();
        }
    }

    /// <summary>
    /// 외부에서 호출하는 피격 처리 메서드
    /// 체력이 0 이하가 되면 즉시 사망 처리하며 중복 사망을 방지한다
    /// </summary>
    /// <param name="damage">받는 피해량</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;
        if (currentHp <= 0f) Die();
    }

    /// <summary>
    /// 피격 방향 정보를 포함한 피격 처리 메서드 — IDamageable 인터페이스 구현
    /// 체력 차감은 기존 TakeDamage(float)에 위임하고, 데미지 텍스트 연출을 추가로 발생시킨다
    /// CBoss는 CEntityBase를 상속하지 않으므로 HitFlash 연출은 생략한다
    /// </summary>
    /// <param name="damage">받는 피해량</param>
    /// <param name="hitDir">피격 방향 벡터 — 데미지 텍스트 오프셋 방향 결정에 사용</param>
    public void TakeDamage(float damage, Vector2 hitDir)
    {
        TakeDamage(damage);
        CDamageTextPoolManager.Instance?.ShowDamage((int)damage, transform.position, hitDir);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 플레이어 방향으로 이동한다
    /// Rigidbody2D.velocity를 직접 제어하여 물리 충돌을 유지하면서 추적한다
    /// </summary>
    private void Chase()
    {
        Vector2 dir = ((Vector2)target.position - rb.position).normalized; // 플레이어 방향 단위 벡터
        rb.velocity = dir * _moveSpeed;
    }

    /// <summary>
    /// 공격 범위 내에서 쿨타임마다 플레이어에게 데미지를 입힌다
    /// 공격 중에는 이동을 정지하여 보스가 플레이어를 관통하는 상황을 방지한다
    /// 플레이어가 사망하면 OnPlayerKilled 이벤트를 발행하고 공격을 중단한다
    /// </summary>
    private void Attack()
    {
        rb.velocity  = Vector2.zero; // 공격 중 이동 정지
        attackTimer += Time.fixedDeltaTime;

        if (attackTimer < _attackInterval) return; // 쿨타임 대기

        attackTimer = 0f;

        _playerDamageable?.TakeDamage(atk);

        // 플레이어 사망 시 이벤트 발행 — CStageManager가 사망 UI 표시 후 리스폰 처리
        CEntityBase playerEntity = target.GetComponent<CEntityBase>();
        if (playerEntity != null && playerEntity.CurrentHealth <= 0f)
            OnPlayerKilled?.Invoke();
    }

    /// <summary>
    /// 보스 사망 처리를 수행한다
    /// 이동을 정지하고 isDead 플래그를 세운 뒤 OnDefeated 이벤트를 발행한다
    /// 실제 오브젝트 파괴는 CBossManager.CleanUpBoss에서 처리한다
    /// </summary>
    public void Die()
    {
        isDead      = true;
        rb.velocity = Vector2.zero;
        OnDefeated?.Invoke(); // CBossManager가 구독하여 클린업 및 StageClear 처리
    }

    #endregion

    #region Gizmos

    /// <summary>
    /// 씬 뷰에서 보스의 공격 범위를 시각화한다
    /// 보스 위치를 기준으로 초록색 원을 그려 공격 반경을 확인할 수 있다
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _attackRange); // 공격 범위 시각화
    }

    #endregion
}
