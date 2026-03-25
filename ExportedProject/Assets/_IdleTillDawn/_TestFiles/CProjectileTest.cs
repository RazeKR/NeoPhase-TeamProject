using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CProjectileTest : MonoBehaviour
{
    #region 인스펙터
    [Header("투사체 속도")]
    [SerializeField] private float _speed = 8f;
    #endregion

    #region 내부 변수
    private float _damage;
    private Vector2 _direction;
    #endregion

    public void Init(float damage, Vector2 direction)
    {
        _damage = damage;
        _direction = direction;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = _direction * _speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
