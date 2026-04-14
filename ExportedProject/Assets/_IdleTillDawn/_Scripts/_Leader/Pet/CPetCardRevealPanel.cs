using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 소환 결과 카드들을 표시하는 패널입니다.
/// 1회 소환으로 열리면 재소환 버튼이 "소환 1회", 33회 소환으로 열리면 "소환 33회"로 표시됩니다.
///
/// [동작]
///   - 카드 클릭 → 해당 카드 1장 공개
///   - 배경(카드 외 영역) 클릭 → 미공개 카드 전체 공개
///   - 공개된 카드 클릭 → CPetCardInfoPanel 표시
///   - 확인 버튼 → 가챠 패널로 복귀
///   - 소환 버튼 → 동일 조건으로 재소환 (소환권 충분할 때만 활성화)
///
/// [패널 구조]
///   PetCard1_RevealPanel or PetCard33_RevealPanel   ← 이 스크립트가 부착된 오브젝트 (초기 비활성화)
///     ├ BackgroundBlocker  ← 카드 외 영역 클릭용 버튼 (최하단 레이어)
///     ├ CardContainer      ← 카드 배치 부모
///     ├ ConfirmButton      ← 확인 버튼
///     ├ SummonButton       ← 재소환 버튼
///     │   └ SummonButtonText (TMP)  ← "소환 1회 🎫 N" or "소환 33회 🎫 N"
///     └ CPetCardInfoPanel  ← 카드 정보 팝업 (자식으로 배치)
/// </summary>
public class CPetCardRevealPanel : MonoBehaviour
{
    #region Inspector

    [SerializeField] private Transform       _cardContainer     = null;
    [SerializeField] private GameObject      _cardItemPrefab    = null;

    [SerializeField] private Button          _backgroundBlocker = null;
    [SerializeField] private Button          _confirmButton     = null;
    [SerializeField] private Button          _summonButton      = null;
    [SerializeField] private TextMeshProUGUI _summonButtonText  = null;

    [Header("터치 힌트 (뒷면 상태일 때만 표시)")]
    [SerializeField] private GameObject      _touchHintObject   = null;  // "터치하여 결과 모두 확인" 오브젝트

    #endregion

    #region Private

    private readonly List<CPetCardItem> _cards = new List<CPetCardItem>();
    private CPetGachaUI _gachaUI;

    // 현재 패널을 연 소환 종류 기억
    private int _currentSummonCount;  // 소환 결과 수
    private int _currentBoxCost;      // 소환권 소모량

    #endregion

    #region Unity Methods

    private void Awake()
    {
        _confirmButton?.onClick.AddListener(OnConfirm);
        _summonButton?.onClick.AddListener(OnResummon);
        _backgroundBlocker?.onClick.AddListener(RevealAll);
    }

    #endregion

    #region Public Methods

    /// <summary>소환 결과를 받아 카드 공개 패널을 엽니다.</summary>
    /// <param name="instances">소환된 펫 인스턴스 목록</param>
    /// <param name="gachaUI">가챠 UI 참조</param>
    /// <param name="summonCount">이번 소환에서 뽑은 수 (1 or 33)</param>
    /// <param name="boxCost">이번 소환에서 소모한 소환권 수 (1 or 30)</param>
    public void Setup(List<CPetInstance> instances, CPetGachaUI gachaUI, int summonCount, int boxCost)
    {
        Debug.Log($"[CPetCardRevealPanel] Setup 진입 | panel={gameObject.name} | instances={instances.Count}");

        _gachaUI            = gachaUI;
        _currentSummonCount = summonCount;
        _currentBoxCost     = boxCost;

        // 이전 카드 제거
        foreach (CPetCardItem card in _cards)
            if (card != null) Destroy(card.gameObject);
        _cards.Clear();

        // 카드 생성
        if (_cardItemPrefab == null) Debug.LogError("[CPetCardRevealPanel] _cardItemPrefab 이 null입니다.", this);
        if (_cardContainer  == null) Debug.LogError("[CPetCardRevealPanel] _cardContainer 가 null입니다.", this);

        foreach (CPetInstance instance in instances)
        {
            if (_cardItemPrefab == null || _cardContainer == null) break;

            GameObject go    = Instantiate(_cardItemPrefab, _cardContainer);
            CPetCardItem item = go.GetComponent<CPetCardItem>();
            if (item == null) { Destroy(go); continue; }

            item.Setup(instance);
            item.OnRevealedCallback = OnCardRevealed;
            _cards.Add(item);
        }

        SetBottomButtonsVisible(false);
        _touchHintObject?.SetActive(true);

        gameObject.SetActive(true);
        Debug.Log($"[CPetCardRevealPanel] SetActive(true) 완료 | activeInHierarchy={gameObject.activeInHierarchy}");
    }

    /// <summary>미공개 카드를 모두 공개합니다. 배경 클릭 시 호출됩니다.</summary>
    public void RevealAll()
    {
        foreach (CPetCardItem card in _cards)
            card.Reveal();

        OnAllRevealed();
    }

    #endregion

    #region Private Methods

    private void OnConfirm()
    {
        gameObject.SetActive(false);
        CPetCardInfoPanel.Instance?.Close();
        _gachaUI?.ShowGachaPanel();
    }

    private void OnResummon()
    {
        if (_gachaUI == null) return;

        List<CPetInstance> newPets = _gachaUI.PerformSummon(_currentSummonCount, _currentBoxCost);
        if (newPets == null || newPets.Count == 0) return;

        Setup(newPets, _gachaUI, _currentSummonCount, _currentBoxCost);
    }

    private void OnCardRevealed()
    {
        bool allRevealed = _cards.TrueForAll(c => c.IsRevealed);
        if (allRevealed)
            OnAllRevealed();
    }

    private void OnAllRevealed()
    {
        _touchHintObject?.SetActive(false);
        RefreshSummonButton();
        SetBottomButtonsVisible(true);
    }

    private void SetBottomButtonsVisible(bool visible)
    {
        if (_confirmButton != null)
            _confirmButton.gameObject.SetActive(visible);

        if (_summonButton != null)
            _summonButton.gameObject.SetActive(visible);
    }

    private void RefreshSummonButton()
    {
        int owned = CJsonManager.Instance?.CurrentSaveData?.petBoxCount ?? 0;

        if (_summonButton != null)
            _summonButton.interactable = owned >= _currentBoxCost;

        if (_summonButtonText != null)
        {
            string label = _currentSummonCount == 1 ? "소환 1회" : $"소환 {_currentSummonCount}회";
            _summonButtonText.text = $"{label}  {_currentBoxCost}";
        }
    }

    #endregion
}
