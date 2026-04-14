using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 펫 소환 탭 패널을 관리합니다.
///
/// [소환 비용]
///   1회 소환  → 소환권 1개 소모, 펫 1마리  → RevealPanel_1  오픈
///   33회 소환 → 소환권 30개 소모, 펫 33마리 → RevealPanel_33 오픈
///
/// [패널 구조]
///   GachaPanel
///     ├ Summon1Button         ← 1회 소환 버튼
///     │   └ Summon1BoxText    ← 소모 소환권 수 (TMP)
///     ├ Summon33Button        ← 33회 소환 버튼
///     │   └ Summon33BoxText   ← 소모 소환권 수 (TMP)
///     ├ RevealPanel_1         ← 1회 결과 패널  [CPetCardRevealPanel]
///     └ RevealPanel_33        ← 33회 결과 패널 [CPetCardRevealPanel]
/// </summary>
public class CPetGachaUI : MonoBehaviour
{
    #region Constants

    private const int Summon1Cost   = 1;
    private const int Summon33Cost  = 30;
    private const int Summon33Count = 33;

    #endregion

    #region Inspector

    [Header("소환 버튼")]
    [SerializeField] private Button          _summon1Button   = null;
    [SerializeField] private TextMeshProUGUI _summon1BoxText  = null;

    [SerializeField] private Button          _summon33Button  = null;
    [SerializeField] private TextMeshProUGUI _summon33BoxText = null;

    [Header("소환 시스템")]
    [SerializeField] private CGeneratePet    _generatePet     = null;

    [Header("패널 참조")]
    [SerializeField] private CPetCardRevealPanel _revealPanel1    = null;  // 1회 전용
    [SerializeField] private CPetCardRevealPanel _revealPanel33   = null;  // 33회 전용

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Inspector 미연결 시 자식에서 자동 탐색 (Pet_GachaPanel 자식 순서 기준)
        if (_revealPanel1 == null || _revealPanel33 == null)
        {
            CPetCardRevealPanel[] panels = GetComponentsInChildren<CPetCardRevealPanel>(true);
            if (panels.Length >= 1 && _revealPanel1  == null) _revealPanel1  = panels[0];
            if (panels.Length >= 2 && _revealPanel33 == null) _revealPanel33 = panels[1];
        }

        if (_revealPanel1  == null) Debug.LogError("[CPetGachaUI] RevealPanel_1 을 찾을 수 없습니다. Inspector 또는 자식 계층을 확인하세요.", this);
        if (_revealPanel33 == null) Debug.LogError("[CPetGachaUI] RevealPanel_33 을 찾을 수 없습니다. Inspector 또는 자식 계층을 확인하세요.", this);
        if (_generatePet   == null) Debug.LogError("[CPetGachaUI] CGeneratePet 이 연결되지 않았습니다.", this);
    }

    private void Start()
    {
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted += OnSaveDataLoaded;

        RefreshButtons();
    }

    private void OnEnable()
    {
        RefreshButtons();
    }

    private void OnDestroy()
    {
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted -= OnSaveDataLoaded;
    }

    #endregion

    #region Public Methods — 버튼 OnClick (Inspector에서 직접 연결)

    /// <summary>소환 1회 버튼 OnClick에 연결합니다.</summary>
    public void OnSummon1ButtonClick()
    {
        Debug.Log("[CPetGachaUI] 소환 1회 버튼 클릭");
        OnSummonClicked(1, Summon1Cost, _revealPanel1);
    }

    /// <summary>소환 33회 버튼 OnClick에 연결합니다.</summary>
    public void OnSummon33ButtonClick()
    {
        Debug.Log("[CPetGachaUI] 소환 33회 버튼 클릭");
        OnSummonClicked(Summon33Count, Summon33Cost, _revealPanel33);
    }

    #endregion

    #region Public Methods

    /// <summary>summonCount마리를 소환하고 결과 목록을 반환합니다. 소환권 boxCost개 차감.</summary>
    public List<CPetInstance> PerformSummon(int summonCount, int boxCost)
    {
        if (_generatePet == null)
        {
            CDebug.LogError("[CPetGachaUI] CGeneratePet 참조가 없습니다.");
            return new List<CPetInstance>();
        }

        List<CPetInstance> results = _generatePet.GeneratePets(summonCount, boxCost);
        RefreshButtons();
        return results;
    }

    /// <summary>카드 공개 패널 종료 후 가챠 패널로 복귀할 때 호출됩니다.</summary>
    public void ShowGachaPanel()
    {
        RefreshButtons();
    }

    #endregion

    #region Private Methods

    private void OnSaveDataLoaded(CSaveData data)
    {
        RefreshButtons();
    }

    private void OnSummonClicked(int summonCount, int boxCost, CPetCardRevealPanel targetPanel)
    {
        Debug.Log($"[CPetGachaUI] 소환 버튼 클릭 — summonCount:{summonCount}, boxCost:{boxCost}");

        if (CJsonManager.Instance == null)
        {
            Debug.LogError("[CPetGachaUI] CJsonManager.Instance가 null입니다.");
            return;
        }

        if (CJsonManager.Instance.CurrentSaveData == null)
        {
            Debug.LogError("[CPetGachaUI] CurrentSaveData가 null입니다.");
            return;
        }

        if (_generatePet == null)
        {
            Debug.LogError("[CPetGachaUI] _generatePet이 연결되지 않았습니다.", this);
            return;
        }

        if (targetPanel == null)
        {
            Debug.LogError($"[CPetGachaUI] targetPanel이 null입니다. (summonCount={summonCount})", this);
            return;
        }

        if (CPetInventorySystem.Instance == null)
        {
            Debug.LogError("[CPetGachaUI] CPetInventorySystem.Instance가 null입니다.");
            return;
        }

        int owned = CJsonManager.Instance.CurrentSaveData.petBoxCount;
        Debug.Log($"[CPetGachaUI] 소환권 보유: {owned}, 필요: {boxCost}");

        if (owned < boxCost)
        {
            Debug.LogWarning($"[CPetGachaUI] 소환권 부족 (보유:{owned} / 필요:{boxCost})");
            return;
        }

        List<CPetInstance> results = PerformSummon(summonCount, boxCost);
        Debug.Log($"[CPetGachaUI] 소환 결과 수: {results.Count}");

        if (results.Count == 0)
        {
            Debug.LogError("[CPetGachaUI] 소환 결과가 0개입니다. 인벤토리 가득 참 여부 또는 GeneratePets 로직을 확인하세요.");
            return;
        }

        Debug.Log($"[CPetGachaUI] Setup 호출 → {targetPanel.gameObject.name}");
        targetPanel.Setup(results, this, summonCount, boxCost);
    }

    private void RefreshButtons()
    {
        int owned = CJsonManager.Instance?.CurrentSaveData?.petBoxCount ?? 0;

        if (_summon1Button != null)
            _summon1Button.interactable = owned >= Summon1Cost;

        if (_summon1BoxText != null)
            _summon1BoxText.text = $"{Summon1Cost}";

        if (_summon33Button != null)
            _summon33Button.interactable = owned >= Summon33Cost;

        if (_summon33BoxText != null)
            _summon33BoxText.text = $"{Summon33Cost}";
    }

    #endregion
}
