using System.Collections.Generic;
using UnityEngine;

public class CSkillAreaEffect : MonoBehaviour, ISkill
{
    [Header("지속 피해 간격")]
    public float damageInterval = 1f;
    [Header("레벨에 따른 스케일 조정 여부")]
    public bool useScaleMagnification = true;
    public float scalePreset = 1f;
    public LayerMask enemyLayer;


    private float _damage;
    private float _lvMagnification;
    private int _level;
    private float _timer = 1f;  // 인터벌용 타이머

    // 현재 범위 안에 있는 적 목록 — Enter/Exit로 관리
    private readonly HashSet<IDamageable> _targetsInRange = new HashSet<IDamageable>();

    /// <summary>
    /// 스킬 시스템에서 생성과 동시에 호출 (의존성 분리)
    /// </summary>
    public void Init(float damage, int level)
    {
        _level = level;

        _damage = damage;
        _lvMagnification = 1f + (_level - 1) * 0.1f;

        if (useScaleMagnification)
            transform.localScale = Vector3.one * scalePreset * _lvMagnification;

        _timer = damageInterval;
    }

    private void Update()
    {
        // 스킬 지속 피해 쿨타임
        _timer += Time.deltaTime;
        if (_timer >= damageInterval)
        {
            _timer -= damageInterval;
            DamageAll();
        }
    }

    /// <summary>
    /// 인터벌마다 범위 안 모든 적에게 동시 데미지
    /// 사망한 적은 삭제 처리
    /// </summary>
    private void DamageAll()
    {
        _targetsInRange.RemoveWhere
            (t =>
                t == null ||
                (t is MonoBehaviour mb && !mb.gameObject.activeInHierarchy)
            );

        foreach (IDamageable target in _targetsInRange)
        {
            // 범위 이펙트는 발사 방향이 없으므로 이펙트 중심→적 방향을 hitDir로 사용
            // MonoBehaviour 캐스팅으로 위치를 얻어 자연스러운 텍스트 오프셋 방향을 결정한다
            Vector2 hitDir = Vector2.up; // 위치 취득 실패 시 기본값
            if (target is MonoBehaviour mb)
                hitDir = ((Vector2)(mb.transform.position - transform.position)).normalized;

            target.TakeDamage(_damage * _lvMagnification, hitDir);
        }            
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
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
