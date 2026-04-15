using System.Collections;
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

    [Header("확률 정보 패널")]
    [SerializeField] private Button          _probInfoButton      = null;  // 확률 정보 버튼
    [SerializeField] private Button          _probInfoCloseButton = null;  // 확률 정보 패널 닫기 버튼
    [SerializeField] private GameObject      _probInfoPanel       = null;  // 확률 정보 패널
    [SerializeField] private TextMeshProUGUI _commonRateText      = null;  // 일반 확률 텍스트
    [SerializeField] private TextMeshProUGUI _rareRateText        = null;  // 레어 확률 텍스트
    [SerializeField] private TextMeshProUGUI _epicRateText        = null;  // 에픽 확률 텍스트
    [SerializeField] private TextMeshProUGUI _legendaryRateText   = null;  // 레전드 확률 텍스트

    [Header("핑퐁 이미지 (위아래 왕복)")]
    [SerializeField] private RectTransform[] _pingPongImages      = null;  // 5장
    [SerializeField] private float           _pingPongAmplitude   = 20f;   // 왕복 거리 (px)
    [SerializeField] private float           _pingPongSpeed       = 0.5f;  // 속도 (낮을수록 느림)

    #endregion

    #region Fields

    private Vector2[] _pingPongOrigins = null;
    private Coroutine _pingPongCoroutine = null;

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

        // 확률 정보 패널은 시작 시 닫아 둠
        if (_probInfoPanel != null)
            _probInfoPanel.SetActive(false);
    }

    private void OnEnable()
    {
        // 저장 데이터 로드 완료 시 버튼 갱신
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted += OnSaveDataLoaded;

        // 소환권 구매 시 버튼 즉시 갱신 — 이 구독이 없으면 패널이 열린 채로 구매해도 버튼이 활성화되지 않음
        CGoldShopUI.OnPetBoxCountChanged += OnPetBoxCountChanged;

        RefreshButtons();
        StartPingPong();

        if (_probInfoButton != null)
            _probInfoButton.onClick.AddListener(OnProbInfoButtonClick);

        if (_probInfoCloseButton != null)
            _probInfoCloseButton.onClick.AddListener(CloseProbInfoPanel);
    }

    private void OnDisable()
    {
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted -= OnSaveDataLoaded;

        CGoldShopUI.OnPetBoxCountChanged -= OnPetBoxCountChanged;

        StopPingPong();

        if (_probInfoButton != null)
            _probInfoButton.onClick.RemoveListener(OnProbInfoButtonClick);

        if (_probInfoCloseButton != null)
            _probInfoCloseButton.onClick.RemoveListener(CloseProbInfoPanel);

        // 패널 열린 채로 탭을 벗어날 때 닫아 둠
        if (_probInfoPanel != null)
            _probInfoPanel.SetActive(false);
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

    private void OnProbInfoButtonClick()
    {
        if (_probInfoPanel == null) return;

        bool isOpen = !_probInfoPanel.activeSelf;
        _probInfoPanel.SetActive(isOpen);

        if (isOpen)
            RefreshProbTexts();
    }

    private void CloseProbInfoPanel()
    {
        if (_probInfoPanel != null)
            _probInfoPanel.SetActive(false);
    }

    private void RefreshProbTexts()
    {
        if (_generatePet == null) return;

        int total = _generatePet.CommonRate
                  + _generatePet.RareRate
                  + _generatePet.EpicRate
                  + _generatePet.LegendaryRate;

        if (total <= 0) return;

        SetProbText(_commonRateText,    "일반",   _generatePet.CommonRate,    total, new Color32(200, 200, 200, 255)); // 회색
        SetProbText(_rareRateText,      "레어",   _generatePet.RareRate,      total, new Color32( 80, 160, 255, 255)); // 파랑
        SetProbText(_epicRateText,      "에픽",   _generatePet.EpicRate,      total, new Color32(180,  80, 255, 255)); // 보라
        SetProbText(_legendaryRateText, "레전드", _generatePet.LegendaryRate, total, new Color32(255, 185,   0, 255)); // 골드
    }

    private void SetProbText(TextMeshProUGUI label, string gradeName, int rate, int total, Color32 color)
    {
        if (label == null) return;

        float percent = rate * 100f / total;
        label.text  = $"{gradeName} ({percent:F1}%)";
        label.color = color;
    }

    private void OnSaveDataLoaded(CSaveData data) => RefreshButtons();

    private void OnPetBoxCountChanged(int count) => RefreshButtons();

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

    private void StartPingPong()
    {
        if (_pingPongImages == null || _pingPongImages.Length == 0) return;

        // 각 이미지의 초기 위치 저장
        _pingPongOrigins = new Vector2[_pingPongImages.Length];
        for (int i = 0; i < _pingPongImages.Length; i++)
        {
            if (_pingPongImages[i] != null)
                _pingPongOrigins[i] = _pingPongImages[i].anchoredPosition;
        }

        _pingPongCoroutine = StartCoroutine(PingPongRoutine());
    }

    private void StopPingPong()
    {
        if (_pingPongCoroutine != null)
        {
            StopCoroutine(_pingPongCoroutine);
            _pingPongCoroutine = null;
        }

        // 원래 위치로 복원
        if (_pingPongOrigins != null)
        {
            for (int i = 0; i < _pingPongImages.Length; i++)
            {
                if (_pingPongImages[i] != null)
                    _pingPongImages[i].anchoredPosition = _pingPongOrigins[i];
            }
        }
    }

    /// <summary>
    /// 이미지들을 위아래로 느리게 왕복시킵니다.
    /// 각 이미지는 오프셋을 두어 물결 효과가 납니다.
    /// </summary>
    private IEnumerator PingPongRoutine()
    {
        float time = 0f;
        int count = _pingPongImages.Length;

        while (true)
        {
            time += Time.deltaTime * _pingPongSpeed;

            for (int i = 0; i < count; i++)
            {
                if (_pingPongImages[i] == null) continue;

                // 이미지마다 위상 오프셋을 주어 순차적으로 물결치는 느낌
                float offset = (float)i / count * Mathf.PI * 2f;
                float yDelta = Mathf.Sin(time * Mathf.PI * 2f + offset) * _pingPongAmplitude;

                _pingPongImages[i].anchoredPosition = _pingPongOrigins[i] + new Vector2(0f, yDelta);
            }

            yield return null;
        }
    }

    private void RefreshButtons()
    {
        int owned = CJsonManager.Instance?.CurrentSaveData?.petBoxCount ?? 0;
        Debug.Log($"[CPetGachaUI] RefreshButtons — 소환권:{owned} | " +
                  $"CJsonManager:{CJsonManager.Instance != null} | " +
                  $"CurrentSaveData:{CJsonManager.Instance?.CurrentSaveData != null}");

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
