using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 플레이어 진행 데이터(CSaveData)의 JSON 저장·로드를 담당하는 싱글톤 매니저입니다.
/// ScriptableObject의 정적 밸런스 데이터는 다루지 않습니다.
/// 인벤토리·스킬·진행도·스탯 등 동적 상태 데이터만 직렬화합니다.
/// Application.persistentDataPath에 저장하여 플랫폼에 무관하게 동작합니다.
/// </summary>
public class CJsonManager : MonoBehaviour
{
    #region InspectorVariables

    [Header("저장 설정")]
    [SerializeField] private string _saveFileName = "save_data.json"; // 세이브 파일 이름
    [SerializeField] private bool _prettyPrint = false;                // 개발 중 true로 설정 시 가독성 좋은 JSON 출력

    #endregion

    #region Events

    /// <summary>저장 성공 시 발생합니다.</summary>
    public event Action OnSaveCompleted;

    /// <summary>로드 성공 시 로드된 CSaveData와 함께 발생합니다.</summary>
    public event Action<CSaveData> OnLoadCompleted;

    /// <summary>저장 또는 로드 중 오류 발생 시 오류 메시지와 함께 발생합니다.</summary>
    public event Action<string> OnError;

    #endregion

    #region PrivateVariables

    private string _savePath; // 전체 저장 경로 캐시

    #endregion

    #region Properties

    /// <summary>싱글톤 인스턴스. Awake 이후에만 유효합니다.</summary>
    public static CJsonManager Instance { get; private set; }

    /// <summary>현재 로드된 세이브 데이터. Load() 또는 GetOrCreateSaveData() 호출 이후 유효합니다.</summary>
    public CSaveData CurrentSaveData { get; private set; }

    /// <summary>persistentDataPath에 세이브 파일이 존재하는지 여부.</summary>
    public bool HasSaveFile => File.Exists(_savePath);

    /// <summary>세이브 파일의 전체 경로.</summary>
    public string SavePath => _savePath;

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
        _savePath = Path.Combine(Application.persistentDataPath, _saveFileName);
    }

    #endregion

    #region PublicMethods

    /// <summary>
    /// CSaveData를 JSON 파일로 저장합니다.
    /// 저장 완료 시 OnSaveCompleted 이벤트가 발생합니다.
    /// </summary>
    public void Save(CSaveData saveData)
    {
        if (saveData == null)
        {
            string error = "[CJsonManager] null 데이터는 저장할 수 없습니다.";
            Debug.LogError(error);
            OnError?.Invoke(error);
            return;
        }

        try
        {
            saveData.lastSavedTime = DateTime.UtcNow.ToString("o"); // ISO 8601 형식
            string json = JsonUtility.ToJson(saveData, _prettyPrint);
            File.WriteAllText(_savePath, json);
            CurrentSaveData = saveData;
            OnSaveCompleted?.Invoke();
            Debug.Log($"[CJsonManager] 저장 완료 → {_savePath}");
        }
        catch (Exception ex)
        {
            string error = $"[CJsonManager] 저장 실패: {ex.Message}";
            Debug.LogError(error);
            OnError?.Invoke(error);
        }
    }

    /// <summary>
    /// JSON 파일에서 CSaveData를 로드합니다.
    /// 파일이 없거나 손상되었으면 새 데이터를 생성해 반환합니다.
    /// 로드 완료 시 OnLoadCompleted 이벤트가 발생합니다.
    /// </summary>
    public CSaveData Load()
    {
        if (!HasSaveFile)
        {
            Debug.Log("[CJsonManager] 세이브 파일이 없습니다. 신규 데이터를 생성합니다.");
            CurrentSaveData = CreateNewSaveData();
            OnLoadCompleted?.Invoke(CurrentSaveData);
            return CurrentSaveData;
        }

        try
        {
            string json = File.ReadAllText(_savePath);
            CSaveData loaded = JsonUtility.FromJson<CSaveData>(json);

            if (loaded == null)
            {
                Debug.LogWarning("[CJsonManager] 역직렬화 실패. 신규 데이터를 생성합니다.");
                CurrentSaveData = CreateNewSaveData();
            }
            else
            {
                CurrentSaveData = loaded;
                Debug.Log($"[CJsonManager] 로드 완료 ← {_savePath} (버전: {loaded.saveVersion})");
            }

            OnLoadCompleted?.Invoke(CurrentSaveData);
            return CurrentSaveData;
        }
        catch (Exception ex)
        {
            string error = $"[CJsonManager] 로드 실패: {ex.Message}. 신규 데이터를 생성합니다.";
            Debug.LogError(error);
            OnError?.Invoke(error);
            CurrentSaveData = CreateNewSaveData();
            OnLoadCompleted?.Invoke(CurrentSaveData);
            return CurrentSaveData;
        }
    }

    /// <summary>
    /// CurrentSaveData가 있으면 반환하고, 없으면 Load()를 호출합니다.
    /// 초기화 흐름을 단순하게 유지하고 싶을 때 사용합니다.
    /// </summary>
    public CSaveData GetOrCreateSaveData()
    {
        if (CurrentSaveData != null) return CurrentSaveData;
        return Load();
    }

    /// <summary>
    /// 세이브 파일을 삭제하고 CurrentSaveData를 초기화합니다.
    /// 게임 초기화(뉴 게임) 시 사용합니다.
    /// </summary>
    public void DeleteSave()
    {
        if (!HasSaveFile)
        {
            Debug.LogWarning("[CJsonManager] 삭제할 세이브 파일이 없습니다.");
            return;
        }

        try
        {
            File.Delete(_savePath);
            CurrentSaveData = null;
            Debug.Log("[CJsonManager] 세이브 파일 삭제 완료.");
        }
        catch (Exception ex)
        {
            string error = $"[CJsonManager] 삭제 실패: {ex.Message}";
            Debug.LogError(error);
            OnError?.Invoke(error);
        }
    }

    /// <summary>
    /// 아이템 수량을 delta만큼 변경하고 즉시 저장합니다.
    /// delta가 양수이면 추가, 음수이면 차감입니다.
    /// </summary>
    public void SaveItemChange(int itemId, int delta)
    {
        EnsureSaveDataLoaded();
        CurrentSaveData.ChangeItemCount(itemId, delta);
        Save(CurrentSaveData);
    }

    /// <summary>스킬 레벨을 업데이트하고 즉시 저장합니다.</summary>
    public void SaveSkillLevel(int skillId, int newLevel)
    {
        EnsureSaveDataLoaded();
        CurrentSaveData.SetSkillLevel(skillId, newLevel);
        Save(CurrentSaveData);
    }

    /// <summary>장착 무기 ID를 업데이트하고 즉시 저장합니다.</summary>
    public void SaveEquippedWeapon(int weaponId)
    {
        EnsureSaveDataLoaded();
        CurrentSaveData.equippedWeaponId = weaponId;
        Save(CurrentSaveData);
    }

    /// <summary>플레이어 레벨과 경험치를 업데이트하고 즉시 저장합니다.</summary>
    public void SavePlayerProgress(int level, float exp, float hp, float mana)
    {
        EnsureSaveDataLoaded();
        CurrentSaveData.playerLevel = level;
        CurrentSaveData.playerExp   = exp;
        CurrentSaveData.currentHp   = hp;
        CurrentSaveData.currentMana = mana;
        Save(CurrentSaveData);
    }

    /// <summary>현재 스테이지 진행도를 업데이트하고 즉시 저장합니다.</summary>
    public void SaveStageProgress(int stageId, int killCount)
    {
        EnsureSaveDataLoaded();
        CurrentSaveData.currentStageId   = stageId;
        CurrentSaveData.currentKillCount = killCount;

        if (stageId > CurrentSaveData.highestStageId)
            CurrentSaveData.highestStageId = stageId;

        Save(CurrentSaveData);
    }

    #endregion

    #region PrivateMethods

    /// <summary>CurrentSaveData가 null이면 Load()를 호출하여 반드시 존재하게 합니다.</summary>
    private void EnsureSaveDataLoaded()
    {
        if (CurrentSaveData == null) Load();
    }

    /// <summary>기본값으로 채워진 신규 CSaveData를 생성합니다.</summary>
    private CSaveData CreateNewSaveData()
    {
        return new CSaveData
        {
            playerStatId     = 1,
            playerLevel      = 1,
            playerExp        = 0f,
            currentHp        = 100f,
            currentMana      = 100f,
            gold             = 0,
            currentStageId   = 1,
            highestStageId   = 1,
            currentKillCount = 0,
            equippedWeaponId = 0,
            skillPoints      = 0,
            saveVersion      = 1,
        };
    }

    #endregion
}
