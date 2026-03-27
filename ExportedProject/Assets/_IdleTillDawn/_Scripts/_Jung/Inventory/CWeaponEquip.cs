using System.Collections;
using UnityEngine;

/*
��CWeaponEquip
- �ν����ͷ� ������ ���� ��������Ʈ �������� ��ü���ִ� ���
   �� �ð������� �������� �ΰ��� ���⸸ �����ϴ� Ŭ����
- ���� �ݵ�/�ֵθ��� �Լ� WeaponRebound() ���ؼ� ���� ����
*/

public class CWeaponEquip : MonoBehaviour
{
    public static CWeaponEquip Instance { get; private set; }

    [SerializeField] private GameObject _targetObject = null;
    [SerializeField] private bool _showDebug = false;

    private string _currentInstanceID;              // ���� �������� ������ �ν��Ͻ�ID
    private SpriteRenderer _targetSpriteRdr;        // ���� ������ ���� �ٲ��� ��� ��������Ʈ
    private CItemDataSO _itemDataSO;                // ���� ��� �ִ� ���� ����SO

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
            if (_showDebug) Debug.Log("_targetObject �ν����� �������");
            enabled = false;
        }

        bool getSpriteRenderer = _targetObject.TryGetComponent<SpriteRenderer>(out _targetSpriteRdr);

        if (!getSpriteRenderer)
        {
            if (_showDebug) Debug.Log("_targetObject�� SpriteRenderer�� �����ϰ� ���� ����");
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
        CWeaponInstance equipped = CInventoryManager.Instance.EquippedWeapon;
        if (equipped == null) return;

        // ���� ������ ����
        if (_currentInstanceID != equipped._instanceID)
        {
            _currentInstanceID = equipped._instanceID;
            _itemDataSO = equipped._itemData as CWeaponDataSO;
            LoadEquippedWeapon();

            if (_showDebug) Debug.Log("���� InstanceID ���� ���� : ���� ���� ������Ʈ");
        }

        // ���� ������ ������ SO�� �޶����� ������Ʈ (�ν����� ������ ���� ��ȯ�Ǵ� ��쿡 ���� �����ڵ�)
        else if (_itemDataSO != equipped._itemData)
        {
            LoadEquippedWeapon();

            if (_showDebug) Debug.Log("���� SO ���� ���� : ���� ���� ������Ʈ");
        }
    }


    private void LoadEquippedWeapon()
    {
        CWeaponInstance weapon = CInventoryManager.Instance.EquippedWeapon;
        if (weapon == null || weapon._itemData == null) return;

        _itemDataSO = weapon._itemData;
        _targetSpriteRdr.sprite = weapon._itemData.ItemSprite;
    }


    
    // �ִϸ����� ���ؼ� ���� �ݵ�/�ֵθ��� ����
    // ������ �ڵ� ���� ��� �ִϸ����� ��Ǹ� ���� �����ͼ� ���
    public void WeaponRebound()
    {
        Animator anim = _targetObject.GetComponent<Animator>();
                
        if (_itemDataSO.ItemId == "weapon_05")
        {
            anim.Play("Swing", 0, 0f);
        }
        else
        {
            anim.Play("Fire", 0, 0f);
        }
    }

    // ����ü ��Ʈ�� ���� �ʿ�
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


    private IEnumerator CoBulletLifeTime(GameObject a, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(a);
    }
}
