using UnityEngine;


[CreateAssetMenu(menuName = "SO/Data/WeaponData", fileName = "WeaponData_")]
public class CWeaponDataSO : CItemDataSO
{
    [SerializeField] private int _weaponDamage = 0;
    /// <summary>초당 발사 횟수. 1=1발/초, 3=3발/초, 10=10발/초 (최대 50, FixedUpdate 주기 상한)</summary>
    [Range(0.1f, 50f)]
    [SerializeField] private float _weaponFireRate = 1.0f;
    [SerializeField] private float _weaponRange = 5.0f;
    [SerializeField] private GameObject _bulletPrefab = null;
    [SerializeField] private float _lifeTime = 1.0f;
    [SerializeField] private float _damagePerRank = 1.1f;

    public int WeaponDamage => _weaponDamage;           // ???? ?????? (???? ????????? ????)
    public float WeaponFireRate => _weaponFireRate;     // ???? ?????? (???? ?÷???? ????? ????)
    public GameObject BulletPrefab => _bulletPrefab;    // ???? ??????
    public float WeaponRange => _weaponRange;           // ???? ????
    public float LifeTime => _lifeTime;                 // ???? ????ð? (??? ?? ??????, ???? ?? ????)
    public float DamagePerRank => _damagePerRank;       // ????? ???? ?????? ????


    // ?????? ???
    public bool IsValid(out string reason)
    {
        if (string.IsNullOrEmpty(_itemId))
        {
            reason = "_itemId ???????";
            return false;
        }

        if (string.IsNullOrEmpty(_itemName))
        {
            reason = "_itemName ???????";
            return false;
        }

        if (_sprite == null)
        {
            reason = "_sprite ???????";
            return false;
        }

        if (_bulletPrefab == null)
        {
            reason = "_bullet ???????";
            return false;
        }

        reason = "??? ????";
        return true;
    }
}
