using System.Collections;
using UnityEngine;

/// <summary>
/// 장착된 무기 정보를 가져와 스프라이트 이미지를 적용합니다.
/// </summary>

public class CWeaponEquip : MonoBehaviour
{
    #region SingleTon
    public static CWeaponEquip Instance { get; private set; }
    #endregion

    #region Inspectors & Private Variables

    [SerializeField] private GameObject _targetObject = null;   // 스프라이트 바꿀 타겟

    private string _currentInstanceID;
    private SpriteRenderer _targetSpriteRdr;
    private CItemDataSO _itemDataSO;

    #endregion




    #region UnityMethods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (_targetObject == null)
        {
            enabled = false;
        }

        bool getSpriteRenderer = _targetObject.TryGetComponent<SpriteRenderer>(out _targetSpriteRdr);

        if (!getSpriteRenderer)
        {
            enabled = false;
        }
    }

    private void Start()
    {
        LoadEquippedWeapon();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {            
            Instance = null;
        }
    }

    private void Update()
    {
        CWeaponInstance equipped = CInventorySystemJ.Instance.EquippedWeapon;
        if (equipped == null) return;

        if (_currentInstanceID != equipped._instanceID)
        {
            _currentInstanceID = equipped._instanceID;
            _itemDataSO = equipped._itemData as CWeaponDataSO;
            LoadEquippedWeapon();
        }

        else if (_itemDataSO != equipped._itemData)
        {
            LoadEquippedWeapon();
        }
    }

    #endregion

    #region PrivateMethods

    private void LoadEquippedWeapon()
    {
        CWeaponInstance weapon = CInventorySystemJ.Instance.EquippedWeapon;
        if (weapon == null || weapon._itemData == null) return;

        _itemDataSO = weapon._itemData;
        _targetSpriteRdr.sprite = weapon._itemData.ItemSprite;
    }

    private IEnumerator CoBulletLifeTime(GameObject a, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(a);
    }

    #endregion

    #region PublicMethods

    /// <summary>
    /// 무기의 반동 모션 혹은 휘두르기 모션을 재생합니다.
    /// </summary>
    public void WeaponRebound()
    {
        Animator anim = _targetObject.GetComponent<Animator>();
                
        if (_itemDataSO.Id == 3)
        {
            anim.Play("Swing", 0, 0f);
        }
        else
        {
            anim.Play("Fire", 0, 0f);
        }
    }

    public void GenerateBullet()
    {
        GameObject a = Instantiate((_itemDataSO as CWeaponDataSO).BulletPrefab);

        a.transform.position = _targetObject.transform.position + Vector3.right * 0.2f;

        Rigidbody2D rb = a.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.AddForce(a.transform.right * 10, ForceMode2D.Impulse);
        }

        StartCoroutine(CoBulletLifeTime(a, (_itemDataSO as CWeaponDataSO).LifeTime));
    }

    #endregion


    
}
