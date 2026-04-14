using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 펫 인벤토리 그리드 안의 카드 슬롯 하나를 나타냅니다.
///
/// [프리팹 계층 구조]
///   Button (이 컴포넌트 부착, 배경이미지 = 카드 테두리)
///     └ 등급 색상 이미지  (_gradeColorImage)
///         └ 펫 아이콘 이미지  (_petIconImage)
///             └ 장착중 표시 이미지  (_equippedMark, 기본 비활성)
///   * 수량 Text (_countText)        ← 계층 어디에 배치해도 무관
///   * 강화 단계 Text (_upgradeText)  ← 계층 어디에 배치해도 무관
///   * 선택 하이라이트 오브젝트 (_selectedHighlight) ← 기본 비활성
/// </summary>
public class CPetSlot : MonoBehaviour
{
    #region Inspector

    [SerializeField] private Button   _button           = null;
    [SerializeField] private Image    _gradeColorImage  = null;  // 등급별 배경 색상 이미지
    [SerializeField] private Sprite[] _gradeSprites     = null;  // [0]=Common [1]=Rare [2]=Epic [3]=Legendary
    [SerializeField] private Image    _petIconImage     = null;  // 펫 아이콘 (비보유 시 회색)
    [SerializeField] private GameObject _equippedMark   = null;  // 장착 표시
    [SerializeField] private Text     _upgradeText      = null;  // 강화 단계 (+N)
    [SerializeField] private GameObject _selectedHighlight = null; // 선택 하이라이트

    #endregion

    #region Private

    private int  _petDataSOId;
    private int  _rank;
    private int  _upgrade;
    private List<CPetInstance> _instances = new List<CPetInstance>();

    #endregion

    #region Properties

    public int  PetDataSOId  => _petDataSOId;
    public int  Rank         => _rank;
    public int  Upgrade      => _upgrade;
    public List<CPetInstance> GetInstances() => _instances;

    #endregion

    #region Public Methods

    /// <summary>
    /// 슬롯 데이터를 설정하고 표시를 갱신합니다.
    /// instances가 비어있으면 미보유 상태(비활성/회색)로 표시됩니다.
    /// </summary>
    public void Setup(int petDataSOId, int rank, int upgrade, List<CPetInstance> instances)
    {
        _petDataSOId = petDataSOId;
        _rank        = rank;
        _upgrade     = upgrade;
        _instances   = instances ?? new List<CPetInstance>();
        Refresh();
    }

    /// <summary>인스턴스 목록을 갱신하고 표시를 다시 그립니다.</summary>
    public void Refresh()
    {
        CPetDataSO so = CDataManager.Instance != null
            ? CDataManager.Instance.GetPet(_petDataSOId)
            : null;

        bool owned = _instances.Count > 0;

        // ── 버튼 활성/비활성 ───────────────────────────────────────────────
        if (_button != null)
            _button.interactable = owned;

        // ── 등급 색상 이미지 ────────────────────────────────────────────────
        if (_gradeColorImage != null && _gradeSprites != null && _rank < _gradeSprites.Length)
            _gradeColorImage.sprite = _gradeSprites[_rank];

        // ── 펫 아이콘 (미보유 = 회색, 보유 = 원본 컬러) ─────────────────────
        if (_petIconImage != null)
        {
            if (so?.ItemSprite != null)
                _petIconImage.sprite = so.ItemSprite;
            _petIconImage.color = owned ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        // ── 장착 표시 ────────────────────────────────────────────────────────
        bool anyEquipped = _instances.Any(p => p._isEquipped);
        if (_equippedMark != null)
            _equippedMark.SetActive(anyEquipped);

        // ── 강화 단계 텍스트 ─────────────────────────────────────────────────
        if (_upgradeText != null)
            _upgradeText.text = _upgrade > 0 ? $"+{_upgrade}" : string.Empty;
    }

    /// <summary>선택 하이라이트를 켜거나 끕니다.</summary>
    public void SetSelectedHighlight(bool selected)
    {
        if (_selectedHighlight != null)
            _selectedHighlight.SetActive(selected);
    }

    /// <summary>슬롯 클릭 — 버튼 OnClick 이벤트에 연결합니다.</summary>
    public void OnSlotClick()
    {
        if (_instances.Count == 0) return;
        if (CPetInventoryUI.Instance == null) return;

        // 장착 중인 인스턴스를 우선 선택, 없으면 첫 번째
        CPetInstance selected = _instances.Find(p => p._isEquipped) ?? _instances[0];
        CPetInventoryUI.Instance.SelectPet(selected, this);
    }

    #endregion
}
