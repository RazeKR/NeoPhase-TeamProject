using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 펫 인벤토리 패널 전체를 관리하는 싱글톤 UI 컨트롤러입니다.
///
/// [패널 구조]
///   ┌───────────────────────────────────────────────┐
///   │  [펫 보관함]  [펫 소환]                  [✕]  │  ← 카테고리 탭
///   ├─────────────────────────────┬─────────────────┤
///   │  ScrollView (카드 그리드)   │  선택 정보 패널  │
///   │  ┌──┐┌──┐┌──┐┌──┐...       │  아이콘 + 이름   │
///   │  │  ││  ││  ││  │          │  장착 효과 설명  │
///   │  └──┘└──┘└──┘└──┘          │  [장착] [강화]   │
///   └─────────────────────────────┴─────────────────┘
///
/// [그리드 정렬]
///   등급 내림차순(Legendary→Common) → 펫 ID 오름차순 → 기본 카드(+0) → 강화 카드(+N)
///
/// [카드 배치 규칙]
///   - 전체 (펫 SO × 4등급) 조합이 항상 미리 배치 (미보유 = 회색/비활성)
///   - 동일 펫+등급+강화단계가 여러 개일 때 수량 중첩 표시
///   - 강화하면 해당 인스턴스가 강화 카드 칸으로 이동 (기본 카드 수량 -1)
///
/// [강화 비용]
///   다이아 5,000개 / 1단계. 최대 +10.
/// </summary>
public class CPetInventoryUI : MonoBehaviour
{
    #region Constants

    private const int EnhanceCost = 5000;

    // 등급 이름 표기
    private static readonly string[] GradeNames = { "Common", "Rare", "Epic", "Legendary" };

    #endregion

    #region Inspector — 패널

    [Header("루트 패널")]
    [SerializeField] private GameObject _petInventoryPanel = null;

    [Header("보유 현황 텍스트")]
    [SerializeField] private TextMeshProUGUI _petBoxCountText = null;   // 펫 소환권 보유 수
    [SerializeField] private TextMeshProUGUI _diamondCountText = null;  // 다이아 보유 수

    #endregion

    #region Inspector — 탭 버튼

    [Header("상단 카테고리 탭")]
    [SerializeField] private Button     _petListTabButton  = null;  // 펫 보관함 탭
    [SerializeField] private Button     _petGachaTabButton = null;  // 펫 소환 탭
    [SerializeField] private Button     _closeButton       = null;  // 닫기 버튼
    [SerializeField] private GameObject _petListPanel      = null;  // 보관함 패널
    [SerializeField] private GameObject _petGachaPanel     = null;  // 소환 패널

    [Header("탭 선택 색상")]
    [SerializeField] private Color _tabSelectedColor = new Color(1f, 0.9f, 0.4f);   // 밝은 노란색
    [SerializeField] private Color _tabNormalColor   = Color.white;

    #endregion

    #region Inspector — 그리드

    [Header("카드 그리드 (ScrollView Content)")]
    [SerializeField] private Transform  _contentParent = null;  // ScrollView Content
    [SerializeField] private GameObject _slotPrefab    = null;  // CPetSlot 프리팹

    #endregion

    #region Inspector — 선택 정보 패널 (우측)

    [Header("선택 정보 패널")]
    [SerializeField] private GameObject _infoPanel       = null;
    [SerializeField] private Image    _infoGradeImage  = null;  // 카드와 동일한 등급 색상 이미지
    [SerializeField] private Sprite[] _infoGradeSprites = null; // [0]=Common [1]=Rare [2]=Epic [3]=Legendary
    [SerializeField] private Image    _infoPetIcon     = null;  // 등급 이미지 위에 올라가는 펫 아이콘
    [SerializeField] private TextMeshProUGUI _infoPetName     = null;  // 펫 이름
    [SerializeField] private Text            _infoUpgradeText = null;  // 강화 단계 (+N / +10)
    [SerializeField] private TextMeshProUGUI _infoCountText   = null;  // 보유 수량 (×N)
    [SerializeField] private TextMeshProUGUI _infoBuffText    = null;  // 장착 효과 설명 (자동 생성)

    [Header("하단 버튼")]
    [SerializeField] private Button          _equipButton     = null;
    [SerializeField] private TextMeshProUGUI _equipButtonText = null;   // "장착" / "해제"
    [SerializeField] private Button          _enhanceButton   = null;
    [SerializeField] private TextMeshProUGUI _enhanceCostText = null;   // "◆ 5,000" / "최대 강화"

    [Header("강화 이펙트")]
    [SerializeField] private Animator        _upgradeAnimator    = null;  // UpgradeAnimator.controller 연결
    [SerializeField] private AudioClip       _upgradeSuccessClip = null;  // 강화 성공 사운드
    [SerializeField] private AudioClip       _upgradeFailClip    = null;  // 강화 실패 사운드

    #endregion

    #region Private

    private readonly List<CPetSlot> _slots = new List<CPetSlot>();
    private CPetInstance _selectedPet  = null;
    private CPetSlot     _selectedSlot = null;

    #endregion

    #region Properties

    public static CPetInventoryUI Instance { get; private set; }

    /// <summary>현재 선택된 펫 인스턴스. CPetSlot에서 참조합니다.</summary>
    public CPetInstance SelectedPet => _selectedPet;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // [중요] Awake에서 SetActive(false)를 호출하면 Pet_InventoryPanel이 비활성화되어
        // 같은 패널에 붙어있는 CPetInventorySystem·CPetBuffApplier의 Awake가 실행되지 않습니다.
        // (씬에서 CPetInventoryUI가 Pet_InventoryPanel보다 먼저 배치되어 있기 때문)
        // → SetActive 호출을 Start()로 이동합니다.
        // Start()도 첫 Update() 전에 실행되므로 패널이 화면에 노출되지 않습니다.

        // 명시적 호출 전까지 이펙트 오브젝트 자체를 숨김 (마지막 프레임 잔상 방지)
        if (_upgradeAnimator != null)
        {
            _upgradeAnimator.enabled = false;
            _upgradeAnimator.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Pet_InventoryPanel과 infoPanel 숨김
        // Awake 단계가 모두 완료된 후 실행되므로
        // CPetInventorySystem·CPetBuffApplier의 Awake 실행이 보장됩니다.
        if (_petInventoryPanel != null) _petInventoryPanel.SetActive(false);
        if (_infoPanel         != null) _infoPanel.SetActive(false);

        _closeButton?.onClick.AddListener(OnOffPetInventoryUI);
        _petListTabButton?.onClick.AddListener(() => SelectTab(0));
        _petGachaTabButton?.onClick.AddListener(() => SelectTab(1));
        _equipButton?.onClick.AddListener(ClickEquip);
        _enhanceButton?.onClick.AddListener(ClickEnhance);

        if (CPetInventorySystem.Instance != null)
            CPetInventorySystem.Instance.OnPetInventoryChanged += OnInventoryChanged;

        CGoldShopUI.OnPetBoxCountChanged += OnPetBoxCountChanged;
        if (CGoldManager.Instance != null)
            CGoldManager.Instance.OnDiamondChanged += OnDiamondChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (CPetInventorySystem.Instance != null)
            CPetInventorySystem.Instance.OnPetInventoryChanged -= OnInventoryChanged;

        CGoldShopUI.OnPetBoxCountChanged -= OnPetBoxCountChanged;
        if (CGoldManager.Instance != null)
            CGoldManager.Instance.OnDiamondChanged -= OnDiamondChanged;
    }

    #endregion

    #region Public Methods — 패널 토글

    /// <summary>외부 버튼(인벤토리 버튼 등)에서 호출하여 패널을 열고 닫습니다.</summary>
    public void OnOffPetInventoryUI()
    {
        if (_petInventoryPanel == null) return;

        bool open = !_petInventoryPanel.activeSelf;
        _petInventoryPanel.SetActive(open);

        if (open)
        {
            ClearSelection();
            RefreshCurrencyDisplay();
            SelectTab(0);
            BuildGrid();
        }
        else
        {
            ClearSelection();
        }
    }

    #endregion

    #region Public Methods — 선택 / 장착 / 강화

    /// <summary>CPetSlot에서 호출합니다. 해당 인스턴스를 선택하고 정보 패널을 갱신합니다.</summary>
    public void SelectPet(CPetInstance instance, CPetSlot slot)
    {
        // 이전 선택 슬롯 하이라이트 해제
        if (_selectedSlot != null)
            _selectedSlot.SetSelectedHighlight(false);

        _selectedPet  = instance;
        _selectedSlot = slot;
        slot.SetSelectedHighlight(true);

        UpdateInfoPanel();
    }

    /// <summary>장착 / 해제 버튼 클릭 처리.</summary>
    public void ClickEquip()
    {
        if (_selectedPet == null || CPetInventorySystem.Instance == null) return;

        if (_selectedPet._isEquipped)
        {
            CPetInventorySystem.Instance.UnequipPet();
            CDebug.Log($"[CPetInventoryUI] 펫 해제: {_selectedPet._data?.ItemName}");
        }
        else
        {
            CPetInventorySystem.Instance.EquipPet(_selectedPet._instanceID);
            CDebug.Log($"[CPetInventoryUI] 펫 장착: {_selectedPet._data?.ItemName} | " +
                       $"등급: {GradeNames[_selectedPet._rank]} | 강화: +{_selectedPet._upgrade} | " +
                       BuildBuffLog(_selectedPet));
        }

        // BuildGrid()는 OnPetInventoryChanged 이벤트 통해 자동 호출
        UpdateInfoPanel();
    }

    /// <summary>강화 버튼 클릭 처리. 다이아 5,000개 소모, 최대 +10. 확률 판정 후 성공 시 강화.</summary>
    public void ClickEnhance()
    {
        if (_selectedPet == null) return;

        if (_selectedPet.IsMaxUpgrade)
        {
            CDebug.Log("[CPetInventoryUI] 이미 최대 강화 단계입니다. (+10)");
            return;
        }

        if (CGoldManager.Instance == null || CGoldManager.Instance.Diamond < EnhanceCost)
        {
            CDebug.Log($"[CPetInventoryUI] 다이아 부족 " +
                       $"(보유: {CGoldManager.Instance?.Diamond ?? 0:N0}, 필요: {EnhanceCost:N0})");
            return;
        }

        int successChance = CPetInventorySystem.GetUpgradeSuccessChance(_selectedPet._upgrade);
        int roll          = UnityEngine.Random.Range(0, 100); // 0~99
        bool success      = roll < successChance;

        int prevUpgrade = _selectedPet._upgrade;
        CGoldManager.Instance.SpendDiamond(EnhanceCost);

        if (success)
        {
            CPetInventorySystem.Instance.UpgradePet(_selectedPet._instanceID);
            CDebug.Log($"[CPetInventoryUI] 펫 강화 성공! {_selectedPet._data?.ItemName} " +
                       $"+{prevUpgrade} → +{_selectedPet._upgrade} " +
                       $"(확률: {successChance}% / 다이아 {EnhanceCost:N0} 소모)");
        }
        else
        {
            CDebug.Log($"[CPetInventoryUI] 펫 강화 실패. {_selectedPet._data?.ItemName} " +
                       $"+{prevUpgrade} 유지 " +
                       $"(확률: {successChance}% / 다이아 {EnhanceCost:N0} 소모)");
        }

        PlayUpgradeEffect(success);

        UpdateInfoPanel();
    }

    #endregion

    #region Private — 보유 현황 갱신

    private void RefreshCurrencyDisplay()
    {
        if (_petBoxCountText != null)
        {
            int boxCount = CJsonManager.Instance?.CurrentSaveData?.petBoxCount ?? 0;
            _petBoxCountText.text = boxCount.ToString();
        }

        if (_diamondCountText != null)
        {
            int diamond = CGoldManager.Instance?.Diamond ?? 0;
            _diamondCountText.text = diamond.ToString("N0");
        }
    }

    #endregion

    #region Private — 그리드 빌드

    /// <summary>
    /// 전체 카드 그리드를 재구성합니다.
    /// - 모든 (펫 SO × 4등급) 기본 카드를 항상 배치
    /// - 강화된 카드(upgrade > 0)는 동적으로 추가
    /// - 정렬: 등급 내림차순 → 펫ID 오름차순 → 기본→강화
    /// </summary>
    private void BuildGrid()
    {
        // 이전 선택 하이라이트 제거 후 슬롯 파괴
        if (_selectedSlot != null) _selectedSlot.SetSelectedHighlight(false);
        _selectedSlot = null;

        foreach (CPetSlot slot in _slots)
            if (slot != null && slot.gameObject != null)
                Destroy(slot.gameObject);
        _slots.Clear();

        if (CDataManager.Instance == null || _contentParent == null || _slotPrefab == null) return;

        var allPets    = CDataManager.Instance.GetAllPets();
        var ownedPets  = CPetInventorySystem.Instance?.Pets ?? new List<CPetInstance>();

        // ── 기본 카드 (upgrade = 0): ALL petId × 4 rank ─────────────────────
        // 등급 내림차순(3→0) × petId 오름차순
        for (int rank = 3; rank >= 0; rank--)
        {
            foreach (var kv in allPets.OrderBy(p => p.Key))
            {
                int petId = kv.Key;
                List<CPetInstance> matches = ownedPets
                    .Where(p => p._itemData.Id == petId && p._rank == rank && p._upgrade == 0)
                    .ToList();

                CreateSlot(petId, rank, 0, matches);
            }
        }

        // ── 강화 카드 (upgrade > 0): 보유 인스턴스에서 수집 ────────────────
        // 동일 (petId, rank, upgrade)를 수량 중첩으로 묶어서 표시
        var enhancedGroups = ownedPets
            .Where(p => p._upgrade > 0)
            .GroupBy(p => (petId: p._itemData.Id, rank: p._rank, upgrade: p._upgrade))
            .OrderByDescending(g => g.Key.rank)
            .ThenBy(g => g.Key.petId)
            .ThenBy(g => g.Key.upgrade);

        foreach (var group in enhancedGroups)
            CreateSlot(group.Key.petId, group.Key.rank, group.Key.upgrade, group.ToList());

        // ── 선택 상태 복원 ────────────────────────────────────────────────────
        if (_selectedPet != null)
        {
            foreach (CPetSlot slot in _slots)
            {
                if (slot.GetInstances().Contains(_selectedPet))
                {
                    _selectedSlot = slot;
                    slot.SetSelectedHighlight(true);
                    break;
                }
            }
        }
    }

    private void CreateSlot(int petId, int rank, int upgrade, List<CPetInstance> instances)
    {
        GameObject go   = Instantiate(_slotPrefab, _contentParent);
        CPetSlot   slot = go.GetComponent<CPetSlot>();
        if (slot == null)
        {
            CDebug.LogError("[CPetInventoryUI] SlotPrefab에 CPetSlot 컴포넌트가 없습니다.");
            Destroy(go);
            return;
        }
        slot.Setup(petId, rank, upgrade, instances);
        _slots.Add(slot);
    }

    #endregion

    #region Private — 정보 패널 갱신

    private void UpdateInfoPanel()
    {
        if (_selectedPet == null)
        {
            if (_infoPanel != null) _infoPanel.SetActive(false);
            return;
        }

        if (_infoPanel != null) _infoPanel.SetActive(true);

        CPetDataSO data = _selectedPet._data;

        // 등급 색상 이미지 — 선택된 카드와 동일하게 표시
        if (_infoGradeImage != null && _infoGradeSprites != null
            && _selectedPet._rank < _infoGradeSprites.Length)
            _infoGradeImage.sprite = _infoGradeSprites[_selectedPet._rank];

        // 펫 아이콘 — 등급 이미지 위에 표시
        if (_infoPetIcon != null && data?.ItemSprite != null)
            _infoPetIcon.sprite = data.ItemSprite;

        // 이름
        if (_infoPetName != null)
            _infoPetName.text = data?.ItemName ?? string.Empty;

        // 강화 단계
        if (_infoUpgradeText != null)
            _infoUpgradeText.text = $"+{_selectedPet._upgrade}";

        // 보유 수량 — 선택된 슬롯에서 가져옴
        if (_infoCountText != null)
        {
            int count = _selectedSlot != null ? _selectedSlot.GetInstances().Count : 1;
            _infoCountText.text = $"보유: ×{count}";
        }

        // 장착 효과 설명 (코드 수치 기반 자동 생성)
        if (_infoBuffText != null)
            _infoBuffText.text = BuildBuffDescription(_selectedPet);

        // 장착 버튼 텍스트
        if (_equipButtonText != null)
            _equipButtonText.text = _selectedPet._isEquipped ? "해제" : "장착";

        // 강화 버튼 상태
        bool maxUpgrade = _selectedPet.IsMaxUpgrade;
        bool hasEnoughDiamond = CGoldManager.Instance != null &&
                                CGoldManager.Instance.Diamond >= EnhanceCost;

        if (_enhanceButton != null)
            _enhanceButton.interactable = !maxUpgrade && hasEnoughDiamond;

        if (_enhanceCostText != null)
        {
            if (maxUpgrade)
                _enhanceCostText.text = "최대 강화";
            else
            {
                int chance = CPetInventorySystem.GetUpgradeSuccessChance(_selectedPet._upgrade);
                _enhanceCostText.text = $"{EnhanceCost:N0}  ({chance}%)";
            }
        }
    }

    /// <summary>펫 인스턴스의 모든 버프를 문자열로 자동 생성합니다.</summary>
    private string BuildBuffDescription(CPetInstance pet)
    {
        if (pet?._data == null) return string.Empty;

        StringBuilder sb = new StringBuilder();

        // 공통 버프: 경험치 증가
        sb.AppendLine($"경험치 획득량  +{pet.GetXpBoostPercent():F0}%");

        // 공통: 펫 자체 공격력
        sb.AppendLine($"펫 공격력  {pet.GetPetAttackPower():F0}");

        // 타입별 버프
        switch (pet._data.PetType)
        {
            case EPetType.ProjectileBoost:
                sb.AppendLine($"투사체 수  +{pet.GetTotalProjectileBonus()}개");
                break;
            case EPetType.AttackPowerBoost:
                sb.AppendLine($"플레이어 공격력  +{pet.GetTotalAttackPowerPercent():F0}%");
                break;
            case EPetType.AttackSpeedBoost:
                sb.AppendLine($"플레이어 공격속도  +{pet.GetTotalAttackSpeedPercent():F0}%");
                break;
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>장착 로그용 간략 버프 문자열을 반환합니다.</summary>
    private string BuildBuffLog(CPetInstance pet)
    {
        if (pet?._data == null) return string.Empty;

        string typeBuff = pet._data.PetType switch
        {
            EPetType.ProjectileBoost  => $"투사체 +{pet.GetTotalProjectileBonus()}개",
            EPetType.AttackPowerBoost => $"공격력 +{pet.GetTotalAttackPowerPercent():F0}%",
            EPetType.AttackSpeedBoost => $"공격속도 +{pet.GetTotalAttackSpeedPercent():F0}%",
            _                         => string.Empty
        };

        return $"경험치 +{pet.GetXpBoostPercent():F0}% | 펫공격력 {pet.GetPetAttackPower():F0} | {typeBuff}";
    }

    /// <summary>강화 성공/실패 애니메이션과 사운드를 재생합니다.</summary>
    private void PlayUpgradeEffect(bool success)
    {
        if (_upgradeAnimator != null)
        {
            string stateName = success ? "Upgrade" : "Fail";
            _upgradeAnimator.gameObject.SetActive(true);
            _upgradeAnimator.enabled = true;
            _upgradeAnimator.Play(stateName, 0, 0f);
            StartCoroutine(DisableAnimatorAfterPlay(stateName));
        }

        AudioClip clip = success ? _upgradeSuccessClip : _upgradeFailClip;
        if (clip != null)
            CAudioManager.Instance?.PlaySFX(clip);
    }

    /// <summary>애니메이션 클립 길이만큼 기다린 뒤 Animator를 비활성화합니다.</summary>
    private IEnumerator DisableAnimatorAfterPlay(string stateName)
    {
        yield return null;  // 스테이트 전환이 적용될 때까지 한 프레임 대기

        AnimatorStateInfo info = _upgradeAnimator.GetCurrentAnimatorStateInfo(0);
        float clipLength = info.IsName(stateName) ? info.length : 0f;
        yield return new WaitForSeconds(clipLength);

        _upgradeAnimator.enabled = false;
        _upgradeAnimator.gameObject.SetActive(false);
    }

    #endregion

    #region Private — 탭 / 공통

    /// <summary>탭을 전환하고 버튼 색상을 갱신합니다. 0 = 보관함, 1 = 소환.</summary>
    private void SelectTab(int index)
    {
        ClearSelection();

        if (_petListPanel  != null) _petListPanel.SetActive(index == 0);
        if (_petGachaPanel != null) _petGachaPanel.SetActive(index == 1);

        SetTabColor(_petListTabButton,  index == 0);
        SetTabColor(_petGachaTabButton, index == 1);

        // 보관함 탭 전환 시 그리드를 항상 최신 상태로
        if (index == 0) BuildGrid();
    }

    private void SetTabColor(Button btn, bool selected)
    {
        if (btn == null) return;

        Color targetColor = selected ? _tabSelectedColor : _tabNormalColor;

        // 버튼 자신의 Image는 제외하고, 자식 오브젝트의 Image / Text / TMP만 색상 변경
        foreach (Image img in btn.GetComponentsInChildren<Image>())
            if (img.gameObject != btn.gameObject)
                img.color = targetColor;

        foreach (Text txt in btn.GetComponentsInChildren<Text>())
            if (txt.gameObject != btn.gameObject)
                txt.color = targetColor;

        foreach (TextMeshProUGUI tmp in btn.GetComponentsInChildren<TextMeshProUGUI>())
            if (tmp.gameObject != btn.gameObject)
                tmp.color = targetColor;
    }

    private void ClearSelection()
    {
        if (_selectedSlot != null) _selectedSlot.SetSelectedHighlight(false);
        _selectedPet  = null;
        _selectedSlot = null;
        if (_infoPanel != null) _infoPanel.SetActive(false);
    }

    private void OnPetBoxCountChanged(int count)
    {
        if (_petBoxCountText != null)
            _petBoxCountText.text = count.ToString();
    }

    private void OnDiamondChanged(int amount)
    {
        if (_diamondCountText != null)
            _diamondCountText.text = amount.ToString("N0");
    }

    private void OnInventoryChanged()
    {
        if (_petInventoryPanel == null || !_petInventoryPanel.activeSelf) return;
        RefreshCurrencyDisplay();
        BuildGrid();

        // 선택 중인 펫이 인벤토리에 여전히 존재하면 정보 패널 갱신
        if (_selectedPet != null)
        {
            bool stillExists = CPetInventorySystem.Instance != null &&
                               CPetInventorySystem.Instance.Pets.Contains(_selectedPet);
            if (stillExists)
                UpdateInfoPanel();
            else
                ClearSelection();
        }
    }

    #endregion
}
