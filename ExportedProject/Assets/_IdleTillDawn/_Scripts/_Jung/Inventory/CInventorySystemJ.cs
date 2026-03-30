using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;
using static UnityEngine.UI.Image;

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

    //private Dictionary<int, int> _inventory = new(); // <아이템 ID, 수량>
    private int _equippedWeaponId = 0;                // 현재 장착 무기 ID (0 = 없음)
    private CWeaponInstance _equippedWeapon; // 현재 장비 중인 무기 정보
    private List<CItemInstance> _inventory = new List<CItemInstance>(); // 접근 가능한 현재 인벤토리 정보

    #endregion

    #region Properties

    /// <summary>싱글톤 인스턴스.</summary>
    public static CInventorySystemJ Instance { get; private set; }

    /// <summary>현재 장착 무기 ID. 0이면 장착 없음.</summary>
    public int EquippedWeaponId => _equippedWeaponId;

    /// <summary>장착 무기의 SO. 미장착이거나 ID가 없으면 null을 반환합니다.</summary>
    public CWeaponDataSO EquippedWeapon =>
        _equippedWeaponId > 0 ? CDataManager.Instance.GetWeapon(_equippedWeaponId) : null;

    public List<CItemInstance> Inventory => _inventory;

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
    }

    private void OnDestroy()
    {
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted -= RestoreFromSaveData;
    }

    #endregion

    #region PublicMethods

    /// <summary>
    /// 아이템을 인벤토리에 추가합니다.
    /// count만큼 수량을 증가시키고 즉시 저장합니다.
    /// </summary>
    public void AddItem(int itemId, int count = 1 , int rank = 0)
    {
        if (count <= 0) return;
        if (CDataManager.Instance.GetItem(itemId) == null) return; // 유효하지 않은 ID 방어

        CItemDataSO so = CDataManager.Instance.GetItem(itemId);

        if (so.ItemType == EItemType.Weapon)
        {
            for (int i = 0; i < count; i++)
            {
                CWeaponInstance w = new CWeaponInstance(so as CWeaponDataSO);

                w._rank = rank;

                Inventory.Add(w);
            }
        }

        else if (so.ItemType == EItemType.Potion)
        {
            var existPotion = Inventory.Find(i => i._itemData.Id == itemId) as CPotionInstance;

            if (existPotion != null)
            {
                existPotion._amount += count;

                if (existPotion._amount + count > existPotion._maxAmount)
                {
                    existPotion._amount = existPotion._maxAmount;
                }
            }
            else
            {
                Inventory.Add(new CPotionInstance(so as CPotionDataSO, count));
            }
        }

        else if (so.ItemType == EItemType.Scroll)
        {
            var existScroll = Inventory.Find(s => s._itemData.Id == itemId) as CScrollInstance;

            if (existScroll != null)
            {
                existScroll._amount += count;

                if (existScroll._amount + count > existScroll._maxAmount)
                {
                    existScroll._amount = existScroll._maxAmount;
                }
            }
            else
            {
                Inventory.Add(new CScrollInstance(so as CScrollDataSO, count));
            }
        }

        CJsonManager.Instance.SaveItemChange(itemId, count);
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 아이템을 인벤토리에서 제거합니다.
    /// count가 현재 수량 이상이면 해당 아이템 항목 자체가 제거됩니다.
    /// </summary>
    public bool RemoveItem(string targetInstanceID, int amount)
    {
        var target = Inventory.Find(i => i._instanceID == targetInstanceID);

        if (target ==  null) return false;


        if (target is CPotionInstance potion)
        {
            potion._amount -= amount;

            if (potion._amount <= 0)
            {
                RemoveItem(targetInstanceID);
            }
        }

        else if (target != null && (target is CScrollInstance scroll))
        {
            scroll._amount -= amount;

            if (scroll._amount <= 0)
            {
                RemoveItem(targetInstanceID);
            }
        }

        else return false;

        Inventory.Remove(target);

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 아이템 제거 함수 오버로딩
    /// 수량 상관없이 제거되는 처리를 수행합니다.
    /// </summary>
    public bool RemoveItem(string targetInstanceID)
    {
        var target = Inventory.Find(i => i._instanceID == targetInstanceID);

        if (target == null) return false;

        if (target == _equippedWeapon)
        {            
            return false;
        }              
        
        Inventory.Remove(target);

        //SaveInventory(Inventory);

        Inventory.Remove(target);

        OnInventoryChanged?.Invoke();
        return true;
    }


    /// <summary>특정 아이템의 보유 수량을 반환합니다. 없으면 0을 반환합니다.</summary>
    //public int GetItemCount(int itemId)
    //{
    //    _inventory.TryGetValue(itemId, out int count);
    //    return count;
    //}

    /// <summary>특정 아이템을 1개 이상 보유하고 있는지 여부를 반환합니다.</summary>
    //public bool HasItem(int itemId, int requiredCount = 1) => GetItemCount(itemId) >= requiredCount;

    /// <summary>인벤토리에 있는 모든 아이템 ID 목록을 반환합니다.</summary>
    //public IEnumerable<int> GetAllItemIds() => _inventory.Keys;

    /// <summary>
    /// 무기를 장착합니다. 인벤토리에 없는 무기는 장착할 수 없습니다.
    /// 장착 성공 시 OnWeaponEquipped 이벤트가 발생합니다.
    /// </summary>
    public bool EquipWeapon(int weaponId)
    {
        //if (!HasItem(weaponId))
        //{
        //    Debug.LogWarning($"[CInventorySystem] 인벤토리에 없는 무기를 장착하려 했습니다. ID: {weaponId}");
        //    return false;
        //}

        if (CDataManager.Instance.GetWeapon(weaponId) == null) return false;

        _equippedWeaponId = weaponId;
        CJsonManager.Instance.SaveEquippedWeapon(weaponId);
        OnWeaponEquipped?.Invoke(weaponId);
        return true;
    }

    /// <summary>장착 무기를 해제합니다.</summary>
    public void UnequipWeapon()
    {
        _equippedWeaponId = 0;
        CJsonManager.Instance.SaveEquippedWeapon(0);
        OnWeaponEquipped?.Invoke(0);
    }

    /// <summary>
    /// 아이템 ID에 해당하는 SO를 CDataManager를 통해 반환합니다.
    /// 인벤토리 내 아이템 ID를 UI에서 표시할 때 사용합니다.
    /// </summary>
    public CItemDataSO GetItemData(int itemId) => CDataManager.Instance.GetItem(itemId);

    /// <summary>
    /// 무기 ID에 해당하는 CWeaponDataSO를 반환합니다.
    /// 스탯 계산이나 발사체 생성 시 사용합니다.
    /// </summary>
    public CWeaponDataSO GetWeaponData(int weaponId) => CDataManager.Instance.GetWeapon(weaponId);

    #endregion

    #region PrivateMethods

    /// <summary>CSaveData에서 인벤토리 상태를 복원합니다. CJsonManager.OnLoadCompleted에 구독됩니다.</summary>
    private void RestoreFromSaveData(CSaveData saveData)
    {
        if (saveData == null) return;

        _inventory.Clear();
        
        CInventorySaveData save = saveData.inventorySaveData;
        List<CItemInstance> loadedInventory = new List<CItemInstance>();

        foreach (var data in save.items)
        {
            CItemDataSO so = CDataManager.Instance.GetItem(data.itemID);

            if (so != null)
            {
                CItemInstance newItem = null;

                if (data.type == EItemType.Potion)
                {
                    newItem = new CPotionInstance(so as CPotionDataSO, data.amount);
                }
                else if (data.type == EItemType.Scroll)
                {
                    newItem = new CScrollInstance(so as CScrollDataSO, data.amount);
                }
                else if (data.type == EItemType.Weapon)
                {
                    var weapon = new CWeaponInstance(so as CWeaponDataSO);
                    weapon._rank = data.rank;
                    weapon._upgrade = data.upgrade;
                    weapon._isEquipped = data.isEquipped;
                    newItem = weapon;
                }

                newItem._instanceID = data.instanceID;
                loadedInventory.Add(newItem);
            }
        }

        // 로드된 리스트의 장착 무기 정보 캐싱
        _equippedWeapon = loadedInventory
            .OfType<CWeaponInstance>()
            .FirstOrDefault(w => w._isEquipped);

        _equippedWeaponId = _equippedWeapon._itemData.Id;

        OnInventoryChanged?.Invoke();
    }

    #endregion
}

