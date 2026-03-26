using System;
using UnityEngine;

// TODO : 빙결, 화상, 넉백 상태 플래그

[RequireComponent(typeof(Rigidbody2D))]
public abstract class CEntityBase : MonoBehaviour, IDamageable
{
    #region 인스펙터
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
    public string EntityName => _entityName;
    public string Description => _description;
    public float DefaultGravityScale
    {
        get => _defaultGravityScale;
        protected set => _defaultGravityScale = value;
    }


    protected Rigidbody2D Rb => _rb;
    protected Transform CurrentTarget
    {
        get => _currentTarget;
        set => _currentTarget = value;
    }
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
        Debug.Log($"[{gameObject.name}] [데미지 : {damage}, 현재 체력 : {CurrentHealth}]");

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
}
