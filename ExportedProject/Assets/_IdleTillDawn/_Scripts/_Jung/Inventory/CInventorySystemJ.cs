using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ID 기반 인벤토리 시스템입니다.
/// 아이템 실체는 int ID로만 보관하며, 표시·연산 시 CDataManager를 통해 SO를 조회합니다.
/// CJsonManager를 통해 CSaveData에 자동으로 저장·복원됩니다.
/// </summary>
/// <example>
/// // 아이템 추가
/// CInventorySystem.Instance.AddItem(1001, 2);
///
/// // 장착 무기 교체
/// CInventorySystem.Instance.EquipWeapon(2001);
///
/// // UI 표시 (ID → SO 변환)
/// foreach (int itemId in CInventorySystem.Instance.GetAllItemIds())
/// {
///     CItemDataSO so = CDataManager.Instance.GetItem(itemId);
///     // so.ItemName, so.ItemSprite 등으로 UI 갱신
/// }
/// </example>
public class CInventorySystemJ : MonoBehaviour
{
    #region Events

    /// <summary>인벤토리 내용이 변경될 때마다 발생합니다. UI 갱신 구독에 사용합니다.</summary>
    public event System.Action OnInventoryChanged;

    /// <summary>장착 무기가 변경되었을 때 변경된 무기 ID와 함께 발생합니다.</summary>
    public event System.Action<int> OnWeaponEquipped;

    #endregion

    #region PrivateVariables

    private int _equippedWeaponId = 0;       // 현재 장착 무기 ID (0 = 없음)
    private CWeaponInstance _equippedWeapon; // 현재 장비 중인 무기 정보
    public List<CItemInstance> _inventory = new List<CItemInstance>(); // 접근 가능한 현재 인벤토리 정보

    #endregion

    #region Properties

    /// <summary>싱글톤 인스턴스.</summary>
    public static CInventorySystemJ Instance { get; private set; }

    /// <summary>현재 인벤토리 정보를 가져옵니다.</summary>
    public List<CItemInstance> Inventory { get { return _inventory; } set { _inventory = value; } }

    /// <summary>현재 장착 무기 ID. 0이면 장착 없음.</summary>
    public int EquippedWeaponId => _equippedWeaponId;    

    /// <summary>현재 장착 무기 인스턴스/// </summary>
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
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // CJsonManager 로드 이벤트 구독 - 씬 로드 후 자동 복원
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted += RestoreFromSaveData;

        OnInventoryChanged += SortInventory;
    }

    private void OnDestroy()
    {
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted -= RestoreFromSaveData;

        OnInventoryChanged -= SortInventory;
    }

    #endregion

    #region PublicMethods

    /// <summary>버튼 할당하여 테스트 하는 용도</summary>
    public void AddPotionScroll()
    {
        AddItem(1, 1);
        AddItem(2, 100);
    }

    /// <summary>아이템을 인벤토리에 추가합니다.count만큼 수량을 증가시키고 즉시 저장합니다.</summary>
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

    /// <summary>아이템을 인벤토리에서 제거합니다. count가 현재 수량 이상이면 해당 아이템 항목 자체가 제거됩니다.</summary>
    public bool RemoveItem(string targetInstanceID, int amount = 1)
    {
        var target = _inventory.Find(i => i._instanceID == targetInstanceID);
        if (target == null) return false;

        // 타입별 수량 차감 및 삭제 처리
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
        else // 무기 등 수량 없는 아이템
        {
            if (target == _equippedWeapon) return false;
            _inventory.Remove(target);
        }

        SyncAndSave();
        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 무기를 강화합니다. 스크롤을 하나 소모합니다.
    /// </summary>
    public void UseScroll(string targetInstanceID)
    {
        var weapon = _inventory.Find(i => i._instanceID == targetInstanceID) as CWeaponInstance;

        if (weapon == null)
        {
            Debug.Log("강화할 무기를 찾을 수 없습니다.");
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
            Debug.Log("소모할 스크롤이 인벤토리에 없습니다.");
        }

        SyncAndSave();
        OnInventoryChanged?.Invoke();
    }


    /// <summary>
    /// 무기를 장착합니다. 인벤토리에 없는 무기는 장착할 수 없습니다.
    /// 장착 성공 시 OnWeaponEquipped 이벤트가 발생합니다.
    /// </summary>
    public bool EquipWeapon(string instanceID)
    {
        var weapon = _inventory.Find(i => i._instanceID == instanceID) as CWeaponInstance;
        if (weapon == null) return false;
        if (weapon._isEquipped == true) return false;

        weapon._isEquipped = true;
        _equippedWeapon._isEquipped = false;
        _equippedWeapon = weapon;
        _equippedWeaponId = weapon._itemData.Id;

        SyncAndSave();
        OnInventoryChanged?.Invoke();
        OnWeaponEquipped?.Invoke(_equippedWeaponId);
        return true;
    }

    /// <summary>현재 인벤토리 리스트를 CSaveData 구조로 변환하여 물리 저장합니다.</summary>
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

            // 수량 정보 개별 할당
            if (item is CPotionInstance p) sData.amount = p._amount;
            else if (item is CScrollInstance s) sData.amount = s._amount;
            else sData.amount = 1;

            data.inventorySaveData.items.Add(sData);
        }

        data.equippedWeaponId = _equippedWeaponId;
        CJsonManager.Instance.Save(data);
    }

    /// <summary>아이템 ID에 해당하는 SO를 CDataManager를 통해 반환합니다. 인벤토리 내 아이템 ID를 UI에서 표시할 때 사용합니다.</summary>
    public CItemDataSO GetItemData(int itemId) => CDataManager.Instance.GetItem(itemId);

    /// <summary>무기 ID에 해당하는 CWeaponDataSO를 반환합니다. 스탯 계산이나 발사체 생성 시 사용합니다.</summary>
    public CWeaponDataSO GetWeaponData(int weaponId) => CDataManager.Instance.GetWeapon(weaponId);

    #endregion

    #region PrivateMethods

    /// <summary>CSaveData에서 인벤토리 상태를 복원합니다. CJsonManager.OnLoadCompleted에 구독됩니다.</summary>
    private void RestoreFromSaveData(CSaveData saveData)
    {
        if (saveData == null || saveData.inventorySaveData == null) return;

        _inventory.Clear();
        foreach (var sData in saveData.inventorySaveData.items)
        {
            CItemDataSO so = CDataManager.Instance.GetItem(sData.itemID);
            if (so == null) continue;

            if (so is CWeaponDataSO wSo)
            {
                var w = new CWeaponInstance(wSo);
                w._rank = sData.rank;
                w._isEquipped = sData.isEquipped;
                w._upgrade = sData.upgrade;
                w._instanceID = sData.instanceID;
                _inventory.Add(w);
            }
            else if (so is CPotionDataSO pSo) _inventory.Add(new CPotionInstance(pSo, sData.amount));
            else if (so is CScrollDataSO sSo) _inventory.Add(new CScrollInstance(sSo, sData.amount));
        }

        _equippedWeaponId = saveData.equippedWeaponId;
        if (_equippedWeaponId != 0)
            _equippedWeapon = _inventory.Find(i => i._itemData.Id == _equippedWeaponId) as CWeaponInstance;

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 인벤토리 내 아이템들을 정렬합니다.
    /// OnInventoryChanged를 구독하여 사용합니다.
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

