using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 ScriptableObject 데이터를 중앙에서 관리하는 싱글톤 매니저입니다.
/// Player 데이터는 Resources/_SO/Player 경로에서 자동 로드됩니다.
/// 나머지 SO는 Inspector 리스트에 등록하면 Awake에서 Dictionary로 캐싱합니다.
/// 게임 내 모든 정적 데이터 접근은 반드시 이 매니저를 통해서만 이루어집니다.
/// </summary>
public class CDataManager : MonoBehaviour
{
    #region InspectorVariables

    [Header("─── Monster ────────────────────────────────────")]
    [SerializeField] private List<CEnemyDataSO> _monsterList = new();  // 일반 몬스터 SO 목록

    [Header("─── Boss ───────────────────────────────────────")]
    [SerializeField] private List<CBossDataSO> _bossList = new();      // 보스 SO 목록

    [Header("─── Stage ──────────────────────────────────────")]
    [SerializeField] private List<CStageDataSO> _stageList = new();    // 스테이지 SO 목록

    [Header("─── Item (무기 제외 일반 아이템) ────────────────")]
    [SerializeField] private List<CItemDataSO> _itemList = new();      // 아이템 SO 목록 (포션, 스크롤 등)

    [Header("─── Weapon ─────────────────────────────────────")]
    [SerializeField] private List<CWeaponDataSO> _weaponList = new();  // 무기 SO 목록

    [Header("─── Skill ──────────────────────────────────────")]
    [SerializeField] private List<CSkillDataSO> _skillList = new();    // 스킬 SO 목록

    #endregion

    #region Events

    /// <summary>모든 데이터 초기화가 완료되었을 때 발생합니다.</summary>
    public event Action OnDataInitialized;

    #endregion

    #region PrivateVariables

    /// <summary>Resources.LoadAll로 자동 로드하는 Player SO 경로. Assets/Resources 하위 상대 경로입니다.</summary>
    private const string PlayerSOPath = "_SO/Player";         // 플레이어 SO 자동 로드 경로
    private const string StageSOPath  = "_StageDataSO";       // 스테이지 SO 자동 로드 경로 (하위 폴더 포함)

    private Dictionary<int, CPlayerDataSO> _playerDict   = new(); // 플레이어 데이터 캐시
    private Dictionary<int, CEnemyDataSO>  _monsterDict  = new(); // 일반 몬스터 캐시
    private Dictionary<int, CBossDataSO>   _bossDict     = new(); // 보스 캐시
    private Dictionary<int, CStageDataSO>  _stageDict    = new(); // 스테이지 캐시
    private Dictionary<int, CItemDataSO>   _allItemDict  = new(); // 무기 포함 전체 아이템 캐시
    private Dictionary<int, CItemDataSO>   _itemDict     = new(); // 일반 아이템 캐시 (무기 제외)
    private Dictionary<int, CWeaponDataSO> _weaponDict   = new(); // 무기 캐시
    private Dictionary<int, CSkillDataSO>  _skillDict    = new(); // 스킬 캐시

    private bool _isInitialized = false; // 초기화 완료 여부

    #endregion

    #region Properties

    /// <summary>싱글톤 인스턴스. Awake 이후에만 유효합니다.</summary>
    public static CDataManager Instance { get; private set; }

    /// <summary>모든 SO Dictionary 초기화 완료 여부.</summary>
    public bool IsInitialized => _isInitialized;

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
        InitAllDictionaries();
    }

    #endregion

    #region PublicMethods

    /// <summary>
    /// 플레이어 데이터를 ID로 반환합니다.
    /// Resources/_SO/Player 경로에서 자동 로드된 CPlayerDataSO를 반환합니다.
    /// </summary>
    public CPlayerDataSO GetPlayerData(int id)
    {
        if (TryGetData(_playerDict, id, out CPlayerDataSO data)) return data;
        Debug.LogError($"[CDataManager] PlayerData 없음 - ID: {id} (경로: Resources/{PlayerSOPath})");
        return null;
    }

    /// <summary>일반 몬스터 데이터를 ID로 반환합니다.</summary>
    public CEnemyDataSO GetMonster(int id)
    {
        if (TryGetData(_monsterDict, id, out CEnemyDataSO data)) return data;
        Debug.LogError($"[CDataManager] MonsterData 없음 - ID: {id}");
        return null;
    }

    /// <summary>보스 데이터를 ID로 반환합니다.</summary>
    public CBossDataSO GetBoss(int id)
    {
        if (TryGetData(_bossDict, id, out CBossDataSO data)) return data;
        Debug.LogError($"[CDataManager] BossData 없음 - ID: {id}");
        return null;
    }

    /// <summary>스테이지 데이터를 ID로 반환합니다.</summary>
    public CStageDataSO GetStage(int id)
    {
        if (TryGetData(_stageDict, id, out CStageDataSO data)) return data;
        Debug.LogError($"[CDataManager] StageData 없음 - ID: {id}");
        return null;
    }

    /// <summary>
    /// 아이템 데이터를 ID로 반환합니다. 무기를 포함한 모든 아이템 타입을 조회합니다.
    /// 무기 타입이 필요하면 GetWeapon(id)를 사용합니다.
    /// </summary>
    public CItemDataSO GetItem(int id)
    {
        if (TryGetData(_allItemDict, id, out CItemDataSO data)) return data;
        Debug.LogError($"[CDataManager] ItemData 없음 - ID: {id}");
        return null;
    }

    /// <summary>무기 데이터를 ID로 반환합니다. CWeaponDataSO 타입으로 직접 반환됩니다.</summary>
    public CWeaponDataSO GetWeapon(int id)
    {
        if (TryGetData(_weaponDict, id, out CWeaponDataSO data)) return data;
        Debug.LogError($"[CDataManager] WeaponData 없음 - ID: {id}");
        return null;
    }

    /// <summary>스킬 데이터를 ID로 반환합니다.</summary>
    public CSkillDataSO GetSkill(int id)
    {
        if (TryGetData(_skillDict, id, out CSkillDataSO data)) return data;
        Debug.LogError($"[CDataManager] SkillData 없음 - ID: {id}");
        return null;
    }

    /// <summary>로드된 모든 플레이어 데이터를 읽기 전용 Dictionary로 반환합니다.</summary>
    public IReadOnlyDictionary<int, CPlayerDataSO> GetAllPlayerData() => _playerDict;

    /// <summary>등록된 모든 몬스터 데이터를 읽기 전용 Dictionary로 반환합니다.</summary>
    public IReadOnlyDictionary<int, CEnemyDataSO> GetAllMonsters() => _monsterDict;

    /// <summary>등록된 모든 스킬 데이터를 읽기 전용 Dictionary로 반환합니다.</summary>
    public IReadOnlyDictionary<int, CSkillDataSO> GetAllSkills() => _skillDict;

    /// <summary>무기를 포함한 모든 아이템 데이터를 읽기 전용 Dictionary로 반환합니다.</summary>
    public IReadOnlyDictionary<int, CItemDataSO> GetAllItems() => _allItemDict;

    /// <summary>
    /// StageIndex((월드-1)×10+(스테이지-1)) 기준으로 스테이지 데이터를 반환합니다.
    /// CGameManager의 currentStageIndex와 CStageDataSO.StageIndex가 일치하는 항목을 찾습니다.
    /// 없으면 null을 반환합니다.
    /// </summary>
    public CStageDataSO GetStageByIndex(int stageIndex)
    {
        foreach (CStageDataSO stage in _stageDict.Values)
        {
            if (stage.StageIndex == stageIndex) return stage;
        }
        Debug.LogError($"[CDataManager] StageIndex {stageIndex}에 해당하는 StageData가 없습니다.");
        return null;
    }

    /// <summary>
    /// StageIndex 기준으로 스테이지 데이터를 조회합니다.
    /// 없으면 에러 로그 없이 false를 반환합니다. (ValidateStageIndex 등 유효성 검사용)
    /// </summary>
    public bool TryGetStageByIndex(int stageIndex, out CStageDataSO result)
    {
        foreach (CStageDataSO stage in _stageDict.Values)
        {
            if (stage.StageIndex == stageIndex)
            {
                result = stage;
                return true;
            }
        }
        result = null;
        return false;
    }

    /// <summary>
    /// 모든 Dictionary를 강제 재초기화합니다.
    /// 런타임 중 SO 목록이 변경되었을 때 호출합니다.
    /// </summary>
    public void ForceReinitialize()
    {
        ClearAllDictionaries();
        InitAllDictionaries();
        Debug.Log("[CDataManager] 강제 재초기화 완료.");
    }

    #endregion

    #region PrivateMethods

    /// <summary>모든 SO를 로드하여 Dictionary로 캐싱합니다.</summary>
    private void InitAllDictionaries()
    {
        LoadPlayerDataFromResources();
        // 1차: Inspector _stageList에 직접 연결된 SO 등록 (DataManager 프리팹 Inspector)
        RegisterList(_stageList, _stageDict, "Stage");
        // 2차: Resources/_StageDataSO 하위 전체 자동 로드 (파일만 있으면 자동 인식, 중복 시 경고 후 스킵)
        LoadStageDataFromResources();
        RegisterList(_monsterList, _monsterDict, "Monster");
        RegisterList(_bossList,    _bossDict,    "Boss");
        RegisterList(_itemList,    _itemDict,    "Item");
        RegisterList(_skillList,   _skillDict,   "Skill");
        RegisterWeaponList();

        _isInitialized = true;
        OnDataInitialized?.Invoke();
        Debug.Log($"[CDataManager] 초기화 완료 - Player:{_playerDict.Count} Monster:{_monsterDict.Count} " +
                  $"Boss:{_bossDict.Count} Stage:{_stageDict.Count} Item:{_allItemDict.Count} Skill:{_skillDict.Count}");
    }

    /// <summary>
    /// Resources/_StageDataSO 하위의 모든 CStageDataSO를 자동 로드합니다.
    /// Stage1, Stage2 등 하위 폴더 구조와 관계없이 전부 스캔합니다.
    /// Inspector _stageList에 별도 등록 없이 파일 배치만으로 동작합니다.
    /// </summary>
    private void LoadStageDataFromResources()
    {
        CStageDataSO[] loaded = Resources.LoadAll<CStageDataSO>(StageSOPath);

        if (loaded == null || loaded.Length == 0)
        {
            Debug.LogWarning($"[CDataManager] Resources/{StageSOPath} 경로에서 CStageDataSO를 찾을 수 없습니다.");
            return;
        }

        foreach (CStageDataSO stage in loaded)
        {
            if (stage == null) continue;

            if (_stageDict.ContainsKey(stage.Id))
            {
                Debug.LogWarning($"[CDataManager] StageData 중복 ID 발견: {stage.Id} ({stage.name}). 건너뜁니다.");
                continue;
            }

            _stageDict.Add(stage.Id, stage);
        }

        Debug.Log($"[CDataManager] StageData 자동 로드 완료 - {_stageDict.Count}개 (경로: Resources/{StageSOPath})");
    }

    /// <summary>
    /// Resources/_SO/Player 경로의 모든 CPlayerDataSO 에셋을 자동 로드합니다.
    /// Inspector 등록 없이 해당 경로에 에셋을 배치하는 것만으로 자동 인식됩니다.
    /// </summary>
    private void LoadPlayerDataFromResources()
    {
        CPlayerDataSO[] loaded = Resources.LoadAll<CPlayerDataSO>(PlayerSOPath);

        if (loaded == null || loaded.Length == 0)
        {
            Debug.LogWarning($"[CDataManager] Resources/{PlayerSOPath} 경로에서 CPlayerDataSO를 찾을 수 없습니다.");
            return;
        }

        foreach (CPlayerDataSO playerData in loaded)
        {
            if (playerData == null) continue;

            if (_playerDict.ContainsKey(playerData.Id))
            {
                Debug.LogWarning($"[CDataManager] PlayerData 중복 ID 발견: {playerData.Id} ({playerData.name}). 건너뜁니다.");
                continue;
            }

            _playerDict.Add(playerData.Id, playerData);
        }

        Debug.Log($"[CDataManager] PlayerData 자동 로드 완료 - {_playerDict.Count}개 (경로: Resources/{PlayerSOPath})");
    }

    /// <summary>
    /// SO 리스트를 순회하여 Dictionary에 등록합니다.
    /// null 항목 및 중복 ID를 감지하면 경고 후 건너뜁니다.
    /// </summary>
    private void RegisterList<T>(List<T> list, Dictionary<int, T> dict, string typeName)
        where T : CBaseDataSO
    {
        if (list == null) return;

        foreach (T item in list)
        {
            if (item == null)
            {
                Debug.LogWarning($"[CDataManager] {typeName} 목록에 null 항목이 있습니다. 건너뜁니다.");
                continue;
            }

            if (dict.ContainsKey(item.Id))
            {
                Debug.LogWarning($"[CDataManager] {typeName} 중복 ID 발견: {item.Id} ({item.name}). 건너뜁니다.");
                continue;
            }

            dict.Add(item.Id, item);

            // 일반 아이템은 전체 아이템 캐시에도 함께 등록
            if (item is CItemDataSO itemData)
            {
                if (_allItemDict.ContainsKey(item.Id))
                {
                    Debug.LogWarning($"[CDataManager] Item 전체 캐시 중복 ID: {item.Id}. 건너뜁니다.");
                    continue;
                }
                _allItemDict.Add(item.Id, itemData);
            }
        }
    }

    /// <summary>
    /// 무기 리스트를 _weaponDict와 _allItemDict 양쪽에 등록합니다.
    /// CWeaponDataSO는 CItemDataSO를 상속하므로 GetItem()으로도 조회 가능합니다.
    /// </summary>
    private void RegisterWeaponList()
    {
        if (_weaponList == null) return;

        foreach (CWeaponDataSO weapon in _weaponList)
        {
            if (weapon == null)
            {
                Debug.LogWarning("[CDataManager] Weapon 목록에 null 항목이 있습니다. 건너뜁니다.");
                continue;
            }

            if (_weaponDict.ContainsKey(weapon.Id))
            {
                Debug.LogWarning($"[CDataManager] Weapon 중복 ID 발견: {weapon.Id} ({weapon.name}). 건너뜁니다.");
                continue;
            }

            _weaponDict.Add(weapon.Id, weapon);

            if (_allItemDict.ContainsKey(weapon.Id))
            {
                Debug.LogWarning($"[CDataManager] Item 전체 캐시에 Weapon ID 충돌: {weapon.Id}. 건너뜁니다.");
                continue;
            }

            _allItemDict.Add(weapon.Id, weapon);
        }
    }

    /// <summary>초기화 완료 여부와 키 존재를 검증한 후 Dictionary 값을 반환합니다.</summary>
    private bool TryGetData<T>(Dictionary<int, T> dict, int id, out T result)
    {
        result = default;

        if (!_isInitialized)
        {
            Debug.LogError("[CDataManager] 초기화 전에 데이터를 요청했습니다. Awake 이후에 접근하세요.");
            return false;
        }

        return dict.TryGetValue(id, out result);
    }

    /// <summary>모든 Dictionary를 비우고 초기화 상태를 해제합니다.</summary>
    private void ClearAllDictionaries()
    {
        _playerDict.Clear();
        _monsterDict.Clear();
        _bossDict.Clear();
        _stageDict.Clear();
        _allItemDict.Clear();
        _itemDict.Clear();
        _weaponDict.Clear();
        _skillDict.Clear();
        _isInitialized = false;
    }

    #endregion
}
