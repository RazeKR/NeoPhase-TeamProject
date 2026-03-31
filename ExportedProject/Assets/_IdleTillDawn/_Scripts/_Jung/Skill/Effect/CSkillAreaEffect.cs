using System.Collections.Generic;
using UnityEngine;

// 스킬 생성 후 매니저로부터 스킬 레벨 데이터를 받아와 데미지 적용

public class CSkillAreaEffect : MonoBehaviour, ISkill
{
    [Header("지속 피해 간격")]
    [SerializeField] private float _damageInterval = 1f;
    [Header("레벨에 따른 투사체 스케일 조정")]
    [SerializeField] private bool _useScaleMagnification = false;

    private int _level;
    private float _damage;
    private float _timer = 0f;  // 인터벌용 타이머

    // 현재 범위 안에 있는 적 목록 — Enter/Exit로 관리
    private readonly HashSet<IDamageable> _targetsInRange = new HashSet<IDamageable>();

    /// <summary>
    /// 스킬 시스템에서 생성과 동시에 호출 (의존성 분리)
    /// </summary>
    public void Init(float damage, int level)
    {
        _level = level;
        float lvMagnification = 1f + (_level - 1) * 0.1f;

        _damage = damage * lvMagnification;

        if (_useScaleMagnification)
            transform.localScale = Vector3.one * lvMagnification;

        _timer = _damageInterval;
    }

    private void Update()
    {
        // 스킬 지속 피해 쿨타임
        _timer += Time.deltaTime;
        if (_timer >= _damageInterval)
        {
            _timer = 0f;
            DamageAll();
        }
    }

    /// <summary>
    /// 인터벌마다 범위 안 모든 적에게 동시 데미지
    /// 사망한 적은 삭제 처리
    /// </summary>
    private void DamageAll()
    {
        // 사망 타겟 제거
        _targetsInRange.RemoveWhere
            (
               t => 
               t == null 
               || (t as MonoBehaviour) == null
               || !(t as MonoBehaviour).gameObject.activeInHierarchy
            );

        foreach (IDamageable target in _targetsInRange)
        {
            target.TakeDamage(_damage);
        }            
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //if (other.GetComponentInParent<CEnemyBase>() == null) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && other.CompareTag("Enemy"))
        {
            _targetsInRange.Add(damageable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            _targetsInRange.Remove(damageable);
        }            
    }
}
