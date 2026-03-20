using UnityEngine;

/*
ㆍCWeaponEquip
- 인스펙터로 지정한 무기 스프라이트 렌더러를 교체해주는 기능
   ㄴ 시각적으로 보여지는 인게임 무기만 관여하는 클래스
- 무기 반동/휘두르기 함수 WeaponRebound() 통해서 연출 가능
*/

public class CWeaponEquip : MonoBehaviour
{
    public static CWeaponEquip Instance { get; private set; }

    [SerializeField] private GameObject _targetObject = null;

    private string _currentInstanceID;
    private SpriteRenderer _targetSpriteRdr;
    private CItemDataSO _itemDataSO;
    
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
            Debug.Log("_targetObject 인스펙터 비어있음");
            enabled = false;
        }

        bool getSpriteRenderer = _targetObject.TryGetComponent<SpriteRenderer>(out _targetSpriteRdr);

        if (!getSpriteRenderer)
        {
            Debug.Log("_targetObject가 SpriteRenderer를 포함하고 있지 않음");
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
        // 현재 장착한 무기
        if (_currentInstanceID != CInventoryManager.Instance.EquippedWeapon._instanceID)
        {
            _currentInstanceID = CInventoryManager.Instance.EquippedWeapon._instanceID;
            _itemDataSO = CInventoryManager.Instance.EquippedWeapon._itemData as CWeaponDataSO;
            LoadEquippedWeapon();

            Debug.Log("무기 InstanceID 변경 감지 : 무기 정보 업데이트");
        }

        // 현재 장착한 무기의 SO가 달라지면 업데이트 (인스펙터 등으로 강제 변환되는 경우에 대한 예외코드)
        else if (_itemDataSO != CInventoryManager.Instance.EquippedWeapon._itemData)
        {
            LoadEquippedWeapon();

            Debug.Log("무기 SO 변경 감지 : 무기 정보 업데이트");
        }

        

        // 확인용, 차후 인풋으로 변경
        if (Input.GetMouseButtonDown(0))
        {
            WeaponRebound();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            CInventoryManager.Instance.SwapWeapon("b");
        }
    }


    private void LoadEquippedWeapon()
    {
        CWeaponInstance weapon = CInventoryManager.Instance.EquippedWeapon;

        _itemDataSO = weapon._itemData;

        _targetSpriteRdr.sprite = weapon._itemData.ItemSprite;
    }


    
    // 애니메이터 통해서 무기 반동/휘두르기 연출
    // 복잡한 코드 제어 대신 애니메이터 모션만 쉽게 가져와서 사용
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
}
