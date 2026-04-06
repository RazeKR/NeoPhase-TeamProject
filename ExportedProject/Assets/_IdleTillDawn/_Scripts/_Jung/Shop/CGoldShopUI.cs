using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 내 카테고리 탭 전환과 재화(GoldShop) 패널의 6개 구매 버튼을 관리합니다.
///
/// [버튼 구성]
///   0 : 11000 다이아 지급  – 주간 5회 한정
///   1 : 33000 다이아 지급  – 주간 5회 한정
///   2 : 55000 다이아 지급  – 주간 5회 한정
///   3 : 110000 다이아 지급 – 주간 5회 한정
///   4 : 10000 골드 지급    – 일일 1회 무료
///   5 : 100000 골드 지급   – 다이아 1000 소모
///
/// [주간·일일 리셋 기준]
///   대한민국 표준시 (KST = UTC+9) 기반 월요일 자정 / 오늘 자정
///
/// [Inspector 연결]
///   _categoryButtons[0~3] : 재화·펫 패키지·무기 패키지·아이템 버튼 (순서대로)
///   _goldShopPanel         : GoldShopPanel 오브젝트
///   _shopItemButtons[0~5]  : 구매 버튼 6개 (위 순서와 동일)
///   _shopItemCountTexts[0~5] : 각 버튼의 구매 횟수 표시 Text
/// </summary>
public class CGoldShopUI : MonoBehaviour
{
    #region Constants

    private const int WeeklyLimit = 5;
    private const int DailyLimit  = 1;

    // 다이아 지급량 (주간 상품 0~3)
    private static readonly int[] DiamondRewards = { 11000, 33000, 55000, 110000 };

    // 다이아 구매 비용 (주간 상품은 0원 — 재미용 한화 표시만)
    private const int GoldWithDiamondCost   = 1000;    // 버튼5: 다이아 소모
    private const int GoldWithDiamondReward = 100000;  // 버튼5: 골드 지급량
    private const int DailyFreeGoldReward   = 10000;   // 버튼4: 무료 골드 지급량

    #endregion

    #region Inspector Variables

    [Header("카테고리 탭 버튼 (재화·펫·무기·아이템 순)")]
    [SerializeField] private Button[] _categoryButtons = new Button[4];

    [Header("카테고리 선택 색상")]
    [SerializeField] private Color _selectedColor  = new Color(0.9f, 0.7f, 0.2f); // 선택된 탭 색
    [SerializeField] private Color _normalColor    = Color.white;                  // 기본 탭 색

    [Header("GoldShopPanel")]
    [SerializeField] private GameObject _goldShopPanel = null;

    [Header("구매 버튼 6개 (0~3: 다이아 주간 / 4: 골드 일일 / 5: 골드 다이아 구매)")]
    [SerializeField] private Button[] _shopItemButtons = new Button[6];

    [Header("버튼별 구매 횟수 Text (0~3: 주간 X/5 / 4: 일일 X/1 / 5: 없어도 됨)")]
    [SerializeField] private Text[] _shopItemCountTexts = new Text[6];

    #endregion

    #region Private Variables

    private int _currentCategoryIndex = 0; // 현재 선택된 카테고리 (0 = 재화)

    #endregion

    #region Unity Methods

    private void Start()
    {
        RegisterCategoryButtons();
        RegisterShopItemButtons();

        // 상점 열릴 때 재화 탭이 기본 선택
        SelectCategory(0);
        RefreshAllButtonStates();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _categoryButtons.Length; i++)
        {
            if (_categoryButtons[i] == null) continue;
            int captured = i;
            _categoryButtons[i].onClick.RemoveAllListeners();
        }

        for (int i = 0; i < _shopItemButtons.Length; i++)
        {
            if (_shopItemButtons[i] != null)
                _shopItemButtons[i].onClick.RemoveAllListeners();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>상점이 열릴 때 외부(CShopUI 등)에서 호출하여 UI 상태를 갱신합니다.</summary>
    public void OnShopOpened()
    {
        SelectCategory(0);
        RefreshAllButtonStates();
    }

    #endregion

    #region Category

    private void RegisterCategoryButtons()
    {
        for (int i = 0; i < _categoryButtons.Length; i++)
        {
            if (_categoryButtons[i] == null) continue;
            int captured = i;
            _categoryButtons[i].onClick.AddListener(() => SelectCategory(captured));
        }
    }

    /// <summary>카테고리 탭을 전환하고 색을 업데이트합니다.</summary>
    private void SelectCategory(int index)
    {
        _currentCategoryIndex = index;

        // 탭 색 갱신
        for (int i = 0; i < _categoryButtons.Length; i++)
        {
            if (_categoryButtons[i] == null) continue;
            ColorBlock cb = _categoryButtons[i].colors;
            cb.normalColor      = (i == index) ? _selectedColor : _normalColor;
            cb.selectedColor    = cb.normalColor;
            cb.highlightedColor = cb.normalColor;
            _categoryButtons[i].colors = cb;
        }

        // 재화(0)만 구현 — 나머지 카테고리 패널은 추후 연결
        if (_goldShopPanel != null)
            _goldShopPanel.SetActive(index == 0);
    }

    #endregion

    #region Shop Item Buttons

    private void RegisterShopItemButtons()
    {
        for (int i = 0; i < _shopItemButtons.Length; i++)
        {
            if (_shopItemButtons[i] == null) continue;
            int captured = i;
            _shopItemButtons[i].onClick.AddListener(() => OnPurchaseButtonClicked(captured));
        }
    }

    private void OnPurchaseButtonClicked(int index)
    {
        switch (index)
        {
            case 0: TryBuyWeeklyDiamond(0); break;
            case 1: TryBuyWeeklyDiamond(1); break;
            case 2: TryBuyWeeklyDiamond(2); break;
            case 3: TryBuyWeeklyDiamond(3); break;
            case 4: TryBuyDailyFreeGold();  break;
            case 5: TryBuyGoldWithDiamond(); break;
        }
    }

    /// <summary>주간 다이아 상품 구매 (0~3번 버튼)</summary>
    private void TryBuyWeeklyDiamond(int itemIndex)
    {
        if (CGoldManager.Instance == null)
        {
            Debug.LogError("[CGoldShopUI] CGoldManager.Instance가 null입니다. 구매 취소.");
            return;
        }

        CSaveData data = GetSaveData();
        if (data == null) return;

        CheckAndResetWeekly(data);

        // 구매 횟수 리스트 크기 보정 (구버전 세이브 호환)
        while (data.shopWeeklyBuyCounts.Count < 4)
            data.shopWeeklyBuyCounts.Add(0);

        int count = data.shopWeeklyBuyCounts[itemIndex];
        if (count >= WeeklyLimit)
        {
            Debug.Log($"[CGoldShopUI] 주간 구매 한도 초과 (상품 {itemIndex}, {count}/{WeeklyLimit})");
            return;
        }

        // 재화 지급 먼저 → 성공 확인 후 횟수 증가 저장 (예외 시 카운트 오염 방지)
        CGoldManager.Instance.AddDiamond(DiamondRewards[itemIndex]);
        data.shopWeeklyBuyCounts[itemIndex]++;
        SaveData(data);
        RefreshAllButtonStates();

        Debug.Log($"[CGoldShopUI] 다이아 {DiamondRewards[itemIndex]:N0} 지급 " +
                  $"(주간 {data.shopWeeklyBuyCounts[itemIndex]}/{WeeklyLimit})");
    }

    /// <summary>일일 무료 골드 구매 (4번 버튼)</summary>
    private void TryBuyDailyFreeGold()
    {
        if (CGoldManager.Instance == null)
        {
            Debug.LogError("[CGoldShopUI] CGoldManager.Instance가 null입니다. 구매 취소.");
            return;
        }

        CSaveData data = GetSaveData();
        if (data == null) return;

        CheckAndResetDaily(data);

        if (data.shopDailyBuyCount >= DailyLimit)
        {
            Debug.Log($"[CGoldShopUI] 일일 무료 골드 이미 수령 ({data.shopDailyBuyCount}/{DailyLimit})");
            return;
        }

        // 재화 지급 먼저 → 성공 확인 후 횟수 증가 저장
        CGoldManager.Instance.AddGold(DailyFreeGoldReward);
        data.shopDailyBuyCount++;
        SaveData(data);
        RefreshAllButtonStates();

        Debug.Log($"[CGoldShopUI] 골드 {DailyFreeGoldReward:N0} 지급 " +
                  $"(일일 {data.shopDailyBuyCount}/{DailyLimit})");
    }

    /// <summary>다이아 소모 골드 구매 (5번 버튼)</summary>
    private void TryBuyGoldWithDiamond()
    {
        if (CGoldManager.Instance == null)
        {
            Debug.LogError("[CGoldShopUI] CGoldManager.Instance가 null입니다. 구매 취소.");
            return;
        }

        if (CGoldManager.Instance.Diamond < GoldWithDiamondCost)
        {
            Debug.Log($"[CGoldShopUI] 다이아 부족 " +
                      $"(보유: {CGoldManager.Instance.Diamond}, 필요: {GoldWithDiamondCost})");
            return;
        }

        bool success = CGoldManager.Instance.BuyGoldWithDiamond(GoldWithDiamondCost, GoldWithDiamondReward);
        if (!success) return;

        RefreshAllButtonStates();
        Debug.Log($"[CGoldShopUI] 다이아 {GoldWithDiamondCost} 소모 → 골드 {GoldWithDiamondReward:N0} 지급");
    }

    #endregion

    #region Button State Refresh

    /// <summary>모든 버튼의 interactable 상태와 카운트 텍스트를 갱신합니다.</summary>
    private void RefreshAllButtonStates()
    {
        CSaveData data = GetSaveData();
        if (data == null) return;

        CheckAndResetWeekly(data);
        CheckAndResetDaily(data);

        while (data.shopWeeklyBuyCounts.Count < 4)
            data.shopWeeklyBuyCounts.Add(0);

        // 주간 다이아 버튼 0~3
        for (int i = 0; i < 4; i++)
        {
            int count = data.shopWeeklyBuyCounts[i];
            bool canBuy = count < WeeklyLimit;

            SetButtonState(i, canBuy);
            SetCountText(i, $"주간 {count}/{WeeklyLimit}", !canBuy);
        }

        // 일일 무료 골드 버튼 4
        {
            int count = data.shopDailyBuyCount;
            bool canBuy = count < DailyLimit;

            SetButtonState(4, canBuy);
            SetCountText(4, canBuy ? $"일일 구매 {count}/{DailyLimit}" : "오늘 수령 완료", !canBuy);
        }

        // 다이아 소모 골드 버튼 5 (한도 없음 — 다이아 잔액만 확인)
        {
            bool hasDiamond = CGoldManager.Instance != null &&
                              CGoldManager.Instance.Diamond >= GoldWithDiamondCost;
            SetButtonState(5, hasDiamond);
            SetCountText(5, $"◆ {GoldWithDiamondCost:N0}");
        }
    }

    private void SetButtonState(int index, bool interactable)
    {
        if (index >= _shopItemButtons.Length || _shopItemButtons[index] == null) return;

        _shopItemButtons[index].interactable = interactable;

        // Unity Color Tint가 Disabled 상태에서 배경 Image를 반투명하게 만드는 것을 방지
        // Disabled Color를 흰색 불투명으로 고정 → 검정 배경 × 흰색 = 검정 그대로 유지
        ColorBlock cb = _shopItemButtons[index].colors;
        cb.disabledColor = Color.white;
        _shopItemButtons[index].colors = cb;
    }

    private void SetCountText(int index, string text, bool limitReached = false)
    {
        if (index >= _shopItemCountTexts.Length || _shopItemCountTexts[index] == null) return;
        _shopItemCountTexts[index].text  = text;
        _shopItemCountTexts[index].color = limitReached ? Color.red : Color.yellow;
    }

    #endregion

    #region KST Time & Reset Logic

    /// <summary>UTC+9 현재 시각 (KST, DST 없음)</summary>
    private static DateTime GetKSTNow() => DateTime.UtcNow.AddHours(9);

    /// <summary>KST 기준 이번 주 월요일 날짜 문자열 (yyyy-MM-dd)</summary>
    private static string GetKSTMondayString()
    {
        DateTime kst = GetKSTNow();
        int daysSinceMonday = ((int)kst.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return kst.AddDays(-daysSinceMonday).ToString("yyyy-MM-dd");
    }

    /// <summary>KST 기준 오늘 날짜 문자열 (yyyy-MM-dd)</summary>
    private static string GetKSTTodayString() => GetKSTNow().ToString("yyyy-MM-dd");

    /// <summary>주간 리셋이 필요하면 구매 횟수를 초기화하고 저장합니다.</summary>
    private static void CheckAndResetWeekly(CSaveData data)
    {
        string monday = GetKSTMondayString();
        if (data.shopWeeklyResetDate == monday) return;

        data.shopWeeklyResetDate = monday;
        data.shopWeeklyBuyCounts = new System.Collections.Generic.List<int> { 0, 0, 0, 0 };
        SaveData(data);
        Debug.Log($"[CGoldShopUI] 주간 구매 횟수 초기화 (기준: {monday})");
    }

    /// <summary>일일 리셋이 필요하면 구매 횟수를 초기화하고 저장합니다.</summary>
    private static void CheckAndResetDaily(CSaveData data)
    {
        string today = GetKSTTodayString();
        if (data.shopDailyResetDate == today) return;

        data.shopDailyResetDate = today;
        data.shopDailyBuyCount  = 0;
        SaveData(data);
        Debug.Log($"[CGoldShopUI] 일일 구매 횟수 초기화 (기준: {today})");
    }

    #endregion

    #region Save Helpers

    private static CSaveData GetSaveData()
    {
        if (CJsonManager.Instance == null)
        {
            Debug.LogError("[CGoldShopUI] CJsonManager.Instance가 null입니다.");
            return null;
        }
        return CJsonManager.Instance.GetOrCreateSaveData();
    }

    private static void SaveData(CSaveData data)
    {
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.Save(data);
    }

    #endregion
}
