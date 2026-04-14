using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 공개된 카드를 클릭했을 때 표시되는 카드 정보 팝업입니다.
///
/// [패널 구조]
///   Panel
///     ├ GradeImage     ← 등급 배경 이미지 (HUD)
///     │   └ PetIcon    ← 펫 아이콘 (등급 이미지 위)
///     ├ PetNameText    ← 펫 이름 (TMP)
///     ├ BuffText       ← 장착 효과 설명 (TMP)
///     └ CloseButton    ← X 버튼
/// </summary>
public class CPetCardInfoPanel : MonoBehaviour
{
    #region Inspector

    [SerializeField] private GameObject        _panel          = null;
    [SerializeField] private Image             _gradeImage     = null;
    [SerializeField] private Sprite[]          _gradeSprites   = null;  // [0]=Common … [3]=Legendary
    [SerializeField] private Image             _petIcon        = null;
    [SerializeField] private TextMeshProUGUI   _petNameText    = null;
    [SerializeField] private TextMeshProUGUI   _buffText       = null;
    [SerializeField] private Button            _closeButton    = null;

    #endregion

    #region Properties

    public static CPetCardInfoPanel Instance { get; private set; }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _closeButton?.onClick.AddListener(Close);
        _panel?.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    #endregion

    #region Public Methods

    public void Show(CPetInstance instance)
    {
        if (instance == null || _panel == null) return;

        _panel.SetActive(true);

        // 등급 배경 이미지
        if (_gradeImage != null && _gradeSprites != null && instance._rank < _gradeSprites.Length)
            _gradeImage.sprite = _gradeSprites[instance._rank];

        // 펫 아이콘
        if (_petIcon != null && instance._data?.ItemSprite != null)
            _petIcon.sprite = instance._data.ItemSprite;

        // 이름
        if (_petNameText != null)
            _petNameText.text = instance._data?.ItemName ?? string.Empty;

        // 장착 효과
        if (_buffText != null)
            _buffText.text = BuildBuffDescription(instance);
    }

    public void Close()
    {
        _panel?.SetActive(false);
    }

    #endregion

    #region Private Methods

    private static readonly string[] GradeNames = { "Common", "Rare", "Epic", "Legendary" };

    private string BuildBuffDescription(CPetInstance pet)
    {
        if (pet?._data == null) return string.Empty;

        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"등급: {GradeNames[Mathf.Clamp(pet._rank, 0, 3)]}");
        sb.AppendLine($"경험치 획득량  +{pet.GetXpBoostPercent():F0}%");
        sb.AppendLine($"펫 공격력  {pet.GetPetAttackPower():F0}");

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

    #endregion
}
