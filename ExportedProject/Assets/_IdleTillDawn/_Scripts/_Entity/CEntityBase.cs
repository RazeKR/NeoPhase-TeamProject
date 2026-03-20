using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class CEntityBase : MonoBehaviour, IDamageable
{
    #region 인스펙터
    [Header("공통 스탯")]
    [SerializeField] protected float _maxHealth;
    [SerializeField] protected float _currentHealth;
    [SerializeField] protected float _moveSpeed;

    [Header("타겟 탐지 설정")]
    [SerializeField] protected float _scanRadius = 10f;
    [SerializeField] protected LayerMask _targetLayer;
    [SerializeField] protected float _scanInterval = 0.2f;

    [Header("엔티티 정보")]
    [SerializeField] protected string _entityName;
    [SerializeField] protected string _description;

    [Header("물리 설정")]
    [SerializeField] protected float _defaultGravityScale = 0f;
    #endregion

    #region 내부 변수
    public float CurrentHealth => _currentHealth;
    public string EntityName => _entityName;

    protected Rigidbody2D _rb;
    protected Transform _currentTarget;
    protected float _scanTimer = 0f;
    #endregion

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _rb.gravityScale = _defaultGravityScale;
        _rb.freezeRotation = true;
    }

    protected virtual void FixedUpdate()
    {
        FindNearestTarget();

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
        _currentHealth -= damage;
        Debug.Log($"[{gameObject.name}] [데미지 : {damage}, 현재 체력 : {_currentHealth}]");

        if (_currentHealth <= 0)
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

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _scanRadius, _targetLayer);

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Collider2D collider in colliders)
        {
            float distance = Vector2.Distance(transform.position, collider.transform.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = collider.transform;
            }
        }

        _currentTarget = nearestEnemy;
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
