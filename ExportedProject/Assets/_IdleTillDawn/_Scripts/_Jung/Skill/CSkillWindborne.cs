using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 스킬 사용 시 매니저로부터 스킬 레벨 정보를 받아와 정보를 수정

public class CSkillWindborne : MonoBehaviour
{
    [SerializeField] private CSkillDataSO _so;
    [SerializeField] private float _lifetime = 5f;
    [SerializeField] private float _damageInterval = 1f;

    private float _level;
    private float _damage;
    private bool _enableDamage = true;
    private float _currentCool = 0;


    private void Start()
    {
        LoadData();

    }

    private void Update()
    {
        if (_lifetime > 0)
        {
            _lifetime -= Time.deltaTime;
            _currentCool -= Time.deltaTime;

            if (_currentCool <= 0)
            {                
                _enableDamage = true;
                _currentCool += _damageInterval;
            }
        }

        else Destroy(gameObject);
    }

    private void LoadData()
    {
        if (_so == null) enabled = false;

        _level = CSkillManager.Instance.GetSkillLevel(_so.name);

        _damage = _so.damage * (1 + (_level * 0.1f));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<CEnemyBase>(out CEnemyBase e)) return;

        IDamageable damageable = e.GetComponent<IDamageable>();

        if (_enableDamage)
        {
            _enableDamage = false;
            Debug.Log("Damage" + _damage);
            damageable?.TakeDamage(_damage);
        }
    }
}
