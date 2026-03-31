using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 UI 정보들을 관리하고 최신화합니다.
/// CInventorySlot 컴포넌트를 가진 유닛들을 생성하여 인벤토리 UI에 정렬합니다.
/// </summary>

public class CInventoryUI : MonoBehaviour
{
    #region SingleTon
    public static CInventoryUI Instance { get; private set; }
    #endregion

    #region Inspectors & PrivateVariables

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

    private CItemInstance _item = null;
    private bool _isChoiceUpgrade = false;
    private int _desiredAmount = 0;

    #endregion

    #region Properties

    public CItemInstance Item { get { return _item; } set { _item = value; } }
    public bool IsChoiceUpgrade { get {  return _isChoiceUpgrade; } set { _isChoiceUpgrade = value; } }
    #endregion

    #region UnityMethods

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

        _item = null;

        if (CInventorySystemJ.Instance != null)
        {
            CInventorySystemJ.Instance.OnInventoryChanged += RefreshUI;
            CInventorySystemJ.Instance.OnInventoryChanged += OnOffInfo;
        }
            
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
            _item = null;


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
                                
                if (!isClickedSlot) _isChoiceUpgrade = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (CInventorySystemJ.Instance != null)
        {
            CInventorySystemJ.Instance.OnInventoryChanged -= RefreshUI;
            CInventorySystemJ.Instance.OnInventoryChanged -= OnOffInfo;
        }
    }

    #endregion

    #region PublicMethods

    /// <summary>UI 슬롯을 최신화합니다.
    /// OnInventoryChanged에 구독하여 갱신합니다.
    /// </summary>
    public void RefreshUI()
    {
        // 자식 슬롯을 순회하며 리셋
        foreach (Transform child in _slotParent)
        {
            Destroy(child.gameObject);
        }

        if (CInventorySystemJ.Instance == null || CInventorySystemJ.Instance.Inventory == null)
        {
            Debug.LogWarning("인벤토리 시스템이 아직 준비되지 않음"); return;
        }

        var inventory = CInventorySystemJ.Instance.Inventory;
        if (inventory ==  null)
        {
            Debug.Log("inventory null"); return;
        }

        // 인벤토리를 순회하며 생성
        foreach (var item in inventory)
        {
            GameObject go = Instantiate(_slotPrefab, _slotParent);
            CInventorySlot slot = go.GetComponent<CInventorySlot>();

            slot.SetSlot(item);
        }


        Debug.Log("UI 최신화 완료");
    }

    /// <summary>
    /// 인벤토리 UI를 열거나 닫습니다.
    /// 버튼 컴포넌트에 연결하거나 키에 할당하여 사용합니다.
    /// </summary>
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

    /// <summary>
    /// 클릭 시 현재 Item에 바인드된 무기로 스왑 / 포션 사용 / 스크롤을 사용할 무기 선택을 활성화합니다.
    /// 버튼 컴포넌트에 연결하거나 키에 할당하여 사용합니다.
    /// </summary>
    public void ClickUse()
    {
        if (_item == null) return;


        if (_item is CWeaponInstance weapon)
        {
            CInventorySystemJ.Instance.EquipWeapon(weapon._instanceID);
        }

        else if (_item is CPotionInstance potion)
        {
            // 포션 사용 로직 구현 필요
            CInventorySystemJ.Instance.RemoveItem(potion._instanceID, 1);
            return;
        }

        else if (_item is CScrollInstance scroll)
        {
            _isChoiceUpgrade = true;
            _upgradeUI.SetActive(true);
            _itemUI.SetActive(false);
        }

        _itemInfoUI.SetActive(false);
        _item = null;
        RefreshUI();
    }

    /// <summary>
    /// 아이템을 삭제합니다. (소모품의 경우 ClickAmountUpDown 버튼으로 추가로 수량을 정한 뒤 삭제합니다.)
    /// 버튼 컴포넌트에 연결하거나 키에 할당하여 사용합니다.
    /// </summary>
    public void ClickDelete()
    {
        if (_item == null) return;

        CInventorySystemJ.Instance.RemoveItem(Item._instanceID, _desiredAmount == 0 ? 1 : _desiredAmount);

        _desiredAmount = 0;
        _item = null;
        _itemInfoUI.SetActive(false);
        OnOffInfo();
        RefreshUI();
    }

    /// <summary>
    /// 삭제할 아이템의 수량을 올리거나 내립니다.
    /// 버튼 컴포넌트에 연결하거나 키에 할당하여 사용하며, bool을 체크할 시 감소, 해제할 시 증가합니다.
    /// </summary>
    public void ClickAmountUpDown(bool isDown)
    {
        if (_item == null) return;

        if (isDown)
        {
            _desiredAmount -= (_desiredAmount > 0 ? 1 : 0);
        }

        else
        {
            if (_item is CPotionInstance potion)
            {
                _desiredAmount += (_desiredAmount < potion._amount ? 1 : 0);
                _amountText.text = $"{_desiredAmount} / {potion._amount}";
            }
            else if (_item is CScrollInstance scroll)
            {
                _desiredAmount += (_desiredAmount < scroll._amount ? 1 : 0);
                _amountText.text = $"{_desiredAmount} / {scroll._amount}";
            }
        }
    }

    #endregion

    #region PrivateMethods

    /// <summary>
    /// 현재 슬롯으로부터 받아온 아이템 상세 UI를 활성화/비활성화를 실행합니다.
    /// </summary>
    private void OnOffInfo()
    {
        if (_item == null || _item._itemData == null)
        {
            if (_itemInfoUI.activeSelf) _itemInfoUI.SetActive(false);
            return;
        }

        if (!_itemInfoUI.activeSelf)
        {
            _itemInfoUI.SetActive(true);
        }
    }


    /// <summary>
    /// 아이템 별로 버튼 UI를 On/Off 힙니다.
    /// </summary>
    private void OnOffButton()
    {
        if (_item ==  null) return;


        if (_item is CWeaponInstance weapon)
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

        else if (_item is CPotionInstance)
        {
            _equippedUI.SetActive(false);
            _weaponUI.SetActive(false);
            _itemUI.SetActive(true);
            _amountText.text = $"{_desiredAmount} / {(_item as CPotionInstance)._amount}";
        }

        else if (_item is CScrollInstance)
        {
            _equippedUI.SetActive(false);
            _weaponUI.SetActive(false);
            _itemUI.SetActive(true);
            _amountText.text = $"{_desiredAmount} / {(_item as CScrollInstance)._amount}";
        }
    }


    /// <summary>
    /// 스프라이트, 이름, 정보 등을 업데이트합니다.
    /// </summary>
    private void InfoUpdate()
    {
        if (_item == null) return;
        
        if (_item is CWeaponInstance weapon)
        {
            _itemSprite.sprite = weapon._itemData.ItemSprite;
            _itemRank.sprite = _itemRankSprites[weapon._rank];
            _itemName.text = weapon._itemData.ItemName;
            _upgradeText.text = "+" + weapon._upgrade.ToString();
        }

        else if (_item is CPotionInstance potion)
        {
            _itemSprite.sprite = potion._itemData.ItemSprite;
            _itemRank.sprite = _itemRankSprites[0];
            _itemName.text = potion._itemData.ItemName;
            _upgradeText.text = "";
        }

        else if (_item is CScrollInstance scroll)
        {
            _itemSprite.sprite = scroll._itemData.ItemSprite;
            _itemRank.sprite = _itemRankSprites[0];
            _itemName.text = scroll._itemData.ItemName;
            _upgradeText.text = "";
        }
    }

    #endregion   

}
