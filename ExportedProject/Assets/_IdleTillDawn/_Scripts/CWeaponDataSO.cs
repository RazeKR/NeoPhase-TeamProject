using UnityEngine;


[CreateAssetMenu(menuName = "SO/Data/WeaponData", fileName = "WeaponData_")]
public class CWeaponDataSO : CItemDataSO
{
    [SerializeField] private int _weaponDamage = 0;
    [SerializeField] private float _weaponFireRate = 1.0f;
    [SerializeField] private float _weaponRange = 5.0f;
    [SerializeField] private GameObject _bulletPrefab = null;
    [SerializeField] private float _lifeTime = 1.0f;
    [SerializeField] private float _damagePerRank = 1.5f;

    public int WeaponDamage => _weaponDamage;           // 무기 데미지 (이후 투사체에서 접근)
    public float WeaponFireRate => _weaponFireRate;     // 무기 연사속도 (이후 플레이어 공격과 연동)
    public GameObject BulletPrefab => _bulletPrefab;    // 발사체 프리팹
    public float WeaponRange => _weaponRange;           // 무기 사거리
    public float LifeTime => _lifeTime;                 // 발사체 잔류시간 (필요 시 사용하고, 미사용 시 삭제)
    public float DamagePerRank => _damagePerRank;       // 랭크에 따른 데미지 배율


    // 데이터 확인
    public bool IsValid(out string reason)
    {
        if (string.IsNullOrEmpty(_itemId))
        {
            reason = "_itemId 비어있음";
            return false;
        }

        if (string.IsNullOrEmpty(_itemName))
        {
            reason = "_itemName 비어있음";
            return false;
        }

        if (_sprite == null)
        {
            reason = "_sprite 비어있음";
            return false;
        }

        if (_bulletPrefab == null)
        {
            reason = "_bullet 비어있음";
            return false;
        }

        reason = "이상 없음";
        return true;
    }
}
