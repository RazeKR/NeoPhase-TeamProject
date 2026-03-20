using UnityEngine;

public abstract class CEnemyBase : CEntityBase
{
    #region 인스펙터
    [Header("적 데이터")]
    [SerializeField] protected CEnemyDataSO _enemyData;
    #endregion

    #region 내부 변수
    protected float _attackDamage;
    protected float _attackCooltime;
    protected float _attackRange;

    protected float _lastAttackTime = 0f;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        // 임시, 추후에 삭제
        InitEnemy(1);
    }

    protected virtual void Start()
    {
        if (_enemyData != null)
        {
            
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} EnemyDataSO 없음. 참조 확인");
            return;
        }
    }

    /// <summary>
    /// 적 스폰 시 스테이지에 맞게 스탯 초기화
    /// </summary>
    /// <param name="currentStage">실제 스테이지</param>
    public virtual void InitEnemy(int currentStage)
    {
        if (_enemyData == null) return;

        _moveSpeed = _enemyData.MoveSpeed;
        _attackRange = _enemyData.AttackRange;
        _attackCooltime = _enemyData.AttackCooltime;

        int growthMultiplier = Mathf.Max(1, currentStage) - 1;

        float scaledHealth = _enemyData.BaseHealth + (_enemyData.HealthGrowthPerStage * growthMultiplier);
        _maxHealth = scaledHealth;
        _currentHealth = scaledHealth;

        _attackDamage = _enemyData.BaseDamage + (_enemyData.DamageGrowthPerStage * growthMultiplier);
    }

    protected override void HandleMovement()
    {
        if (_currentTarget == null)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, _currentTarget.position);
        Vector2 dirToPlayer = (_currentTarget.position - transform.position).normalized;

        FlipCharacter(dirToPlayer.x);

        if (distance > _attackRange)
        {
            _rb.velocity = dirToPlayer * _moveSpeed;
        }
        else
        {
            _rb.velocity = Vector2.zero;
        }
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
    /// 실제 공격 실행
    /// </summary>
    protected abstract void ExecuteAttack();
}
