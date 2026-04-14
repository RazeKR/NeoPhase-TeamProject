using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// 플레이어가 보유한 펫 인스턴스를 런타임에서 관리하는 싱글톤 매니저입니다.
/// CInventorySystemJ(무기 인벤토리)와 동일한 설계 패턴을 따릅니다.
///
/// [역할]
///   - 펫 추가 / 제거 / 장착
///   - 세이브 데이터 직렬화 (SyncAndSave)
///   - 세이브 데이터 역직렬화 복구 (RestoreFromSaveData)
///   - CDataManager를 통한 int ID → CPetDataSO 조회
/// </summary>
public class CPetInventorySystem : MonoBehaviour
{
    #region Events

    /// <summary>펫 인벤토리가 변경될 때 발생합니다. UI 갱신에 사용합니다.</summary>
    public event System.Action OnPetInventoryChanged;

    /// <summary>펫이 장착될 때 발생합니다. 인자: 장착된 펫 인스턴스</summary>
    public event System.Action<CPetInstance> OnPetEquipped;

    /// <summary>펫이 해제될 때 발생합니다.</summary>
    public event System.Action OnPetUnequipped;

    #endregion

    #region Private Variables

    private List<CPetInstance> _pets = new List<CPetInstance>();
    private CPetInstance _equippedPet = null;

    private const int BaseCapacity = 30;

    #endregion

    #region Properties

    public static CPetInventorySystem Instance { get; private set; }

    public List<CPetInstance> Pets     => _pets;
    public CPetInstance EquippedPet    => _equippedPet;

    public int MaxCapacity  => BaseCapacity;
    public int UsedCapacity => _pets.Count;
    public bool IsFull      => _pets.Count >= BaseCapacity;

    #endregion

    #region Unity Methods

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
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted += RestoreFromSaveData;

        if (CJsonManager.Instance?.CurrentSaveData != null)
            RestoreFromSaveData(CJsonManager.Instance.CurrentSaveData);
    }

    private void OnDestroy()
    {
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted -= RestoreFromSaveData;
    }

    #endregion

    #region Public Methods

    /// <summary>int ID로 펫을 추가합니다. CDataManager에서 CPetDataSO를 조회합니다.</summary>
    public void AddPet(int petItemId, int rank = 0)
    {
        if (IsFull)
        {
            CDebug.LogWarning($"[CPetInventorySystem] 펫 인벤토리가 가득 찼습니다. ({MaxCapacity}칸)");
            return;
        }

        CPetDataSO so = CDataManager.Instance.GetPet(petItemId);
        if (so == null) return;

        CPetInstance instance = new CPetInstance(so, rank);
        _pets.Add(instance);

        SyncAndSave();
        OnPetInventoryChanged?.Invoke();
    }

    /// <summary>instanceID로 펫을 제거합니다. 장착 중인 펫은 제거할 수 없습니다.</summary>
    public bool RemovePet(string instanceID)
    {
        CPetInstance target = _pets.Find(p => p._instanceID == instanceID);
        if (target == null) return false;
        if (target._isEquipped) return false;

        _pets.Remove(target);
        SyncAndSave();
        OnPetInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>instanceID로 펫을 장착합니다. 기존 장착 펫은 해제됩니다.</summary>
    public bool EquipPet(string instanceID)
    {
        CPetInstance target = _pets.Find(p => p._instanceID == instanceID);
        if (target == null) return false;
        if (target._isEquipped) return false;

        if (_equippedPet != null)
            _equippedPet._isEquipped = false;

        target._isEquipped = true;
        _equippedPet = target;

        SyncAndSave();
        OnPetInventoryChanged?.Invoke();
        OnPetEquipped?.Invoke(_equippedPet);
        return true;
    }

    /// <summary>현재 장착된 펫을 해제합니다.</summary>
    public bool UnequipPet()
    {
        if (_equippedPet == null) return false;

        _equippedPet._isEquipped = false;
        _equippedPet = null;

        SyncAndSave();
        OnPetInventoryChanged?.Invoke();
        OnPetUnequipped?.Invoke();
        return true;
    }

    /// <summary>인벤토리 목록을 세이브 데이터와 동기화하고 저장합니다.</summary>
    public void SyncAndSave()
    {
        if (CJsonManager.Instance == null) return;

        CSaveData data = CJsonManager.Instance.GetOrCreateSaveData();
        data.petInventorySaveData.pets.Clear();

        foreach (CPetInstance pet in _pets)
        {
            data.petInventorySaveData.pets.Add(new CPetSaveData
            {
                itemID     = pet._itemData.Id,
                instanceID = pet._instanceID,
                rank       = pet._rank,
                upgrade    = pet._upgrade,
                isEquipped = pet._isEquipped,
            });
        }

        CJsonManager.Instance.Save(data);
    }

    /// <summary>
    /// instanceID에 해당하는 펫을 1단계 강화합니다.
    /// 최대 강화(+10)이거나 인스턴스를 찾지 못하면 false를 반환합니다.
    /// </summary>
    public bool UpgradePet(string instanceID)
    {
        CPetInstance target = _pets.Find(p => p._instanceID == instanceID);
        if (target == null) return false;
        if (target.IsMaxUpgrade) return false;

        target._upgrade++;
        SyncAndSave();
        OnPetInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>int ID로 CPetDataSO를 반환합니다.</summary>
    public CPetDataSO GetPetData(int petItemId) => CDataManager.Instance.GetPet(petItemId);

    #endregion

    #region Private Methods

    /// <summary>CSaveData에서 펫 인벤토리 상태를 복구합니다.</summary>
    private void RestoreFromSaveData(CSaveData saveData)
    {
        if (saveData?.petInventorySaveData == null) return;

        _pets.Clear();
        _equippedPet = null;

        foreach (CPetSaveData sData in saveData.petInventorySaveData.pets)
        {
            CPetDataSO so = CDataManager.Instance.GetPet(sData.itemID);
            if (so == null)
            {
                CDebug.LogWarning($"[CPetInventorySystem] ID={sData.itemID} CPetDataSO를 CDataManager에서 찾을 수 없습니다.");
                continue;
            }

            CPetInstance instance = new CPetInstance(so, sData.instanceID)
            {
                _rank       = sData.rank,
                _upgrade    = sData.upgrade,
                _isEquipped = sData.isEquipped,
            };
            _pets.Add(instance);

            if (sData.isEquipped)
                _equippedPet = instance;
        }

        OnPetInventoryChanged?.Invoke();
    }

    #endregion
}
