using System.Collections.Generic;
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
public class CInventorySystem : MonoBehaviour
{
    #region Events

    /// <summary>인벤토리 내용이 변경될 때마다 발생합니다. UI 갱신 구독에 사용합니다.</summary>
    public event System.Action OnInventoryChanged;

    /// <summary>장착 무기가 변경되었을 때 변경된 무기 ID와 함께 발생합니다.</summary>
    public event System.Action<int> OnWeaponEquipped;

    #endregion

    #region PrivateVariables

    private Dictionary<int, int> _inventory = new(); // <아이템 ID, 수량>
    private int _equippedWeaponId = 0;                // 현재 장착 무기 ID (0 = 없음)

    #endregion

    #region Properties

    /// <summary>싱글톤 인스턴스.</summary>
    public static CInventorySystem Instance { get; private set; }

    /// <summary>현재 장착 무기 ID. 0이면 장착 없음.</summary>
    public int EquippedWeaponId => _equippedWeaponId;

    /// <summary>장착 무기의 SO. 미장착이거나 ID가 없으면 null을 반환합니다.</summary>
    public CWeaponDataSO EquippedWeapon =>
        _equippedWeaponId > 0 ? CDataManager.Instance.GetWeapon(_equippedWeaponId) : null;

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

    /// <summary>버튼 할당하여 테스트 하는 용도/// </summary>
    public void AddPotionScroll()
    {
        AddItem(1, 1);
        AddItem(2, 1);
    }


    /// <summary>아이템을 인벤토리에 추가합니다. count만큼 수량을 증가시키고 즉시 저장합니다./// </summary>
    public void AddItem(int itemId, int count = 1)
    {
        if (count <= 0) return;
        if (CDataManager.Instance.GetItem(itemId) == null) return; // 유효하지 않은 ID 방어

        if (_inventory.ContainsKey(itemId))
            _inventory[itemId] += count;
        else
            _inventory[itemId] = count;

        CJsonManager.Instance.SaveItemChange(itemId, count);
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 아이템을 인벤토리에서 제거합니다.
    /// count가 현재 수량 이상이면 해당 아이템 항목 자체가 제거됩니다.
    /// </summary>
    public bool RemoveItem(int itemId, int count = 1)
    {
        if (!_inventory.TryGetValue(itemId, out int current)) return false;
        if (current < count) return false;

        current -= count;
        if (current <= 0)
            _inventory.Remove(itemId);
        else
            _inventory[itemId] = current;

        CJsonManager.Instance.SaveItemChange(itemId, -count);
        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>특정 아이템의 보유 수량을 반환합니다. 없으면 0을 반환합니다.</summary>
    public int GetItemCount(int itemId)
    {
        _inventory.TryGetValue(itemId, out int count);
        return count;
    }

    /// <summary>특정 아이템을 1개 이상 보유하고 있는지 여부를 반환합니다.</summary>
    public bool HasItem(int itemId, int requiredCount = 1) => GetItemCount(itemId) >= requiredCount;

    /// <summary>인벤토리에 있는 모든 아이템 ID 목록을 반환합니다.</summary>
    public IEnumerable<int> GetAllItemIds() => _inventory.Keys;

    /// <summary>
    /// 무기를 장착합니다. 인벤토리에 없는 무기는 장착할 수 없습니다.
    /// 장착 성공 시 OnWeaponEquipped 이벤트가 발생합니다.
    /// </summary>
    public bool EquipWeapon(int weaponId)
    {
        if (!HasItem(weaponId))
        {
            Debug.LogWarning($"[CInventorySystem] 인벤토리에 없는 무기를 장착하려 했습니다. ID: {weaponId}");
            return false;
        }

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
        for (int i = 0; i < saveData.inventoryIds.Count; i++)
        {
            int id    = saveData.inventoryIds[i];
            int count = i < saveData.inventoryCounts.Count ? saveData.inventoryCounts[i] : 1;
            if (count > 0) _inventory[id] = count;
        }

        _equippedWeaponId = saveData.equippedWeaponId;
        OnInventoryChanged?.Invoke();
    }

    #endregion
}
