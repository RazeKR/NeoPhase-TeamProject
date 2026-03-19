using UnityEngine;

public abstract class CEntityBase : MonoBehaviour, IDamageable
{
    #region 인스펙터
    [Header("공통 스탯")]
    [SerializeField] protected float _currentHealth;
    [SerializeField] protected float _moveSpeed;
    #endregion

    #region 내부 변수
    public float CurrentHealth => _currentHealth;

    protected Rigidbody2D _rb;
    #endregion

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        HandleMovement();
        HandleAttack();
    }

    protected abstract void HandleMovement();
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
}
