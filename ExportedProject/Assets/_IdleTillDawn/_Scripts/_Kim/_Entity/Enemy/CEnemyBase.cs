using System;
using UnityEngine;

public abstract class CEnemyBase : CEntityBase
{
    #region 인스펙터
    [Header("적 데이터")]
    [SerializeField] protected CEnemyDataSO _enemyData;
    #endregion

    #region 이벤트
    /// <summary>
    /// 사망 시 발행 — CSpawnManager가 구독하여 풀 반환 처리
    /// </summary>
    public event Action<CEnemyBase> OnDied;
    #endregion

    #region 내부 변수
    protected float _attackDamage;
    protected float _attackCooltime;
    protected float _attackRange;

    protected float    _lastAttackTime = 0f;
    private   Transform _playerTransform;   // 스폰 시 주입된 플레이어 Transform
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 스캔 없이 플레이어를 직접 타겟으로 설정 — CSpawnManager가 스폰 시 호출
    /// </summary>
    public void SetTarget(Transform player)
    {
        _playerTransform = player;
        _currentTarget   = player;
    }

    /// <summary>
    /// 스캔 방식 대신 주입된 플레이어 Transform을 항상 타겟으로 사용
    /// </summary>
    protected override void FixedUpdate()
    {
        _currentTarget = _playerTransform; // 스캔 없이 직접 추적
        HandleMovement();
        HandleAttack();
    }

    protected virtual void Start()
    {
        if (_enemyData == null)
            Debug.LogWarning($"{gameObject.name} EnemyDataSO 없음. 참조 확인");
    }

    /// <summary>
    /// 스폰 시 스테이지에 맞게 스탯 초기화
    /// </summary>
    /// <param name="currentStage">현재 스테이지 (1-based)</param>
    public virtual void InitEnemy(int currentStage)
    {
        if (_enemyData == null) return;

        _moveSpeed      = _enemyData.MoveSpeed;
        _attackRange    = _enemyData.AttackRange;
        _attackCooltime = _enemyData.AttackCooltime;

        int growthMultiplier = Mathf.Max(1, currentStage) - 1;

        float scaledHealth = _enemyData.BaseHealth + (_enemyData.HealthGrowthPerStage * growthMultiplier);
        _maxHealth    = scaledHealth;
        _currentHealth = scaledHealth;

        _attackDamage = _enemyData.BaseDamage + (_enemyData.DamageGrowthPerStage * growthMultiplier);
    }

    /// <summary>
    /// 풀 반환 시 호출 — 상태 초기화
    /// </summary>
    public virtual void ResetForPool()
    {
        _currentTarget  = null;
        _scanTimer      = 0f;
        _lastAttackTime = 0f;
        _rb.velocity    = Vector2.zero;
        _currentHealth  = _maxHealth;
    }

    /// <summary>
    /// 사망 처리 — Destroy 대신 OnDied 이벤트 발행, 실제 비활성화는 CSpawnManager가 처리
    /// </summary>
    public override void Die()
    {
        _rb.velocity = Vector2.zero;
        OnDied?.Invoke(this);
    }

    protected override void HandleMovement()
    {
        if (_currentTarget == null)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        float distance    = Vector2.Distance(transform.position, _currentTarget.position);
        Vector2 dirToPlayer = (_currentTarget.position - transform.position).normalized;

        FlipCharacter(dirToPlayer.x);

        if (distance > _attackRange)
            _rb.velocity = dirToPlayer * _moveSpeed;
        else
            _rb.velocity = Vector2.zero;
    }

    protected override void HandleAttack()
    {
        if (_currentTarget == null) return;

        float distance = Vector2.Distance(transform.position, _currentTarget.position);

        if (distance <= _attackRange && Time.time >= _lastAttackTime + _attackCooltime)
        {
            _lastAttackTime = Time.time;
            ExecuteAttack();
        }
    }

    /// <summary>
    /// 실제 공격 실행 — 서브클래스에서 구현
    /// </summary>
    protected abstract void ExecuteAttack();
}
