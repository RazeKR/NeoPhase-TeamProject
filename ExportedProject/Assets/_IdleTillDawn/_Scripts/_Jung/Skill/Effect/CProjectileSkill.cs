using System.Collections.Generic;
using UnityEngine;

public class CProjectileSkill : MonoBehaviour, ISkill
{
    [Header("투사체 옵션")]
    public float speed = 10f;
    public float lifeTime = 5f;
    /// <summary>충돌 시 피해 여부입니다. 다른 ISkill과 혼용 시 false할 수 있습니다.</summary>
    public bool damagable = true;

    [Header("충돌 설정")]
    /// <summary>충돌 시 즉시 삭제 여부입니다.</summary>
    public bool destroyOnHit = true;
    /// <summary>충돌 이후 생성되는 이펙트입니다. 충돌 이펙트에도 ISkill이 포함될 수 있습니다.</summary>
    public GameObject effectPrefab;

    private float _damage;
    private float _lvMagnification;
    private int _level;

    /// <summary>이미 데미지를 입은 적 저장 목록</summary>
    private HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

    public void Init(float damage, int level)
    {
        _level = level;
        _damage = damage;
        _lvMagnification = (1f + (_level - 1) * 0.1f);

        Destroy(gameObject, lifeTime);
    }


    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;

        if (_hitTargets.Contains(target)) return;

        if (damagable)
        {
            target?.TakeDamage(_damage * _lvMagnification);
            _hitTargets.Add(target);
        }            

        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);

            if (effect.TryGetComponent(out ISkill skillLogic))
            {
                skillLogic.Init(_damage, _level);
            }
        }

        if (destroyOnHit) Destroy(gameObject);
    }
}
