using System.Collections.Generic;
using UnityEngine;

// 스킬 생성 후 매니저로부터 스킬 레벨 데이터를 받아와 데미지 적용

public class CSkillWindborne : MonoBehaviour
{
    [SerializeField] private CSkillDataSO _so;
    [SerializeField] private float _lifetime      = 5f;
    [SerializeField] private float _damageInterval = 1f;

    private float _level;
    private float _damage;
    private float _currentCool = 0f;

    // 현재 범위 안에 있는 적 목록 — Enter/Exit로 관리
    private readonly HashSet<IDamageable> _targetsInRange = new HashSet<IDamageable>();

    private void Start()
    {
        LoadData();
    }

    private void Update()
    {
        if (_lifetime > 0f)
        {
            _lifetime     -= Time.deltaTime;
            _currentCool  -= Time.deltaTime;

            if (_currentCool <= 0f)
            {
                _currentCool += _damageInterval;
                DamageAll();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadData()
    {
        if (_so == null) { enabled = false; return; }

        _level  = CSkillManager.Instance.GetSkillLevel(_so.Id);

        float lvMagnifcation = 1f + _level * 0.1f;

        _damage = _so.damage * lvMagnifcation;

        Debug.Log(_level);

        gameObject.transform.localScale = Vector3.one * lvMagnifcation;
    }

    /// <summary>
    /// 인터벌마다 범위 안 모든 적에게 동시 데미지
    /// </summary>
    private void DamageAll()
    {
        _targetsInRange.RemoveWhere(t => t == null); // 사망으로 제거된 적 정리

        foreach (IDamageable target in _targetsInRange)
            target.TakeDamage(_damage);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<CEnemyBase>() == null) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
            _targetsInRange.Add(damageable);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
            _targetsInRange.Remove(damageable);
    }
}
