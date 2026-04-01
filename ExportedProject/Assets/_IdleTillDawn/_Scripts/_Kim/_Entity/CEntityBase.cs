using System;
using System.Collections;
using UnityEngine;

// TODO : 빙결, 화상, 넉백 상태 플래그
[Flags]
public enum EStatusEffect
{
    None = 0,
    Freeze = 1 << 0,
    Burn = 1 << 1,
    Knockback = 1 << 2,
}

[RequireComponent(typeof(Rigidbody2D))]
public abstract class CEntityBase : MonoBehaviour, IDamageable
{

    #region 인스펙터
    [Header("테스트 용 설정")]
    [SerializeField] private bool _isPersonalScene = true;

    [Header("공통 스탯")]
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _currentHealth;
    [SerializeField] private float _moveSpeed;

    [Header("타겟 탐지 설정")]
    [SerializeField] private float _scanRadius = 10f;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private float _scanInterval = 0.2f;

    [Header("엔티티 정보")]
    [SerializeField] private string _entityName;
    [SerializeField] private string _description;

    [Header("물리 설정")]
    [SerializeField] private float _defaultGravityScale = 0f;
    #endregion

    #region 내부 변수
    public event Action<float, float> OnHealthChanged;

    private Rigidbody2D _rb;
    private Transform _currentTarget;
    private float _scanTimer = 0f;
    private Collider2D[] _targetColliders = new Collider2D[50];

    private Coroutine _burnCoroutine;
    private Coroutine _freezeCoroutine;
    private float _currentFreezeSlowRate = 0f;
    private Coroutine _knockbackCoroutine;
    #endregion

    #region 프로퍼티
    public float MaxHealth
    {
        get => _maxHealth;
        protected set => _maxHealth = value;
    }
    public float MoveSpeed
    {
        get => _moveSpeed;
        protected set => _moveSpeed = value;
    }
    public float CurrentHealth
    {
        get => _currentHealth;
        protected set
        {
            _currentHealth = Mathf.Clamp(value, 0f, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
    }

    public virtual float CurrentMoveSpeed
    {
        get
        {
            float finalSpeed = MoveSpeed;

            if (HasStatus(EStatusEffect.Freeze))
            {
                finalSpeed *= _currentFreezeSlowRate;
            }

            return finalSpeed;
        }
    }

    public string EntityName => _entityName;
    public string Description => _description;
    public float DefaultGravityScale
    {
        get => _defaultGravityScale;
        protected set => _defaultGravityScale = value;
    }

    public Rigidbody2D Rb => _rb;
    public Transform CurrentTarget
    {
        get => _currentTarget;
        set => _currentTarget = value;
    }

    public bool IsPersonalScene => _isPersonalScene;

    public EStatusEffect CurrentStatus { get; protected set; } = EStatusEffect.None;

    protected float CurrentFreezeSlowRate => _currentFreezeSlowRate;
    #endregion

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _rb.gravityScale = _defaultGravityScale;
        _rb.freezeRotation = true;
    }

    protected virtual void FixedUpdate()
    {
        HandleMovement();
        HandleAttack();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _scanRadius);
    }

    /// <summary>
    /// 움직임 제어
    /// </summary>
    protected abstract void HandleMovement();
    /// <summary>
    /// 공격 제어
    /// </summary>
    protected abstract void HandleAttack();

    public virtual void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        //Debug.Log($"[{gameObject.name}] [데미지 : {damage}, 현재 체력 : {CurrentHealth}]");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Debug.Log($"{gameObject.name} 사망");
        Destroy(gameObject);
    }

    /// <summary>
    /// 가장 가까운 적을 찾는 메서드
    /// </summary>
    protected void FindNearestTarget()
    {
        _scanTimer += Time.fixedDeltaTime;

        if (_scanTimer < _scanInterval) return;
        _scanTimer = 0f;

        int count = Physics2D.OverlapCircleNonAlloc(transform.position, _scanRadius, _targetColliders, _targetLayer);

        float shortestDistanceSqr = Mathf.Infinity;
        Transform nearestTarget = null;

        for (int i = 0; i < count; i++)
        {
            Collider2D targetCollider = _targetColliders[i];

            Vector2 offset = targetCollider.transform.position - transform.position;
            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance < shortestDistanceSqr)
            {
                shortestDistanceSqr = sqrDistance;
                nearestTarget = targetCollider.transform;
            }
        }

        _currentTarget = nearestTarget;
    }

    /// <summary>
    /// 캐릭터 좌우 반전
    /// </summary>
    /// <param name="velocityX"></param>
    protected void FlipCharacter(float velocityX)
    {
        if (velocityX > 0.001f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (velocityX < -0.001f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    #region 상태이상 관리
    /// <summary>
    /// 상태이상 추가
    /// </summary>
    /// <param name="effect"></param>
    public void AddStatus(EStatusEffect effect)
    {
        CurrentStatus |= effect;
    }

    /// <summary>
    /// 상태이상 제거
    /// </summary>
    /// <param name="effect"></param>
    public void RemoveStatus(EStatusEffect effect)
    {
        CurrentStatus &= ~effect;
    }

    /// <summary>
    /// 특정 상태이상에 걸려있는지 확인
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public bool HasStatus(EStatusEffect effect)
    {
        return (CurrentStatus & effect) == effect;
    }
    #endregion

    #region 상태이상 기능
    /// <summary>
    /// 화상을 적용하는 함수
    /// </summary>
    /// <param name="duration">총 지속 시간</param>
    /// <param name="tickDamage">틱당 데미지</param>
    /// <param name="tickInterval">데미지가 들어가는 간격</param>
    public virtual void ApplyBurn(float duration, float tickDamage, float tickInterval)
    {
        if (_burnCoroutine != null)
        {
            StopCoroutine(_burnCoroutine);
        }

        _burnCoroutine = StartCoroutine(CoBurnRoutine(duration, tickDamage, tickInterval));
    }

    private IEnumerator CoBurnRoutine(float duration, float tickDamage, float tickInterval)
    {
        AddStatus(EStatusEffect.Burn);

        float timer = 0f;

        while (timer < duration)
        {
            TakeDamage(tickDamage);

            if (CurrentHealth <= 0f) break;

            yield return new WaitForSeconds(tickInterval);
            timer += tickInterval;
        }

        RemoveStatus(EStatusEffect.Burn);
        _burnCoroutine = null;
    }

    /// <summary>
    /// 빙결을 적용하는 함수
    /// </summary>
    /// <param name="duration">총 지속 시간</param>
    /// <param name="slowAmount">둔화율 (30f면 30% 속도로 느려짐)</param>
    public virtual void ApplyFreeze(float duration, float slowAmount)
    {
        if (_freezeCoroutine != null)
        {
            StopCoroutine(_freezeCoroutine);
        }

        _freezeCoroutine = StartCoroutine(CoFreezeRoutine(duration, slowAmount));
    }

    private IEnumerator CoFreezeRoutine(float duration, float slowAmount)
    {
        AddStatus(EStatusEffect.Freeze);

        _currentFreezeSlowRate = slowAmount / 100f;

        yield return new WaitForSeconds(duration);

        RemoveStatus(EStatusEffect.Freeze);
        _currentFreezeSlowRate = 1f;
        _freezeCoroutine = null;
    }

    /// <summary>
    /// 넉백을 적용하는 함수
    /// </summary>
    /// <param name="force">밀리는 힘</param>
    /// <param name="duration">넉백 지속 시간</param>
    public virtual void ApplyKnockback(Vector2 force, float duration)
    {
        if (HasStatus(EStatusEffect.Knockback)) return;

        _knockbackCoroutine = StartCoroutine(CoKnockbackRoutine(force, duration));
    }

    private IEnumerator CoKnockbackRoutine(Vector2 force, float duration)
    {
        AddStatus(EStatusEffect.Knockback);

        Rb.velocity = force;

        yield return new WaitForSeconds(duration);

        RemoveStatus(EStatusEffect.Knockback);
        Rb.velocity = Vector2.zero;
        _knockbackCoroutine = null;
    }

    public virtual void ClearAllStatuses()
    {
        CurrentStatus = EStatusEffect.None;
        _currentFreezeSlowRate = 1f;

        if (_burnCoroutine != null)
        {
            StopCoroutine(_burnCoroutine);
            _burnCoroutine = null;
        }

        if (_freezeCoroutine != null)
        {
            StopCoroutine(_freezeCoroutine);
            _freezeCoroutine = null;
        }

        if (_knockbackCoroutine != null)
        {
            StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = null;
        }
    }
    #endregion
}
