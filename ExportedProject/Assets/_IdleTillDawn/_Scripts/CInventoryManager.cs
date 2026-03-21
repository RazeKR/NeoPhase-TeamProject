using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System;

// mvc 패턴 위배

/*
ㆍCInventorySaveManager
- 인벤토리 데이터 세이브/로드 담당 매니저 (json 사용)
- 무기 정보 저장용으로 일단 뼈대만 잡아둠
*/

public class CInventoryManager : MonoBehaviour
{
    [SerializeField] private Transform _slotParent = null;      // 인벤토리 슬롯이 들어갈 부모 객체
    [SerializeField] private GameObject _slotPrefab = null;     // 인벤토리 슬롯 프리팹
    [SerializeField] private GameObject _inventoryUI = null;    // 인벤토리 UI (창 On/Off 조절)

    public static CInventoryManager Instance {  get; private set; }

    public List <CItemInstance> Inventory = new List<CItemInstance>(); // 접근 가능한 현재 인벤토리 정보

    private CWeaponInstance _equippedWeapon; // 현재 장비 중인 무기 정보

    // 아이템 SO 정보 저장 딕셔너리
    private Dictionary<string, CItemDataSO> _itemDataCache = new Dictionary<string, CItemDataSO>();   

    // Application.persistentDataPath : OS별로 데이터 저장이 허용된 안전 경로 탐색. 게임 삭제해도 데이터 유지
    private string SavePath => Path.Combine(Application.persistentDataPath, "inventory.json");

    // 현재 장비 중인 무기 정보 공유 용도
    public CWeaponInstance EquippedWeapon => _equippedWeapon;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PreloadItemData();

        var loaded = LoadInventory();
        if (loaded.Count > 0) Inventory = loaded;

        foreach (var item in Inventory)
        {
            if (string.IsNullOrEmpty(item._instanceID))
            {
                item._instanceID = Guid.NewGuid().ToString();
                Debug.Log($"{item._itemData.ItemName}의 누락된 InstanceID 자동 생성 : {item._itemData.ItemId}");
            }
        }

        // 형변환 방어 코드
        for (int i = 0; i < Inventory.Count; i++)
        {
            if (Inventory[i] != null && !(Inventory[i] is CWeaponInstance) && !(Inventory[i] is CPotionInstance))
            {
                if (Inventory[i]._itemData is CWeaponDataSO weaponDataSO)
                {
                    Inventory[i] = new CWeaponInstance(weaponDataSO) { _instanceID = Inventory[i]._instanceID };
                }

                else if (Inventory[i]._itemData is CPotionDataSO potionDataSO)
                {
                    Inventory[i] = new CPotionInstance(potionDataSO, 1) { _instanceID = Inventory[i]._instanceID };
                }
            }
        }

        if (_equippedWeapon == null && Inventory.Count > 0)
        {
            AutoEquipWeapon();
        }


        if (_inventoryUI != null)
        {
            _inventoryUI.SetActive(false);
        }
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // 테스트용 업데이트
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddItem("weapon_03");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddItem("potion_01", 100);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ResetInventory();
        }
    }


    // 최고 등급 무기 자동 장착 (미장착 방지용, 차후 자동 무기 장착 버튼으로 써도 됨)
    private void AutoEquipWeapon()
    {
        // 무기군 중 최고 등급으로 필터링
        var bestWeapon = Inventory
            .OfType<CWeaponInstance>()
            .OrderByDescending(w => w._rank)
            .ThenBy(w => w._data.ItemId)
            .FirstOrDefault();

        Debug.Log($"무기군 중 최고 등급으로 필터링 : {bestWeapon._itemData.ItemName}, {bestWeapon._rank}, {bestWeapon._instanceID}");

        if (bestWeapon != null)
        {
            bestWeapon._isEquipped = true;
            _equippedWeapon = bestWeapon;
            Debug.Log("무기 자동 장착");
        }

        SaveInventory(Inventory);
    }


    // 딕셔너리에 무기 정보가 담긴 SO 일괄 저장하여 미리 캐싱 (종류가 적기 때문에 사전 캐싱)
    private void PreloadItemData()
    {        
        CItemDataSO[] allItems = Resources.LoadAll<CItemDataSO>("SO/Items");

        foreach (var item in allItems)
        {
            _itemDataCache[item.ItemId] = item;
            Debug.Log($"{item.ItemId} 캐시");
        }
    }


    // Json으로 인벤토리 내 정보 저장
    public void SaveInventory(List<CItemInstance> inventory)
    {
        Inventory = inventory;

        SortInventory();

        CInventorySaveData saveData = new CInventorySaveData();

        foreach (var item in Inventory)
        {
            CItemSaveData data = new CItemSaveData
            {
                itemID = item._itemData.ItemId,
                instanceID = item._instanceID
            };

            if (item is CWeaponInstance weapon)
            {
                data.rank = weapon._rank;
                data.isEquipped = weapon._isEquipped;
                data.type = EItemType.Weapon;
            }
            else if (item is CPotionInstance potion)
            {
                data.amount = potion._amount;
                data.type = EItemType.Potion;
            }

            saveData.items.Add(data);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);

        RefreshUI();

        Debug.Log($"저장 완료 (저장 경로: {SavePath})");
    }


    // 경로에 저장된 인벤토리 정보 로드
    public List<CItemInstance> LoadInventory()
    {
        if (!File.Exists(SavePath)) return new List<CItemInstance>(); // 파일이 없다면 빈 리스트 반환

        string json = File.ReadAllText(SavePath);
        CInventorySaveData saveData = JsonUtility.FromJson<CInventorySaveData>(json);
        List<CItemInstance> loadedInventory = new List<CItemInstance>();

        foreach (var data in saveData.items)
        {
            if (_itemDataCache.TryGetValue(data.itemID, out CItemDataSO originSO))
            {
                CItemInstance newItem = null;
                
                if (data.type == EItemType.Potion)
                {
                    newItem = new CPotionInstance(originSO as CPotionDataSO, data.amount);
                }
                else if (data.type == EItemType.Weapon)
                {
                    var weapon = new CWeaponInstance(originSO as CWeaponDataSO);
                    weapon._rank = data.rank;
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


        Inventory = loadedInventory;

        // 인벤토리 정렬
        SortInventory();

        return Inventory;
    }

    
    // 기존 장착중인 무기 해제 / 새 무기 장착
    public void SwapWeapon(string targetInstanceID)
    {
        if (_equippedWeapon != null)
        {
            _equippedWeapon._isEquipped = false;
        }

        var target = Inventory.Find(i => i._instanceID == targetInstanceID) as CWeaponInstance;

        if (target != null)
        {
            target._isEquipped = true;
            _equippedWeapon = target;
        }

        SortInventory();
        SaveInventory(Inventory);
        RefreshUI();
    }


    // 포션 사용
    // 포션의 스택 반영 (0개가 되면 삭제 처리)
    public void UsePotion(string targetInstanceID)
    {
        Debug.Log("포션사용 임시 로그");
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

        // 인벤토리를 순회하며 생성
        foreach (var item in Inventory)
        {
            GameObject go = Instantiate(_slotPrefab, _slotParent);
            CInventorySlot slot = go.GetComponent<CInventorySlot>();

            slot.SetSlot(item);
        }

        Debug.Log("UI 최신화 완료");
    }



    // 아이템 획득 함수
    public void AddItem(string itemID, int count = 1, int rank = 0)
    { 
        if (!_itemDataCache.TryGetValue(itemID, out var originSO))
        {
            Debug.Log($"{itemID}의 SO를 캐시에서 찾을 수 없음");
            return;
        }

        if (originSO.ItemType == EItemType.Weapon)
        {
            for (int i = 0; i < count; i++)
            {
                CWeaponInstance w = new CWeaponInstance(originSO as CWeaponDataSO);
                
                w._rank = rank;

                Inventory.Add(w);
            }
            Debug.Log($"{originSO.ItemName} : {count}개 획득");
        }

        else if (originSO.ItemType == EItemType.Potion)
        {
            var existPotion = Inventory.Find(i => i._itemData.ItemId == itemID) as CPotionInstance;

            if (existPotion != null)
            {
                // 보류
                /*
                if (existPotion._amount + count > existPotion._maxAmount)
                {
                    // 카운트에서 최대량만큼 차감
                    // 기존 포션 최대치로 설정
                    // 차감한 카운트를 최대치로 나누고, 카운트 = 나머지
                    // 나눗셈 값만큼 Add 반복
                
                    count -= (byte)(existPotion._maxAmount - existPotion._amount);
                
                    existPotion._amount = existPotion._maxAmount;
                
                
                    int bundle = count / existPotion._maxAmount;
                    int left = count % existPotion._maxAmount;
                
                    for (int i = 0; i < bundle; i++)
                    {
                        Inventory.Add(new CPotionInstance(originSO as CPotionDataSO, count));
                    }
                    ...
                }
                */
                
                existPotion._amount += count;

                if (existPotion._amount + count > existPotion._maxAmount)
                {
                    existPotion._amount = existPotion._maxAmount;
                    Debug.Log("포션 최대치 = 999");
                }

                Debug.Log($"{originSO.ItemName} : {count}개 획득, 수량 : {existPotion._amount}");
            }
            else
            {
                Inventory.Add(new CPotionInstance(originSO as CPotionDataSO, count));
            }
        }

        SaveInventory(Inventory);
    }


    // 인벤토리 리셋 (장착중인 무기는 버리지 않음, 필요 시 활용)
    public void ResetInventory()
    {
        CWeaponInstance backupEquipped = _equippedWeapon;

        Inventory.Clear();

        if (backupEquipped != null)
        {
            Inventory.Add(_equippedWeapon);
            Debug.Log("장착중인 무기를 제외한 모든 무기 제거");
        }
        else
        {
            // 이 상황은 생기지 않도록 방어 필요하지만, 일단 예외처리함
            Debug.Log("장착중인 무기가 없어 인벤토리 완전 초기화");
        }


        RefreshUI();
    }


    // 특정 아이템 삭제 (버리기 등)
    // 인벤토리 슬롯 UI를 좀 더 확장했을 때 사용
    public void RemoveItem(string targetInstanceID)
    {
        var target = Inventory.Find(i => i._instanceID == targetInstanceID);
        
        if (target != null && target == _equippedWeapon)
        {
            Debug.Log("장착중인 무기는 버릴 수 없음");
            return;
        }

        Inventory.Remove(target);
        Debug.Log($"{target._itemData.ItemName} 삭제 완료");

        SaveInventory(Inventory);

        RefreshUI();
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



    // 인벤토리 데이터 정렬 함수
    private void SortInventory()
    {
        Inventory = Inventory
            .OrderByDescending(i => (i as CWeaponInstance)?._isEquipped ?? false)   // 1. 장착 중인 무기 최우선 정렬
            .ThenBy(i => i._itemData.ItemType)                                      // 2. 무기군 -> 포션군
            .ThenByDescending(i => (i as CWeaponInstance)?._rank ?? 0)              // 3. 무기 내 등급순
            .ThenBy(i => i._itemData.ItemId)                                        // 4. 이름별 (오름차순)
            .ToList();
    }
}
