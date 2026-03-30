using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System;


/*
��CInventoryManager
- �κ��丮 ������ ���̺�/�ε� ��� �Ŵ��� (json ���)
- ���� ���� ��������� �ϴ� ���븸 ��Ƶ�
*/

public class CInventoryManager : MonoBehaviour
{
    [SerializeField] private CUpgradeSO _upgradeSO = null;
    [SerializeField] private bool _showDebug = false;

    public static CInventoryManager Instance {  get; private set; }

    public List <CItemInstance> Inventory = new List<CItemInstance>(); // ���� ������ ���� �κ��丮 ����

    private CWeaponInstance _equippedWeapon; // ���� ��� ���� ���� ����

    // ������ SO ���� ���� ��ųʸ�
    private Dictionary<int, CItemDataSO> _itemDataCache = new Dictionary<int, CItemDataSO>();

    // Application.persistentDataPath : OS���� ������ ������ ���� ���� ��� Ž��. ���� �����ص� ������ ����
    private string SavePath => Path.Combine(Application.persistentDataPath, "save_data.json");

    // ���� ��� ���� ���� ���� ���� �뵵
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

                if (_showDebug) Debug.Log($"{item._itemData.ItemName}�� ������ InstanceID �ڵ� ���� : {item._itemData.ItemId}");
            }
        }

        // ����ȯ ��� �ڵ�
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


        if (_upgradeSO == null)
        {
            if (_showDebug) Debug.LogWarning("�κ��丮 �Ŵ��� : UpgradeSO �ʿ�");
        }
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // �׽�Ʈ�� ������Ʈ
    private void Update()
    {        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddItem(0, 100);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddItem(1, 1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ResetInventory();
        }
    }


    // �ְ� ��� ���� �ڵ� ���� (������ ������, ���� �ڵ� ���� ���� ��ư���� �ᵵ ��)
    private void AutoEquipWeapon()
    {
        // �κ��丮�� ������ ����ִ� ��쿡 ���� ����ڵ�
        if (Inventory ==  null || Inventory.Count == 0)
        {
            var weapon = _itemDataCache.First();

            AddItem(weapon.Value.Id);
        }

        // ���ⱺ �� �ְ� ������� ���͸�
        var bestWeapon = Inventory
            .OfType<CWeaponInstance>()
            .OrderByDescending(w => w._rank)
            .ThenBy(w => w._data.ItemId)
            .FirstOrDefault();

        if (_showDebug) Debug.Log($"���ⱺ �� �ְ� ������� ���͸� : {bestWeapon._itemData.ItemName}, {bestWeapon._rank}, {bestWeapon._instanceID}");

        if (bestWeapon != null)
        {
            bestWeapon._isEquipped = true;
            _equippedWeapon = bestWeapon;
            if (_showDebug) Debug.Log("���� �ڵ� ����");
        }

        SaveInventory(Inventory);
    }


    // ��ųʸ��� ���� ������ ��� SO �ϰ� �����Ͽ� �̸� ĳ�� (������ ���� ������ ���� ĳ��)
    private void PreloadItemData()
    {        
        CItemDataSO[] allItems = Resources.LoadAll<CItemDataSO>("SO/Items");

        foreach (var item in allItems)
        {
            _itemDataCache[item.Id] = item;
            if (_showDebug) Debug.Log($"{item.ItemId} ĳ��");
        }
    }


    // Json���� �κ��丮 �� ���� ����
    public void SaveInventory(List<CItemInstance> inventory)
    {
        Inventory = inventory;

        SortInventory();

        CInventorySaveData saveData = new CInventorySaveData();

        foreach (var item in Inventory)
        {
            CItemSaveData data = new CItemSaveData
            {
                itemID = item._itemData.Id,
                instanceID = item._instanceID
            };

            if (item is CWeaponInstance weapon)
            {
                data.rank = weapon._rank;
                data.isEquipped = weapon._isEquipped;
                data.type = EItemType.Weapon;
                data.upgrade = weapon._upgrade;
            }
            else if (item is CPotionInstance potion)
            {
                data.amount = potion._amount;
                data.type = EItemType.Potion;
            }
            else if (item is CScrollInstance scroll)
            {
                data.amount = scroll._amount;
                data.type = EItemType.Scroll;
            }

            saveData.items.Add(data);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);

        CInventoryUI.Instance.RefreshUI();

        if (_showDebug) Debug.Log($"���� �Ϸ� (���� ���: {SavePath})");
    }


    // ��ο� ����� �κ��丮 ���� �ε�
    public List<CItemInstance> LoadInventory()
    {
        if (!File.Exists(SavePath)) return new List<CItemInstance>(); // ������ ���ٸ� �� ����Ʈ ��ȯ

        string json = File.ReadAllText(SavePath).Trim();

        if (string.IsNullOrEmpty(json) || !json.StartsWith("{"))
        {
            Debug.LogWarning($"[CInventoryManager] 인벤토리 저장 파일이 손상되었습니다. 초기화합니다. ({SavePath})");
            File.Delete(SavePath);
            return new List<CItemInstance>();
        }

        CInventorySaveData saveData;
        try
        {
            saveData = JsonUtility.FromJson<CInventorySaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[CInventoryManager] JSON 파싱 실패, 초기화합니다. 오류: {e.Message}");
            File.Delete(SavePath);
            return new List<CItemInstance>();
        }
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
                else if (data.type == EItemType.Scroll)
                {
                    newItem = new CScrollInstance(originSO as CScrollDataSO, data.amount);
                }
                else if (data.type == EItemType.Weapon)
                {
                    var weapon = new CWeaponInstance(originSO as CWeaponDataSO);
                    weapon._rank = data.rank;
                    weapon._upgrade = data.upgrade;
                    weapon._isEquipped = data.isEquipped;
                    newItem = weapon;
                }

                newItem._instanceID = data.instanceID;
                loadedInventory.Add(newItem);
            }
        }

        // �ε�� ����Ʈ�� ���� ���� ���� ĳ��
        _equippedWeapon = loadedInventory
            .OfType<CWeaponInstance>()
            .FirstOrDefault(w => w._isEquipped);


        Inventory = loadedInventory;

        // �κ��丮 ����
        SortInventory();

        return Inventory;
    }

    
    // ���� �������� ���� ���� / �� ���� ����
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
        CInventoryUI.Instance.RefreshUI();
    }


    // ���� ���
    public void UsePotion(string targetInstanceID)
    {
        var potion = Inventory.Find(s => s._instanceID == targetInstanceID) as CPotionInstance;

        if (potion == null)
        {
            if (_showDebug) Debug.Log("��ȭ ���� : ������ �������� ����");
            return;
        }

        ReduceItemAmount(potion._instanceID, 1);

        SaveInventory(Inventory);
    }


    // ��ũ�� ���
    public void UseScroll(string targetInstanceID)
    {
        var scroll = Inventory.Find(s => s._itemData.ItemType == EItemType.Scroll) as CScrollInstance;

        if (scroll == null)
        {
            if (_showDebug) Debug.Log("��ȭ ���� : ��ũ���� �������� ����");
            return;
        }

        ReduceItemAmount(scroll._instanceID, 1);

        var target = Inventory.Find(i => i._instanceID == targetInstanceID) as CWeaponInstance;

        if (_upgradeSO.GetUpgradeResult(target._upgrade))
        {
            target._upgrade += 1;
            if (_showDebug) Debug.Log("��ȭ ����");
        }
        else
        {
            if (_showDebug) Debug.Log("��ȭ ����");
        }


        SaveInventory(Inventory);
    }


    // ������ ȹ�� �Լ�
    public void AddItem(int itemID, int count = 1, int rank = 0)
    { 
        if (!_itemDataCache.TryGetValue(itemID, out var originSO))
        {
            if (_showDebug) Debug.Log($"{itemID}�� SO�� ĳ�ÿ��� ã�� �� ����");
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
            if (_showDebug) Debug.Log($"{originSO.ItemName} : {count}�� ȹ��");
        }

        else if (originSO.ItemType == EItemType.Potion)
        {
            var existPotion = Inventory.Find(i => i._itemData.Id == itemID) as CPotionInstance;

            if (existPotion != null)
            {
                // ����
                /*
                if (existPotion._amount + count > existPotion._maxAmount)
                {
                    // ī��Ʈ���� �ִ뷮��ŭ ����
                    // ���� ���� �ִ�ġ�� ����
                    // ������ ī��Ʈ�� �ִ�ġ�� ������, ī��Ʈ = ������
                    // ������ ����ŭ Add �ݺ�
                
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
                    if (_showDebug) Debug.Log("���� �ִ�ġ = 999");
                }

                if (_showDebug) Debug.Log($"{originSO.ItemName} : {count}�� ȹ��, ���� : {existPotion._amount}");
            }
            else
            {
                Inventory.Add(new CPotionInstance(originSO as CPotionDataSO, count));
            }
        }

        else if (originSO.ItemType == EItemType.Scroll)
        {
            var existScroll = Inventory.Find(s => s._itemData.Id == itemID) as CScrollInstance;

            if (existScroll != null)
            {
                existScroll._amount += count;

                if (existScroll._amount + count > existScroll._maxAmount)
                {
                    existScroll._amount = existScroll._maxAmount;
                    if (_showDebug) Debug.Log("��ũ�� �ִ�ġ = 999");
                }

                if (_showDebug) Debug.Log($"{originSO.ItemName} : {count}�� ȹ��, ���� : {existScroll._amount}");
            }
            else
            {
                Inventory.Add(new CScrollInstance(originSO as CScrollDataSO, count));
            }
        }

        SaveInventory(Inventory);
    }


    // �κ��丮 ���� (�������� ����� ������ ����, �ʿ� �� Ȱ��)
    public void ResetInventory()
    {
        CWeaponInstance backupEquipped = _equippedWeapon;

        Inventory.Clear();

        if (backupEquipped != null)
        {
            Inventory.Add(_equippedWeapon);
            if (_showDebug) Debug.Log("�������� ���⸦ ������ ��� ���� ����");
        }
        else
        {
            // �� ��Ȳ�� ������ �ʵ��� ��� �ʿ�������, �ϴ� ����ó����
            if (_showDebug) Debug.Log("�������� ���Ⱑ ���� �κ��丮 ���� �ʱ�ȭ");
        }


        CInventoryUI.Instance.RefreshUI();
    }


    // Ư�� ������ ���� (������ ��)
    // �κ��丮 ���� UI�� �� �� Ȯ������ �� ���
    public void RemoveItem(string targetInstanceID)
    {
        var target = Inventory.Find(i => i._instanceID == targetInstanceID);
        
        if (target != null && target == _equippedWeapon)
        {
            if (_showDebug) Debug.Log("�������� ����� ���� �� ����");
            return;
        }

        Inventory.Remove(target);
        if (_showDebug) Debug.Log($"{target._itemData.ItemName} ���� �Ϸ�");

        SaveInventory(Inventory);

        CInventoryUI.Instance.RefreshUI();
    }


    // ������ �ִ� �������� ������ ������ŭ ����
    // 0���� �Ǹ� ���� ó��
    public void ReduceItemAmount(string targetInstanceID, int amount)
    {
        var target = Inventory.Find(i => i._instanceID == targetInstanceID);

        if (target != null && (target is CPotionInstance potion))
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

        else return;

        SaveInventory(Inventory);
    }

    // �κ��丮 ������ ���� �Լ�
    private void SortInventory()
    {
        Inventory = Inventory
            .OrderByDescending(i => (i as CWeaponInstance)?._isEquipped ?? false)   // 1. ���� ���� ���� �ֿ켱 ����
            //.ThenBy(i => (i as CItemInstance)._itemData.ItemType)                   // 2. ���ⱺ -> ���Ǳ� -> ��ũ��
            .ThenByDescending(i => (i as CWeaponInstance)?._rank ?? 0)              // 3. ���� �� ��޼�
            .ThenByDescending(i => (i as CWeaponInstance)?._upgrade ?? 0)           // 4. ���� �� ��ȭ ��
            //.ThenBy(i => i._itemData.Id)                                        // 5. �̸��� (��������)
            .ToList();
    }
}
