using System;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Data/WeaponData", fileName = "WeaponData_")]
public class CWeaponDataSO : CItemDataSO
{
    [SerializeField] private int[] _weaponDamage;
    [Range(0.1f, 50f)]
    [SerializeField] private float _weaponFireRate = 1.0f;
    [SerializeField] private float _weaponRange = 5.0f;
    [SerializeField] private GameObject _bulletPrefab = null;    
    [SerializeField] private float _lifeTime = 1.0f;
    [SerializeField] private float _damagePerRank = 1.1f;
    [Tooltip("체크하면 근접무기로 분류됩니다. 총구화염이 발생하지 않습니다.")]
    [SerializeField] private bool _isMelee = false;
    [Tooltip("Number of Projectile when shot.")]
    [SerializeField] private int _projectileAmount = 1;
    [SerializeField] private float _projectileSpeed = 10f;
    [Tooltip("For Melee Special Attack")]
    [SerializeField] private GameObject _meleeSpin = null;

    [Header("사운드")]
    [SerializeField] private CSoundData _fireSFX;  // 발사(공격) 시 재생 사운드


    public int[] WeaponDamage => _weaponDamage;
    public float WeaponFireRate => _weaponFireRate;
    public GameObject BulletPrefab => _bulletPrefab;
    public float WeaponRange => _weaponRange;
    public float LifeTime => _lifeTime;
    public float DamagePerRank => _damagePerRank;
    public bool IsMelee => _isMelee;                    // 근접무기 여부

    public GameObject MeleeSpin => _meleeSpin;

    public int ProjectileAmount => _projectileAmount;

    public float ProjectileSpeed => _projectileSpeed;

    /// <summary>발사(공격) 시 재생할 사운드 데이터. null이면 무음</summary>
    public CSoundData FireSFX => _fireSFX;
}