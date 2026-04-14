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

    // 무기 상자 골드 상품 (0~2번 버튼): 수량, 골드 비용
    private static readonly int[] WeaponBoxGoldAmounts = { 10, 25, 50 };
    private static readonly int[] WeaponBoxGoldCosts   = { 100000, 250000, 500000 };

    // 무기 상자 다이아 상품 (3~5번 버튼): 수량, 다이아 비용
    private static readonly int[] WeaponBoxDiamondAmounts = { 10, 25, 50 };
    private static readonly int[] WeaponBoxDiamondCosts   = { 1000, 2500, 5000 };

    // 아이템 상점 상품 (0~3번 버튼)
    private static readonly int   HpPotionId         = 1;
    private static readonly int   ManaPotionId        = 8;
    private static readonly int   WeaponScrollId      = 2;
    private static readonly int   HpPotionCost        = 100000;
    private static readonly int   ManaPotionCost      = 100000;
    private static readonly int   WeaponScrollCost    = 1000000;
    private static readonly int   InventoryExpandCost = 1000000;
    private static readonly int   ItemShopAmount      = 10;

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

    [Header("WeaponShopPanel (무기 패키지 탭)")]
    [SerializeField] private GameObject _weaponShopPanel = null;

    [Header("무기 상자 구매 버튼 6개 (0~2: 골드 / 3~5: 다이아)")]
    [SerializeField] private Button[] _weaponShopButtons = new Button[6];

    [Header("무기 상자 버튼별 비용 Text")]
    [SerializeField] private Text[] _weaponShopCostTexts = new Text[6];

    [Header("ItemShopPanel (아이템 탭)")]
    [SerializeField] private GameObject _itemShopPanel = null;

    [Header("아이템 구매 버튼 4개 (0: HP포션 / 1: 마나포션 / 2: 무기주문서 / 3: 인벤토리 확장)")]
    [SerializeField] private Button[] _itemShopButtons = new Button[4];

    #endregion

    #region Private Variables

    private int _currentCategoryIndex = 0; // 현재 선택된 카테고리 (0 = 재화, 2 = 무기 패키지)

    // CGoldManager 이벤트 구독 해제용 델리게이트 캐시
    private Action<int> _onGoldChanged;
    private Action<int> _onDiamondChanged;

    #endregion

    #region Unity Methods

    private void Start()
    {
        RegisterCategoryButtons();
        RegisterShopItemButtons();
        RegisterWeaponShopButtons();

        // CGoldManager 재화 변경 이벤트 구독 — 재화가 바뀌면 무기/아이템 상점 버튼 상태를 즉시 갱신
        _onGoldChanged    = _ => { RefreshWeaponShopButtonStates(); RefreshItemShopButtonStates(); };
        _onDiamondChanged = _ => RefreshWeaponShopButtonStates();
        if (CGoldManager.Instance != null)
        {
            CGoldManager.Instance.OnGoldChanged    += _onGoldChanged;
            CGoldManager.Instance.OnDiamondChanged += _onDiamondChanged;
        }

        // 상점 열릴 때 재화 탭이 기본 선택
        RegisterItemShopButtons();

        SelectCategory(0);
        RefreshAllButtonStates();
        // 무기/아이템 상점 버튼은 CGoldManager.Start()가 끝난 뒤 첫 재화 이벤트로 갱신되므로
        // 여기서는 패널이 비활성이어도 상관없지만, 이미 로드된 경우를 위해 한 번 호출
        RefreshWeaponShopButtonStates();
        RefreshItemShopButtonStates();
    }

    private void OnDestroy()
    {
        if (CGoldManager.Instance != null)
        {
            if (_onGoldChanged    != null) CGoldManager.Instance.OnGoldChanged    -= _onGoldChanged;
            if (_onDiamondChanged != null) CGoldManager.Instance.OnDiamondChanged -= _onDiamondChanged;
        }

        for (int i = 0; i < _categoryButtons.Length; i++)
        {
            if (_categoryButtons[i] == null) continue;
            _categoryButtons[i].onClick.RemoveAllListeners();
        }

        for (int i = 0; i < _shopItemButtons.Length; i++)
        {
            if (_shopItemButtons[i] != null)
                _shopItemButtons[i].onClick.RemoveAllListeners();
        }

        for (int i = 0; i < _weaponShopButtons.Length; i++)
        {
            if (_weaponShopButtons[i] != null)
                _weaponShopButtons[i].onClick.RemoveAllListeners();
        }

        for (int i = 0; i < _itemShopButtons.Length; i++)
        {
            if (_itemShopButtons[i] != null)
                _itemShopButtons[i].onClick.RemoveAllListeners();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>상점이 열릴 때 외부(CShopUI 등)에서 호출하여 UI 상태를 갱신합니다.</summary>
    public void OnShopOpened()
    {
        SelectCategory(0);
        RefreshAllButtonStates();
        RefreshWeaponShopButtonStates();
        RefreshItemShopButtonStates();
    }

    /// <summary>무기 상자 보유 수량 변경 시 발행됩니다. 인자: 변경 후 보유 수량</summary>
    public static event Action<int> OnWeaponBoxCountChanged;

    /// <summary>
    /// 외부에서 무기 상자 개수 변경 이벤트를 발생시키기 위한 래퍼(Wrapper) 메서드입니다.
    /// </summary>
    public static void TriggerWeaponBoxCountChanged(int count)
    {
        OnWeaponBoxCountChanged?.Invoke(count);
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

        if (_goldShopPanel != null)
            _goldShopPanel.SetActive(index == 0);

        if (_weaponShopPanel != null)
        {
            _weaponShopPanel.SetActive(index == 2);
            if (index == 2)
                RefreshWeaponShopButtonStates();
        }

        if (_itemShopPanel != null)
        {
            _itemShopPanel.SetActive(index == 3);
            if (index == 3)
                RefreshItemShopButtonStates();
        }
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

    private void RegisterWeaponShopButtons()
    {
        for (int i = 0; i < _weaponShopButtons.Length; i++)
        {
            if (_weaponShopButtons[i] == null) continue;
            int captured = i;
            _weaponShopButtons[i].onClick.AddListener(() => OnWeaponShopButtonClicked(captured));
        }
    }

    private void OnWeaponShopButtonClicked(int index)
    {
        if (index < 3) TryBuyWeaponBoxWithGold(index);
        else           TryBuyWeaponBoxWithDiamond(index - 3);
    }

    /// <summary>골드로 무기 상자 구매 (0~2번 버튼)</summary>
    private void TryBuyWeaponBoxWithGold(int itemIndex)
    {
        if (CGoldManager.Instance == null)
        {
            CDebug.LogError("[CGoldShopUI] CGoldManager.Instance가 null입니다. 구매 취소.");
            return;
        }

        int cost = WeaponBoxGoldCosts[itemIndex];
        if (CGoldManager.Instance.Gold < cost)
        {
            CDebug.Log($"[CGoldShopUI] 골드 부족 (보유: {CGoldManager.Instance.Gold:N0}, 필요: {cost:N0})");
            return;
        }

        bool success = CGoldManager.Instance.SpendGold(cost);
        if (!success) return;

        CSaveData data = GetSaveData();
        if (data == null) return;

        int amount = WeaponBoxGoldAmounts[itemIndex];
        data.weaponBoxCount += amount;
        SaveData(data);

        OnWeaponBoxCountChanged?.Invoke(data.weaponBoxCount);
        RefreshWeaponShopButtonStates();
        CDebug.Log($"[CGoldShopUI] 골드 {cost:N0} 소모 → 무기 상자 {amount}개 지급 (총 {data.weaponBoxCount}개)");
    }

    /// <summary>다이아로 무기 상자 구매 (3~5번 버튼)</summary>
    private void TryBuyWeaponBoxWithDiamond(int itemIndex)
    {
        if (CGoldManager.Instance == null)
        {
            CDebug.LogError("[CGoldShopUI] CGoldManager.Instance가 null입니다. 구매 취소.");
            return;
        }

        int cost = WeaponBoxDiamondCosts[itemIndex];
        if (CGoldManager.Instance.Diamond < cost)
        {
            CDebug.Log($"[CGoldShopUI] 다이아 부족 (보유: {CGoldManager.Instance.Diamond:N0}, 필요: {cost:N0})");
            return;
        }

        bool success = CGoldManager.Instance.SpendDiamond(cost);
        if (!success) return;

        CSaveData data = GetSaveData();
        if (data == null) return;

        int amount = WeaponBoxDiamondAmounts[itemIndex];
        data.weaponBoxCount += amount;
        SaveData(data);

        OnWeaponBoxCountChanged?.Invoke(data.weaponBoxCount);
        RefreshWeaponShopButtonStates();
        CDebug.Log($"[CGoldShopUI] 다이아 {cost:N0} 소모 → 무기 상자 {amount}개 지급 (총 {data.weaponBoxCount}개)");
    }

    private void RegisterItemShopButtons()
    {
        for (int i = 0; i < _itemShopButtons.Length; i++)
        {
            if (_itemShopButtons[i] == null) continue;
            int captured = i;
            _itemShopButtons[i].onClick.AddListener(() => OnItemShopButtonClicked(captured));
        }
    }

    private void OnItemShopButtonClicked(int index)
    {
        switch (index)
        {
            case 0: TryBuyItemWithGold(HpPotionId,    HpPotionCost,        ItemShopAmount, "HP 포션");        break;
            case 1: TryBuyItemWithGold(ManaPotionId,   ManaPotionCost,      ItemShopAmount, "마나 포션");      break;
            case 2: TryBuyItemWithGold(WeaponScrollId, WeaponScrollCost,    ItemShopAmount, "무기 주문서");    break;
            case 3: TryBuyInventoryExpand();                                                                   break;
        }
    }

    /// <summary>골드로 아이템을 구매해 인벤토리에 지급합니다. 포션·스크롤은 기존 슬롯에 스택되므로 새 슬롯이 필요할 때만 인벤토리 여유 공간을 확인합니다.</summary>
    private void TryBuyItemWithGold(int itemId, int cost, int amount, string itemName)
    {
        if (CGoldManager.Instance == null)
        {
            CDebug.LogError("[CGoldShopUI] CGoldManager.Instance가 null입니다. 구매 취소.");
            return;
        }

        if (CGoldManager.Instance.Gold < cost)
        {
            CDebug.Log($"[CGoldShopUI] 골드 부족 (보유: {CGoldManager.Instance.Gold:N0}, 필요: {cost:N0})");
            return;
        }

        // 포션·스크롤은 이미 슬롯이 있으면 스택 가능 — 새 슬롯이 필요한 경우만 공간 체크
        bool existsInInventory = CInventorySystemJ.Instance._inventory
            .Find(i => i._itemData != null && i._itemData.Id == itemId) != null;

        if (!existsInInventory && CInventorySystemJ.Instance.IsFull)
        {
            CDebug.Log($"[CGoldShopUI] 인벤토리가 가득 차 {itemName}을(를) 지급할 수 없습니다.");
            CInventorySystemJ.Instance.NotifyInventoryFull();
            return;
        }

        bool success = CGoldManager.Instance.SpendGold(cost);
        if (!success) return;

        CInventorySystemJ.Instance.AddItem(itemId, amount);
        RefreshItemShopButtonStates();
        CDebug.Log($"[CGoldShopUI] 골드 {cost:N0} 소모 → {itemName} {amount}개 지급");
    }

    /// <summary>골드로 인벤토리 25칸을 확장합니다.</summary>
    private void TryBuyInventoryExpand()
    {
        if (CGoldManager.Instance == null)
        {
            CDebug.LogError("[CGoldShopUI] CGoldManager.Instance가 null입니다. 구매 취소.");
            return;
        }

        if (CGoldManager.Instance.Gold < InventoryExpandCost)
        {
            CDebug.Log($"[CGoldShopUI] 골드 부족 (보유: {CGoldManager.Instance.Gold:N0}, 필요: {InventoryExpandCost:N0})");
            return;
        }

        bool success = CGoldManager.Instance.SpendGold(InventoryExpandCost);
        if (!success) return;

        CInventorySystemJ.Instance.ExpandCapacity();
        RefreshItemShopButtonStates();
        CDebug.Log($"[CGoldShopUI] 골드 {InventoryExpandCost:N0} 소모 → 인벤토리 25칸 확장 (현재 {CInventorySystemJ.Instance.MaxCapacity}칸)");
    }

    /// <summary>주간 다이아 상품 구매 (0~3번 버튼)</summary>
    private void TryBuyWeeklyDiamond(int itemIndex)
    {
        if (CGoldManager.Instance == null)
        {
            CDebug.LogError("[CGoldShopUI] CGoldManager.Instance가 null입니다. 구매 취소.");
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
            CDebug.Log($"[CGoldShopUI] 주간 구매 한도 초과 (상품 {itemIndex}, {count}/{WeeklyLimit})");
            return;
        }

        // 재화 지급 먼저 → 성공 확인 후 횟수 증가 저장 (예외 시 카운트 오염 방지)
        CGoldManager.Instance.AddDiamond(DiamondRewards[itemIndex]);
        data.shopWeeklyBuyCounts[itemIndex]++;
        SaveData(data);
        RefreshAllButtonStates();

        CDebug.Log($"[CGoldShopUI] 다이아 {DiamondRewards[itemIndex]:N0} 지급 " +
                  $"(주간 {data.shopWeeklyBuyCounts[itemIndex]}/{WeeklyLimit})");
    }

    /// <summary>일일 무료 골드 구매 (4번 버튼)</summary>
    private void TryBuyDailyFreeGold()
    {
        if (CGoldManager.Instance == null)
        {
            CDebug.LogError("[CGoldShopUI] CGoldManager.Instance가 null입니다. 구매 취소.");
            return;
        }

        CSaveData data = GetSaveData();
        if (data == null) return;

        CheckAndResetDaily(data);

        if (data.shopDailyBuyCount >= DailyLimit)
        {
            CDebug.Log($"[CGoldShopUI] 일일 무료 골드 이미 수령 ({data.shopDailyBuyCount}/{DailyLimit})");
            return;
        }

        // 재화 지급 먼저 → 성공 확인 후 횟수 증가 저장
        CGoldManager.Instance.AddGold(DailyFreeGoldReward);
        data.shopDailyBuyCount++;
        SaveData(data);
        RefreshAllButtonStates();

        CDebug.Log($"[CGoldShopUI] 골드 {DailyFreeGoldReward:N0} 지급 " +
                  $"(일일 {data.shopDailyBuyCount}/{DailyLimit})");
    }

    /// <summary>다이아 소모 골드 구매 (5번 버튼)</summary>
    private void TryBuyGoldWithDiamond()
    {
        if (CGoldManager.Instance == null)
        {
            CDebug.LogError("[CGoldShopUI] CGoldManager.Instance가 null입니다. 구매 취소.");
            return;
        }

        if (CGoldManager.Instance.Diamond < GoldWithDiamondCost)
        {
            CDebug.Log($"[CGoldShopUI] 다이아 부족 " +
                      $"(보유: {CGoldManager.Instance.Diamond}, 필요: {GoldWithDiamondCost})");
            return;
        }

        bool success = CGoldManager.Instance.BuyGoldWithDiamond(GoldWithDiamondCost, GoldWithDiamondReward);
        if (!success) return;

        RefreshAllButtonStates();
        CDebug.Log($"[CGoldShopUI] 다이아 {GoldWithDiamondCost} 소모 → 골드 {GoldWithDiamondReward:N0} 지급");
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

    /// <summary>무기 상점 버튼 6개의 interactable 상태와 비용 텍스트 색깔을 갱신합니다.</summary>
    private void RefreshWeaponShopButtonStates()
    {
        if (CGoldManager.Instance == null) return;

        // 골드 버튼 0~2: 텍스트 내용은 유지, 재화 부족 시 색깔만 빨간색으로
        for (int i = 0; i < WeaponBoxGoldCosts.Length; i++)
        {
            bool canBuy = CGoldManager.Instance.Gold >= WeaponBoxGoldCosts[i];
            SetWeaponShopButtonState(i, canBuy);
            SetWeaponShopCostTextColor(i, !canBuy);
        }

        // 다이아 버튼 3~5: 텍스트 내용은 유지, 재화 부족 시 색깔만 빨간색으로
        for (int i = 0; i < WeaponBoxDiamondCosts.Length; i++)
        {
            bool canBuy = CGoldManager.Instance.Diamond >= WeaponBoxDiamondCosts[i];
            SetWeaponShopButtonState(i + 3, canBuy);
            SetWeaponShopCostTextColor(i + 3, !canBuy);
        }
    }

    private void SetWeaponShopButtonState(int index, bool interactable)
    {
        if (index >= _weaponShopButtons.Length || _weaponShopButtons[index] == null) return;
        _weaponShopButtons[index].interactable = interactable;
        ColorBlock cb = _weaponShopButtons[index].colors;
        cb.disabledColor = Color.white;
        _weaponShopButtons[index].colors = cb;
    }

    // 텍스트 내용은 변경하지 않고 색깔만 변경합니다.
    private void SetWeaponShopCostTextColor(int index, bool insufficient)
    {
        if (index >= _weaponShopCostTexts.Length || _weaponShopCostTexts[index] == null) return;
        _weaponShopCostTexts[index].color = insufficient ? Color.red : Color.yellow;
    }

    /// <summary>아이템 상점 버튼 4개의 interactable 상태와 비용 텍스트 색깔을 갱신합니다.</summary>
    private void RefreshItemShopButtonStates()
    {
        if (CGoldManager.Instance == null) return;

        int   gold       = CGoldManager.Instance.Gold;
        int[] costs      = { HpPotionCost, ManaPotionCost, WeaponScrollCost, InventoryExpandCost };

        for (int i = 0; i < _itemShopButtons.Length; i++)
        {
            bool canAfford = gold >= costs[i];
            SetItemShopButtonState(i, canAfford);
            SetItemShopCostTextColor(i, !canAfford);
        }
    }

    private void SetItemShopButtonState(int index, bool interactable)
    {
        if (index >= _itemShopButtons.Length || _itemShopButtons[index] == null) return;
        _itemShopButtons[index].interactable = interactable;
        ColorBlock cb = _itemShopButtons[index].colors;
        cb.disabledColor = Color.white;
        _itemShopButtons[index].colors = cb;
    }

    private void SetItemShopCostTextColor(int index, bool insufficient)
    {
        if (index >= _itemShopButtons.Length || _itemShopButtons[index] == null) return;
        Text costText = _itemShopButtons[index].GetComponentInChildren<Text>();
        if (costText == null) return;
        costText.color = insufficient ? Color.red : Color.yellow;
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
        CDebug.Log($"[CGoldShopUI] 주간 구매 횟수 초기화 (기준: {monday})");
    }

    /// <summary>일일 리셋이 필요하면 구매 횟수를 초기화하고 저장합니다.</summary>
    private static void CheckAndResetDaily(CSaveData data)
    {
        string today = GetKSTTodayString();
        if (data.shopDailyResetDate == today) return;

        data.shopDailyResetDate = today;
        data.shopDailyBuyCount  = 0;
        SaveData(data);
        CDebug.Log($"[CGoldShopUI] 일일 구매 횟수 초기화 (기준: {today})");
    }

    #endregion

    #region Save Helpers

    private static CSaveData GetSaveData()
    {
        if (CJsonManager.Instance == null)
        {
            CDebug.LogError("[CGoldShopUI] CJsonManager.Instance가 null입니다.");
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
