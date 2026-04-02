using System.Collections.Generic;
using UnityEngine;

public class CProjectileSkill : MonoBehaviour, ISkill
{
    [Header("투사체 옵션")]
    public bool damagable = true;
    public bool movable = true;

    [Header("회전값 고정 이미지(자식 오브젝트 연결)")]
    public Transform _visualChild;

    [Header("온힛 설정")]
    public bool destroyOnHit = true;
    public GameObject effectPrefab;
    public LayerMask enemyLayer;

    private float _damage;
    private int _level;
    private CSkillDataSO _data;

    private HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

    public void Init(CSkillDataSO data, int level)
    {
        _damage = data.ActiveLevelDatas[level].damage;

        _data = data;

        Destroy(gameObject, data.lifeTime);
    }


    private void Update()
    {
        if (!movable || _data == null) return;

        transform.Translate(Vector3.right * _data.speed * Time.deltaTime);
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
                // transform.right : 투사체의 월드 기준 진행 방향을 hitDir로 전달하여 HitFlash·데미지텍스트 연출 활성화
                target.TakeDamage(_damage, transform.right);

                if (_data.useSkillEffect)
                {
                    switch (_data.skillEffect.type)
                    {
                        case EEffectType.Burn:
                            (target as CEntityBase).ApplyBurn(_data.skillEffect.duration, _data.skillEffect.value, 1f);
                            break;

                        case EEffectType.Freeze:
                            (target as CEntityBase).ApplyFreeze(_data.skillEffect.duration, _data.skillEffect.value);
                            break;
                    }
                }

                _hitTargets.Add(target);
            }

            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);

                if (effect.TryGetComponent(out ISkill skillLogic))
                {
                    skillLogic.Init(_data, _level);
                }
            }

            if (destroyOnHit)
            {
                StopParticleAndDestroy();
            }
        }
    }

    /// <summary>
    /// 기존 삭제와 더불어, 파티클이 있다면 파티클 개별 처리 후 삭제
    /// </summary>
    private void StopParticleAndDestroy()
    {
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();

        if (ps != null)
        {
            ps.transform.SetParent(null);

            var emission = ps.emission;
            emission.enabled = false;
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        Destroy(gameObject);
    }
}
