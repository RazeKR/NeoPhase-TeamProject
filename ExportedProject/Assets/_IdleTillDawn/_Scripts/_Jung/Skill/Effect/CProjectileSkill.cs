using System.Collections.Generic;
using UnityEngine;

public class CProjectileSkill : MonoBehaviour, ISkill
{
    [Header("투사체 옵션")]
    public float speed = 10f;
    public float lifeTime = 5f;
    public bool damagable = true;

    [Header("회전값 고정 이미지(자식 오브젝트 연결)")]
    public Transform _visualChild;

    [Header("온힛 설정")]
    public bool destroyOnHit = true;
    public GameObject effectPrefab;
    public LayerMask enemyLayer;

    private float _damage;
    private float _lvMagnification;
    private int _level;

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
        if (speed == 0f) return;
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (_visualChild != null)
        {
            _visualChild.rotation = Quaternion.identity;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            if (_hitTargets.Contains(target)) return;

            if (damagable)
            {
                Debug.Log("피해를 주었습니다.");
                // transform.right : 투사체의 월드 기준 진행 방향을 hitDir로 전달하여 HitFlash·데미지텍스트 연출 활성화
                target.TakeDamage(_damage * _lvMagnification, transform.right);
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

            if (destroyOnHit)
            {
                Debug.Log("투사체 파괴");
                Destroy(gameObject);
            }
        }        
    }
}
