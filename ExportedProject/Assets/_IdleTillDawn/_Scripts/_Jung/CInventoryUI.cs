using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CInventoryUI : MonoBehaviour
{
    public static CInventoryUI Instance { get; private set; }

    [Header("인벤토리 슬롯 인스펙터")]
    [SerializeField] private Transform _slotParent = null;      // 인벤토리 슬롯이 들어갈 부모 객체
    [SerializeField] private GameObject _slotPrefab = null;     // 인벤토리 슬롯 프리팹
    [SerializeField] private GameObject _inventoryUI = null;    // 인벤토리 UI (창 On/Off 조절)

    [Header("아이템 상세 정보 인스펙터")]
    [SerializeField] private GameObject _itemInfoUI = null;
    [SerializeField] private Image _itemSprite = null;
    [SerializeField] private Image _itemRank = null;
    [SerializeField] private Sprite[] _itemRanksSprite = null;
    [SerializeField] private Text _itemName = null;
    [SerializeField] private Text _itemText = null;

    [Header("무기 상호작용 UI 인스펙터")]
    [SerializeField] private GameObject _weaponUI = null;

    [Header("아이템 상호작용 UI 인스펙터")]
    [SerializeField] private GameObject _itemUI = null;
    [SerializeField] private Text _amountText = null;


    public CItemInstance Item;
    
    public bool IsChoiceUpgrade = false;
    private int _desiredAmount = 0;
    private bool _isActive = false;
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
            return;
        }

        Instance = this;        
    }

    private void Start()
    {
        if (_inventoryUI != null)
        {
            _inventoryUI.SetActive(false);
        }
    }


    private void Update()
    {
        OnOffInfo();
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // UI 슬롯 최신화
    //  ㄴ 최신화 후 유니티 UI 컴포넌트로 별도 정렬함
    public void RefreshUI()
    {
        // 자식 슬롯을 순회하며 리셋
        foreach (Transform child in _slotParent)
        {
            Destroy(child.gameObject);
        }

        var inventory = CInventoryManager.Instance.Inventory;

        // 인벤토리를 순회하며 생성
        foreach (var item in inventory)
        {
            GameObject go = Instantiate(_slotPrefab, _slotParent);
            CInventorySlot slot = go.GetComponent<CInventorySlot>();

            slot.SetSlot(item);
        }

        Debug.Log("UI 최신화 완료");
    }



    // 인벤토리 창 열고 닫는 기능 (버튼이나 키 할당해서 사용)
    public void OnOffInventoryUI()
    {
        if (_inventoryUI == null) return;

        if (_inventoryUI.activeInHierarchy)
        {
            _inventoryUI.SetActive(false);
        }

        else
        {
            _inventoryUI.SetActive(true);
            RefreshUI();
        }
    }


    // 현재 슬롯으로부터 받아온 아이템 인스턴스 여부에 따라 UI 활성화/비활성화
    private void OnOffInfo()
    {
        if (!_isActive && Item != null)
        {
            _itemInfoUI.SetActive(true);
        }

        else if (_isActive && Item == null)
        {
            _itemInfoUI.SetActive(false);
        }
    }


    // 버튼 컴포넌트에 연결하여 호출
    // 클릭 시 현재 Item에 바인드된 무기로 스왑 / 포션 사용 / 스크롤 사용할 무기 선택 활성화
    public void ClickUse()
    {
        if (Item == null) return;

        if (Item is CWeaponInstance weapon)
        {
            CInventoryManager.Instance.SwapWeapon(weapon._instanceID);
        }

        else if (Item is CPotionInstance potion)
        {
            CInventoryManager.Instance.UsePotion(potion._instanceID);
        }

        else if (Item is CScrollInstance scroll)
        {
            IsChoiceUpgrade = true; // 인벤토리 슬롯 내에서 이 bool 값을 확인하고 있음
        }

        RefreshUI();
    }


    public void ClickDelete()
    {
        if (Item == null) return;

        if (Item is CWeaponInstance weapon)
        {
            CInventoryManager.Instance.RemoveItem(Item._instanceID);            
        }

        else if (Item is CPotionInstance potion)
        {
            (Item as CPotionInstance)._amount -= _desiredAmount;

            if ((Item as CPotionInstance)._amount <= 0)
            {
                CInventoryManager.Instance.RemoveItem(Item._instanceID);
            }

            CInventoryManager.Instance.SaveInventory(CInventoryManager.Instance.Inventory);
        }

        else if (Item is CScrollInstance scroll)
        {
            (Item as CScrollInstance)._amount -= _desiredAmount;

            if ((Item as CScrollInstance)._amount <= 0)
            {
                CInventoryManager.Instance.RemoveItem(Item._instanceID);
            }

            CInventoryManager.Instance.SaveInventory(CInventoryManager.Instance.Inventory);
        }

        _desiredAmount = 0;
        RefreshUI();

        _itemInfoUI.SetActive(false);
    }


    public void ClickAmountUpDown(bool isDown)
    {
        if (Item ==  null) return;

        if (isDown)
        {
            _desiredAmount -= (_desiredAmount > 0 ? 1 : 0);
        }

        else
        {
            _desiredAmount += (_desiredAmount < (Item as CPotionInstance)._amount ? 1 : 0);
        }

        _amountText.text = $"{_desiredAmount} / {(Item as CPotionInstance)._amount}";
    }

}
