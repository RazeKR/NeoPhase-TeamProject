using UnityEngine;


[CreateAssetMenu(menuName = "SO/Data/WeaponData", fileName = "WeaponData_")]
public class CWeaponDataSO : CItemDataSO
{
    [SerializeField] private int _weaponDamage = 0;
    /// <summary>�ʴ� �߻� Ƚ��. 1=1��/��, 3=3��/��, 10=10��/�� (�ִ� 50, FixedUpdate �ֱ� ����)</summary>
    [Range(0.1f, 50f)]
    [SerializeField] private float _weaponFireRate = 1.0f;
    [SerializeField] private float _weaponRange = 5.0f;
    [SerializeField] private GameObject _bulletPrefab = null;
    [SerializeField] private float _lifeTime = 1.0f;
    [SerializeField] private float _damagePerRank = 1.1f;
    [Tooltip("체크하면 근접무기로 분류됩니다. 총구화염이 발생하지 않습니다.")]
    [SerializeField] private bool _isMelee = false;

    public int WeaponDamage => _weaponDamage;           // ???? ?????? (???? ????????? ????)
    public float WeaponFireRate => _weaponFireRate;     // ???? ?????? (???? ?��???? ????? ????)
    public GameObject BulletPrefab => _bulletPrefab;    // ???? ??????
    public float WeaponRange => _weaponRange;           // ???? ????
    public float LifeTime => _lifeTime;                 // ???? ????��? (??? ?? ??????, ???? ?? ????)
    public float DamagePerRank => _damagePerRank;       // ????? ???? ?????? ????
    public bool IsMelee => _isMelee;                    // 근접무기 여부


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
