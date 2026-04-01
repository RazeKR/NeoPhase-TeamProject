using System.Collections.Generic;
using UnityEngine;

public class CProjectileSkill : MonoBehaviour, ISkill
{
    [Header("����ü �ɼ�")]
    public float speed = 10f;
    public float lifeTime = 5f;
    /// <summary>�浹 �� ���� �����Դϴ�. �ٸ� ISkill�� ȥ�� �� false�� �� �ֽ��ϴ�.</summary>
    public bool damagable = true;

    [Header("�浹 ����")]
    /// <summary>�浹 �� ��� ���� �����Դϴ�.</summary>
    public bool destroyOnHit = true;
    /// <summary>�浹 ���� �����Ǵ� ����Ʈ�Դϴ�. �浹 ����Ʈ���� ISkill�� ���Ե� �� �ֽ��ϴ�.</summary>
    public GameObject effectPrefab;
    public LayerMask enemyLayer;

    private float _damage;
    private float _lvMagnification;
    private int _level;

    /// <summary>�̹� �������� ���� �� ���� ���</summary>
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
        if (other.gameObject.layer != enemyLayer) return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;


        if (_hitTargets.Contains(target)) return;

        if (damagable)
        {
            // transform.right : 투사체의 월드 기준 진행 방향을 hitDir로 전달하여 HitFlash·데미지텍스트 연출 활성화
            target?.TakeDamage(_damage * _lvMagnification, transform.right);
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
