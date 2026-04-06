using System;
using UnityEngine;

/// <summary>
/// 골드·다이아 재화를 통합 관리하는 싱글톤 매니저입니다.
/// CSaveData를 통해 CJsonManager와 연동하여 저장/로드합니다.
///
/// [골드 획득 경로]
///   - 몬스터 확정 드랍 (CEnemyDataSO의 min~max 범위 랜덤, CSpawnManager 호출)
///   - 보스 처치 보상 (CBossDataSO.BossGoldReward, CBossManager 호출)
///   - 스테이지 클리어 보상 (CStageDataSO.ClearGoldReward, CStageManager 호출)
///   - 다이아로 골드 구입 (추후 상점 UI에서 BuyGoldWithDiamond 호출)
///
/// [초기화]
///   P키 → CGameManager.ResetAllData() → 이 오브젝트 Destroy → 새 씬에서 재생성 → gold=0 로드
/// </summary>
public class CGoldManager : MonoBehaviour
{
    #region Singleton

    public static CGoldManager Instance { get; private set; }

    /// <summary>씬 로드 전 자동 생성. CGameManager와 동일한 패턴을 따릅니다.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;
        new GameObject("GoldManager").AddComponent<CGoldManager>();
    }

    #endregion

    #region Events

    /// <summary>골드 수량이 변경될 때 발행됩니다. 인자: 변경 후 골드 수량</summary>
    public event Action<int> OnGoldChanged;

    /// <summary>다이아 수량이 변경될 때 발행됩니다. 인자: 변경 후 다이아 수량</summary>
    public event Action<int> OnDiamondChanged;

    #endregion

    #region Private Variables

    private int _gold;
    private int _diamond;

    #endregion

    #region Properties

    /// <summary>현재 보유 골드</summary>
    public int Gold    => _gold;

    /// <summary>현재 보유 다이아</summary>
    public int Diamond => _diamond;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFromSave();
    }

    #endregion

    #region Public Methods — Gold

    /// <summary>골드를 추가합니다. 음수 또는 0은 무시합니다.</summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        _gold += amount;
        OnGoldChanged?.Invoke(_gold);
        SaveToData();
    }

    /// <summary>
    /// 골드를 소비합니다.
    /// 보유량 부족 시 소비하지 않고 false를 반환합니다.
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (amount <= 0 || _gold < amount) return false;
        _gold -= amount;
        OnGoldChanged?.Invoke(_gold);
        SaveToData();
        return true;
    }

    #endregion

    #region Public Methods — Diamond

    /// <summary>다이아를 추가합니다. (추후 상점 UI에서 버튼 클릭 시 호출)</summary>
    public void AddDiamond(int amount)
    {
        if (amount <= 0) return;
        _diamond += amount;
        OnDiamondChanged?.Invoke(_diamond);
        SaveToData();
    }

    /// <summary>
    /// 다이아를 소비합니다.
    /// 보유량 부족 시 소비하지 않고 false를 반환합니다.
    /// </summary>
    public bool SpendDiamond(int amount)
    {
        if (amount <= 0 || _diamond < amount) return false;
        _diamond -= amount;
        OnDiamondChanged?.Invoke(_diamond);
        SaveToData();
        return true;
    }

    /// <summary>
    /// 다이아 diamondCost개를 소비하여 골드 goldAmount를 획득합니다.
    /// 상점 UI에서 호출합니다. 다이아 부족 시 false를 반환합니다.
    /// </summary>
    public bool BuyGoldWithDiamond(int diamondCost, int goldAmount)
    {
        if (!SpendDiamond(diamondCost)) return false;
        AddGold(goldAmount);
        return true;
    }

    #endregion

    #region Private Methods

    /// <summary>CJsonManager의 CSaveData에서 골드·다이아를 로드합니다.</summary>
    private void LoadFromSave()
    {
        if (CJsonManager.Instance == null) return;
        CSaveData saveData = CJsonManager.Instance.GetOrCreateSaveData();
        _gold    = saveData.gold;
        _diamond = saveData.diamond;
    }

    /// <summary>현재 골드·다이아를 CSaveData에 반영하고 즉시 저장합니다.</summary>
    private void SaveToData()
    {
        if (CJsonManager.Instance == null) return;
        CSaveData saveData = CJsonManager.Instance.GetOrCreateSaveData();
        saveData.gold    = _gold;
        saveData.diamond = _diamond;
        CJsonManager.Instance.Save(saveData);
    }

    #endregion
}
