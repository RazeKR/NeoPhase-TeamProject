using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private Sprite[] _itemRankSprites = null;
    [SerializeField] private Text _itemName = null;
    [SerializeField] private Text _upgradeText = null;
    
    [Header("무기 상호작용 UI 인스펙터")]
    [SerializeField] private GameObject _weaponUI = null;
    [SerializeField] private GameObject _equippedUI = null;

    [Header("아이템 상호작용 UI 인스펙터")]
    [SerializeField] private GameObject _itemUI = null;
    [SerializeField] private Text _amountText = null;
    [SerializeField] private Button _deleteButton = null;

    [Header("강화 UI 인스펙터")]
    [SerializeField] private GameObject _upgradeUI = null;

    public CItemInstance Item = null;
    public bool IsChoiceUpgrade = false;
    private int _desiredAmount = 0;
    

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

        if (_itemInfoUI != null)
        {
            _itemInfoUI.SetActive(false);
        }

        if (_upgradeUI != null)
        {
            _upgradeUI.SetActive(false);
        }

        Item = null;
    }


    private void Update()
    {
        if (_desiredAmount == 0 && _deleteButton.interactable)
        {
            _deleteButton.interactable = false;
        }
        else if (_desiredAmount != 0 && !_deleteButton.interactable)
        {
            _deleteButton.interactable = true;
        }

        OnOffInfo();

        OnOffButton();

        InfoUpdate();

        if (Input.GetMouseButtonDown(0))
        {
            // 마우스에서 레이를 쏘고, 이 오브젝트가 걸리는지 체크
            if 
                (
                EventSystem.current.IsPointerOverGameObject()
                && RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition)
                )
                return;

            _desiredAmount = 0;
            _upgradeUI.SetActive(false);
            _itemInfoUI.SetActive(false);            
            Item = null;


            if (IsChoiceUpgrade)
            {
                PointerEventData pd = new PointerEventData(EventSystem.current);
                pd.position = Input.mousePosition;

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pd, results);

                bool isClickedSlot = false;

                foreach (RaycastResult r in results)
                {
                    if (r.gameObject.transform.parent.CompareTag("Slot"))
                    {
                        Debug.Log("슬록 확인됨");
                        isClickedSlot = true;
                        break;
                    }
                }

                
                if (!isClickedSlot) IsChoiceUpgrade = false;
            }
        }
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
        if (Item ==  null) return;

        if (Item._itemData != null && !_itemInfoUI.activeInHierarchy)
        {
            _itemInfoUI.SetActive(true);
        }

        if (Item._itemData == null && _itemInfoUI.activeInHierarchy)
        {
            _itemInfoUI.SetActive(false);
        }
    }


    // 아이템 별로 버튼 UI를 On/Off
    private void OnOffButton()
    {
        if (Item ==  null) return;


        if (Item is CWeaponInstance weapon)
        {
            if (weapon._isEquipped == true)
            {
                _equippedUI.SetActive(true);
                _weaponUI.SetActive(false);
                _itemUI.SetActive(false);
            }

            else
            {
                _equippedUI.SetActive(false);
                _weaponUI.SetActive(true);
                _itemUI.SetActive(false);
            }
        }

        else if (Item is CPotionInstance)
        {
            _equippedUI.SetActive(false);
            _weaponUI.SetActive(false);
            _itemUI.SetActive(true);
            _amountText.text = $"{_desiredAmount} / {(Item as CPotionInstance)._amount}";
        }

        else if (Item is CScrollInstance)
        {
            _equippedUI.SetActive(false);
            _weaponUI.SetActive(false);
            _itemUI.SetActive(true);
            _amountText.text = $"{_desiredAmount} / {(Item as CScrollInstance)._amount}";
        }
    }


    // 스프라이트, 이름, 정보 등 업데이트
    private void InfoUpdate()
    {
        if (Item == null) return;
        
        if (Item is CWeaponInstance weapon)
        {
            _itemSprite.sprite = weapon._itemData.ItemSprite;
            _itemRank.sprite = _itemRankSprites[weapon._rank];
            _itemName.text = weapon._itemData.ItemName;
            _upgradeText.text = "+" + weapon._upgrade.ToString();
        }

        else if (Item is CPotionInstance potion)
        {
            _itemSprite.sprite = potion._itemData.ItemSprite;
            _itemRank.sprite = _itemRankSprites[0];
            _itemName.text = potion._itemData.ItemName;
            _upgradeText.text = "";
        }

        else if (Item is CScrollInstance scroll)
        {
            _itemSprite.sprite = scroll._itemData.ItemSprite;
            _itemRank.sprite = _itemRankSprites[0];
            _itemName.text = scroll._itemData.ItemName;
            _upgradeText.text = "";
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
            IsChoiceUpgrade = true;
            _upgradeUI.SetActive(true);
            _itemUI.SetActive(false);
            Debug.Log("Click Use로 스크롤 선택 : " + IsChoiceUpgrade);
        }

        Item = null;
        RefreshUI();
    }


    public void ClickDelete()
    {
        if (Item == null) return;

        if (Item is CWeaponInstance weapon)
        {
            CInventoryManager.Instance.RemoveItem(Item._instanceID);            
        }

        else
        {
            CInventoryManager.Instance.ReduceItemAmount(Item._instanceID, _desiredAmount);
        }

        _desiredAmount = 0;
        Item = null;
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
            if (Item is CPotionInstance potion)
            {
                _desiredAmount += (_desiredAmount < potion._amount ? 1 : 0);
                _amountText.text = $"{_desiredAmount} / {potion._amount}";
            }
            else if (Item is CScrollInstance scroll)
            {
                _desiredAmount += (_desiredAmount < scroll._amount ? 1 : 0);
                _amountText.text = $"{_desiredAmount} / {scroll._amount}";
            }
        }
    }

}
