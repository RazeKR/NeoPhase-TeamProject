using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// �κ��丮 UI �������� �����ϰ� �ֽ�ȭ�մϴ�.
/// CInventorySlot ������Ʈ�� ���� ���ֵ��� �����Ͽ� �κ��丮 UI�� �����մϴ�.
/// </summary>

public class CInventoryUI : MonoBehaviour
{
    #region SingleTon
    public static CInventoryUI Instance { get; private set; }
    #endregion

    #region Inspectors & PrivateVariables

    [Header("�κ��丮 ���� �ν�����")]
    [SerializeField] private Transform _slotParent = null;      // �κ��丮 ������ �� �θ� ��ü
    [SerializeField] private GameObject _slotPrefab = null;     // �κ��丮 ���� ������
    [SerializeField] private GameObject _inventoryUI = null;    // �κ��丮 UI (â On/Off ����)

    [Header("������ �� ���� �ν�����")]
    [SerializeField] private GameObject _itemInfoUI = null;
    [SerializeField] private Image _itemSprite = null;
    [SerializeField] private Image _itemRank = null;
    [SerializeField] private Sprite[] _itemRankSprites = null;
    [SerializeField] private Text _itemName = null;
    [SerializeField] private Text _upgradeText = null;
    
    [Header("���� ��ȣ�ۿ� UI �ν�����")]
    [SerializeField] private GameObject _weaponUI = null;
    [SerializeField] private GameObject _equippedUI = null;

    [Header("������ ��ȣ�ۿ� UI �ν�����")]
    [SerializeField] private GameObject _itemUI = null;
    [SerializeField] private Text _amountText = null;
    [SerializeField] private Button _deleteButton = null;

    [Header("��ȭ UI �ν�����")]
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
            // ���콺���� ���̸� ���, �� ������Ʈ�� �ɸ����� üũ
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
                        Debug.Log("���� Ȯ�ε�");
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

    /// <summary>UI ������ �ֽ�ȭ�մϴ�.
    /// OnInventoryChanged�� �����Ͽ� �����մϴ�.
    /// </summary>
    public void RefreshUI()
    {
        // �ڽ� ������ ��ȸ�ϸ� ����
        foreach (Transform child in _slotParent)
        {
            Destroy(child.gameObject);
        }

        if (CInventorySystemJ.Instance == null || CInventorySystemJ.Instance.Inventory == null)
        {
            Debug.LogWarning("�κ��丮 �ý����� ���� �غ���� ����"); return;
        }

        var inventory = CInventorySystemJ.Instance.Inventory;
        if (inventory ==  null)
        {
            Debug.Log("inventory null"); return;
        }

        // �κ��丮�� ��ȸ�ϸ� ����
        foreach (var item in inventory)
        {
            GameObject go = Instantiate(_slotPrefab, _slotParent);
            CInventorySlot slot = go.GetComponent<CInventorySlot>();

            slot.SetSlot(item);
        }


        Debug.Log("UI �ֽ�ȭ �Ϸ�");
    }

    /// <summary>
    /// �κ��丮 UI�� ���ų� �ݽ��ϴ�.
    /// ��ư ������Ʈ�� �����ϰų� Ű�� �Ҵ��Ͽ� ����մϴ�.
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
    /// Ŭ�� �� ���� Item�� ���ε�� ����� ���� / ���� ��� / ��ũ���� ����� ���� ������ Ȱ��ȭ�մϴ�.
    /// ��ư ������Ʈ�� �����ϰų� Ű�� �Ҵ��Ͽ� ����մϴ�.
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
            CPlayerController player = FindObjectOfType<CPlayerController>();
            if (player != null)
            {
                CPotionDataSO data = potion._data;

                if (data.HealAmount > 0)
                    player.Heal(data.HealAmount);

                if (data.ManaHealAmount > 0)
                {
                    CPlayerStatManager statManager = player.GetComponent<CPlayerStatManager>();
                    if (statManager != null)
                        statManager.RestoreMana(data.ManaHealAmount);
                }
            }

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
    /// �������� �����մϴ�. (�Ҹ�ǰ�� ��� ClickAmountUpDown ��ư���� �߰��� ������ ���� �� �����մϴ�.)
    /// ��ư ������Ʈ�� �����ϰų� Ű�� �Ҵ��Ͽ� ����մϴ�.
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
    /// ������ �������� ������ �ø��ų� �����ϴ�.
    /// ��ư ������Ʈ�� �����ϰų� Ű�� �Ҵ��Ͽ� ����ϸ�, bool�� üũ�� �� ����, ������ �� �����մϴ�.
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
    /// ���� �������κ��� �޾ƿ� ������ �� UI�� Ȱ��ȭ/��Ȱ��ȭ�� �����մϴ�.
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
    /// ������ ���� ��ư UI�� On/Off ���ϴ�.
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
    /// ��������Ʈ, �̸�, ���� ���� ������Ʈ�մϴ�.
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
