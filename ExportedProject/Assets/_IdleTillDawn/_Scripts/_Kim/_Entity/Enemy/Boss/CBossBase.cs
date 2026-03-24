using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class CBossBase : CEnemyBase
{
    #region 인스펙터
    [Header("애니메이터 설정")]
    [SerializeField] private Animator _animator;
    [SerializeField] private string _paramSpeed = "aSpeed";
    [SerializeField] private string _paramAttack = "tAttack";
    #endregion

    #region 내부 변수
    private int _hashSpeed;
    private int _hashAttack;
    private bool _hasAttackParam;

    private bool _isAttacking = false;

    private Coroutine _attackCoroutine;

    private CEntityBase _playerEntity;
    #endregion

    #region 이벤트
    public event Action OnDefeated;
    public event Action OnPlayerKilled;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        _hashSpeed = Animator.StringToHash(_paramSpeed);

        _hasAttackParam = !string.IsNullOrEmpty(_paramAttack);
        if (_hasAttackParam)
        {
            _hashAttack = Animator.StringToHash(_paramAttack);
        }
    }

    private void OnDisable()
    {
        StopAttack();
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, AttackRange); // 공격 범위 시각화
    }

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
        SetTarget(playerTransform);

        if (EnemyData != null)
        {
            MaxHealth = EnemyData.BaseHealth * hpMultiplier;
            CurrentHealth = MaxHealth;
            AttackDamage = EnemyData.BaseDamage * atkMultiplier;

            MoveSpeed = EnemyData.MoveSpeed;
            AttackRange = EnemyData.AttackRange;
            AttackCooltime = EnemyData.AttackCooltime;
        }

        if (playerTransform != null)
        {
            _playerEntity = playerTransform.GetComponent<CEntityBase>();
        }
    }

    public override void ResetForPool()
    {
        base.ResetForPool();

        OnDefeated?.Invoke();

        StopAttack();
    }

    public override void Die()
    {
        StopAttack();
        base.Die();
    }

    protected override void HandleMovement()
    {
        if (_isAttacking) return;

        base.HandleMovement();

        float speed = Rb.velocity.magnitude;

        _animator.SetFloat(_hashSpeed, speed);
    }

    protected override void HandleAttack()
    {
        base.HandleAttack();

        CheckPlayerKilled();
    }

    protected override void ExecuteAttack()
    {
        if (_isAttacking || Time.time < LastAttackTime + EnemyData.AttackCooltime) return;

        _attackCoroutine = StartCoroutine(CoAttackSequence());
    }

    protected void CheckPlayerKilled()
    {
        if (_playerEntity != null && _playerEntity.CurrentHealth <= 0)
        {
            OnPlayerKilled?.Invoke();
        }
    }

    protected virtual IEnumerator CoAttackSequence()
    {
        _isAttacking = true;
        _animator.SetTrigger(_hashAttack);

        yield return StartCoroutine(CoTelegraph());


        yield return StartCoroutine(CoProcessPattern());

        _isAttacking = false;
        LastAttackTime = Time.time;
    }

    protected virtual IEnumerator CoTelegraph()
    {
        yield return new WaitForSeconds(1f);
    }

    protected abstract IEnumerator CoProcessPattern();

    protected virtual void StopAttack()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }

        _isAttacking = false;

        Rb.velocity = Vector2.zero;
    }
}
