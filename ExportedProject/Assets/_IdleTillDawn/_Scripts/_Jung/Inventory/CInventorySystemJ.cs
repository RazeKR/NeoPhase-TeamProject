using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ID ��� �κ��丮 �ý����Դϴ�.
/// ������ ��ü�� int ID�θ� �����ϸ�, ǥ�á����� �� CDataManager�� ���� SO�� ��ȸ�մϴ�.
/// CJsonManager�� ���� CSaveData�� �ڵ����� ���塤�����˴ϴ�.
/// </summary>
/// <example>
/// // ������ �߰�
/// CInventorySystem.Instance.AddItem(1001, 2);
///
/// // ���� ���� ��ü
/// CInventorySystem.Instance.EquipWeapon(2001);
///
/// // UI ǥ�� (ID �� SO ��ȯ)
/// foreach (int itemId in CInventorySystem.Instance.GetAllItemIds())
/// {
///     CItemDataSO so = CDataManager.Instance.GetItem(itemId);
///     // so.ItemName, so.ItemSprite ������ UI ����
/// }
/// </example>
public class CInventorySystemJ : MonoBehaviour
{
    #region Events

    /// <summary>�κ��丮 ������ ����� ������ �߻��մϴ�. UI ���� ������ ����մϴ�.</summary>
    public event System.Action OnInventoryChanged;

    /// <summary>���� ���Ⱑ ����Ǿ��� �� ����� ���� ID�� �Բ� �߻��մϴ�.</summary>
    public event System.Action<int> OnWeaponEquipped;

    #endregion

    #region PrivateVariables

    private int _equippedWeaponId = 0;       // ���� ���� ���� ID (0 = ����)
    private CWeaponInstance _equippedWeapon; // ���� ��� ���� ���� ����
    public List<CItemInstance> _inventory = new List<CItemInstance>(); // ���� ������ ���� �κ��丮 ����

    #endregion

    #region Properties

    /// <summary>�̱��� �ν��Ͻ�.</summary>
    public static CInventorySystemJ Instance { get; private set; }

    /// <summary>���� �κ��丮 ������ �����ɴϴ�.</summary>
    public List<CItemInstance> Inventory { get { return _inventory; } set { _inventory = value; } }

    /// <summary>���� ���� ���� ID. 0�̸� ���� ����.</summary>
    public int EquippedWeaponId => _equippedWeaponId;    

    /// <summary>���� ���� ���� �ν��Ͻ�/// </summary>
    public CWeaponInstance EquippedWeapon { get { return _equippedWeapon; } set { _equippedWeapon = value; } }

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
    }

    private void Start()
    {
        // CJsonManager �ε� �̺�Ʈ ���� - �� �ε� �� �ڵ� ����
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted += RestoreFromSaveData;

        OnInventoryChanged += SortInventory;

        // 이미 로드된 데이터가 있다면 즉시 복구
        if (CJsonManager.Instance.CurrentSaveData != null)
        {
            RestoreFromSaveData(CJsonManager.Instance.CurrentSaveData);
        }
    }

    private void OnDestroy()
    {
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted -= RestoreFromSaveData;

        OnInventoryChanged -= SortInventory;
    }

    #endregion

    #region PublicMethods

    /// <summary>��ư �Ҵ��Ͽ� �׽�Ʈ �ϴ� �뵵</summary>
    public void AddPotionScroll()
    {
        AddItem(1, 1);
        AddItem(2, 100);
    }

    /// <summary>�������� �κ��丮�� �߰��մϴ�.count��ŭ ������ ������Ű�� ��� �����մϴ�.</summary>
    public void AddItem(int itemId, int count = 1 , int rank = 0)
    {
        if (count <= 0) return;
        CItemDataSO so = CDataManager.Instance.GetItem(itemId);
        if (so == null) return;

        if (so.ItemType == EItemType.Weapon)
        {
            for (int i = 0; i < count; i++)
            {
                CWeaponInstance w = new CWeaponInstance(so as CWeaponDataSO);
                w._rank = rank;
                _inventory.Add(w);
            }
        }
        else if (so.ItemType == EItemType.Potion)
        {
            var exist = _inventory.Find(i => i._itemData.Id == itemId) as CPotionInstance;
            if (exist != null) exist._amount = (byte)Mathf.Min(exist._amount + count, exist._maxAmount);
            else _inventory.Add(new CPotionInstance(so as CPotionDataSO, count));
        }
        else if (so.ItemType == EItemType.Scroll)
        {
            var exist = _inventory.Find(i => i._itemData.Id == itemId) as CScrollInstance;
            if (exist != null) exist._amount = (byte)Mathf.Min(exist._amount + count, exist._maxAmount);
            else _inventory.Add(new CScrollInstance(so as CScrollDataSO, count));
        }

        SyncAndSave();
        OnInventoryChanged?.Invoke();
    }

    /// <summary>�������� �κ��丮���� �����մϴ�. count�� ���� ���� �̻��̸� �ش� ������ �׸� ��ü�� ���ŵ˴ϴ�.</summary>
    public bool RemoveItem(string targetInstanceID, int amount = 1)
    {
        var target = _inventory.Find(i => i._instanceID == targetInstanceID);
        if (target == null) return false;

        // Ÿ�Ժ� ���� ���� �� ���� ó��
        if (target is CPotionInstance p)
        {
            p._amount -= amount;
            if (p._amount <= 0) _inventory.Remove(target);
        }
        else if (target is CScrollInstance s)
        {
            s._amount -= amount;
            if (s._amount <= 0) _inventory.Remove(target);
        }
        else // ���� �� ���� ���� ������
        {
            if (target == _equippedWeapon) return false;
            _inventory.Remove(target);
        }

        SyncAndSave();
        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// ���⸦ ��ȭ�մϴ�. ��ũ���� �ϳ� �Ҹ��մϴ�.
    /// </summary>
    public void UseScroll(string targetInstanceID)
    {
        var weapon = _inventory.Find(i => i._instanceID == targetInstanceID) as CWeaponInstance;

        if (weapon == null)
        {
            Debug.Log("��ȭ�� ���⸦ ã�� �� �����ϴ�.");
            return;
        }

        var scroll = _inventory.Find(i => i is CScrollInstance) as CScrollInstance;

        if (scroll != null)
        {
            CWeaponUpgrade.Instance.TryUpgrade(weapon);

            RemoveItem(scroll._instanceID, 1);            
        }
        else
        {
            Debug.Log("�Ҹ��� ��ũ���� �κ��丮�� �����ϴ�.");
        }

        SyncAndSave();
        OnInventoryChanged?.Invoke();
    }


    /// <summary>
    /// ���⸦ �����մϴ�. �κ��丮�� ���� ����� ������ �� �����ϴ�.
    /// ���� ���� �� OnWeaponEquipped �̺�Ʈ�� �߻��մϴ�.
    /// </summary>
    public bool EquipWeapon(string instanceID)
    {
        var weapon = _inventory.Find(i => i._instanceID == instanceID) as CWeaponInstance;
        if (weapon == null) return false;
        if (weapon._isEquipped == true) return false;

        weapon._isEquipped = true;
        if (_equippedWeapon != null) _equippedWeapon._isEquipped = false;
        _equippedWeapon = weapon;
        _equippedWeaponId = weapon._itemData.Id;

        SyncAndSave();
        OnInventoryChanged?.Invoke();
        OnWeaponEquipped?.Invoke(_equippedWeaponId);
        return true;
    }

    /// <summary>���� �κ��丮 ����Ʈ�� CSaveData ������ ��ȯ�Ͽ� ���� �����մϴ�.</summary>
    public void SyncAndSave()
    {
        if (CJsonManager.Instance == null) return;
        CSaveData data = CJsonManager.Instance.GetOrCreateSaveData();

        data.inventorySaveData.items.Clear();
        foreach (var item in _inventory)
        {
            CItemSaveData sData = new CItemSaveData
            {
                itemID = item._itemData.Id,
                instanceID = item._instanceID,
                type = item._itemData.ItemType,
                rank = (item is CWeaponInstance r) ? r._rank : 0,
                isEquipped = (item is CWeaponInstance e) ? e._isEquipped : false,
                upgrade = (item is CWeaponInstance u) ? u._upgrade : 0,
            };

            // ���� ���� ���� �Ҵ�
            if (item is CPotionInstance p) sData.amount = p._amount;
            else if (item is CScrollInstance s) sData.amount = s._amount;
            else sData.amount = 1;

            data.inventorySaveData.items.Add(sData);
        }

        data.equippedWeaponId = _equippedWeaponId;
        CJsonManager.Instance.Save(data);
    }

    /// <summary>������ ID�� �ش��ϴ� SO�� CDataManager�� ���� ��ȯ�մϴ�. �κ��丮 �� ������ ID�� UI���� ǥ���� �� ����մϴ�.</summary>
    public CItemDataSO GetItemData(int itemId) => CDataManager.Instance.GetItem(itemId);

    /// <summary>���� ID�� �ش��ϴ� CWeaponDataSO�� ��ȯ�մϴ�. ���� ����̳� �߻�ü ���� �� ����մϴ�.</summary>
    public CWeaponDataSO GetWeaponData(int weaponId) => CDataManager.Instance.GetWeapon(weaponId);

    #endregion

    #region PrivateMethods

    /// <summary>CSaveData���� �κ��丮 ���¸� �����մϴ�. CJsonManager.OnLoadCompleted�� �����˴ϴ�.</summary>
    private void RestoreFromSaveData(CSaveData saveData)
    {
        if (saveData == null || saveData.inventorySaveData == null) return;

        _inventory.Clear();
        foreach (var sData in saveData.inventorySaveData.items)
        {
            CItemDataSO so = CDataManager.Instance.GetItem(sData.itemID);
            if (so == null)
            {
                Debug.LogWarning($"[CInventorySystemJ] ID={sData.itemID} ({sData.type}) SO를 CDataManager에서 찾을 수 없습니다. CDataManager의 _weaponList/_itemList에 해당 SO가 등록되었는지 확인하세요.");
                continue;
            }

            CItemInstance itemInstance = null;

            if (so is CWeaponDataSO wSo)
            {
                var w = new CWeaponInstance(wSo);
                w._rank = sData.rank;
                w._isEquipped = sData.isEquipped;
                w._upgrade = sData.upgrade;
                itemInstance = w; // 할당
            }
            else if (so is CPotionDataSO pSo)
            {
                itemInstance = new CPotionInstance(pSo, sData.amount);
            }
            else if (so is CScrollDataSO sSo)
            {
                itemInstance = new CScrollInstance(sSo, sData.amount);
            }

            if (itemInstance != null)
            {
                itemInstance._instanceID = sData.instanceID;
                _inventory.Add(itemInstance);
            }
        }

        _equippedWeaponId = saveData.equippedWeaponId;
        if (_equippedWeaponId != 0)
            _equippedWeapon = _inventory.Find(i => i is CWeaponInstance w && w._isEquipped) as CWeaponInstance;

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// �κ��丮 �� �����۵��� �����մϴ�.
    /// OnInventoryChanged�� �����Ͽ� ����մϴ�.
    /// </summary>
    private void SortInventory()
    {
        Inventory = Inventory
            .OrderByDescending(i => (i as CWeaponInstance)?._isEquipped ?? false)
            .ThenBy(i => i._itemData.ItemType)
            .ThenByDescending(i => (i as CWeaponInstance)?._rank ?? 0)
            .ThenBy(i => i._itemData.Id)
            .ToList();
    }
        
    #endregion
}

