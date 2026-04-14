using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 카드 공개 패널에서 카드 한 장을 나타내는 컴포넌트입니다.
///
/// [프리팹 계층 구조]
///   Button (이 컴포넌트 부착)
///     ├ BackFace           ← 뒷면 (등급 색상 tint)
///     │   └ BackGradeImage
///     └ FrontFace          ← 앞면 (P_PetSlot과 동일 구조, 기본 비활성)
///         ├ GradeColorImage
///         └ PetIconImage
/// </summary>
public class CPetCardItem : MonoBehaviour
{
    #region Inspector

    [SerializeField] private GameObject _backFace         = null;
    [SerializeField] private Image      _backGradeImage   = null;  // 뒷면 등급 색상

    [SerializeField] private GameObject _frontFace        = null;
    [SerializeField] private Image      _frontGradeImage  = null;  // 앞면 등급 배경
    [SerializeField] private Image      _frontPetIcon     = null;  // 앞면 펫 아이콘

    [SerializeField] private Sprite[]   _gradeSprites     = null;  // [0]=Common … [3]=Legendary

    #endregion

    #region Private

    private CPetInstance _instance;
    private bool         _isRevealed = false;

    #endregion

    #region Public Methods

    public void Setup(CPetInstance instance)
    {
        _instance   = instance;
        _isRevealed = false;

        // 뒷면 등급 색상 설정
        if (_backGradeImage != null && _gradeSprites != null && instance._rank < _gradeSprites.Length)
            _backGradeImage.sprite = _gradeSprites[instance._rank];

        // 앞면 미리 세팅 (숨김 상태)
        SetupFrontFace();

        _backFace?.SetActive(true);
        _frontFace?.SetActive(false);
    }

    /// <summary>카드를 공개합니다 (뒷면 → 앞면).</summary>
    public void Reveal()
    {
        if (_isRevealed) return;
        _isRevealed = true;

        _backFace?.SetActive(false);
        _frontFace?.SetActive(true);
    }

    public bool IsRevealed => _isRevealed;

    /// <summary>버튼 OnClick 이벤트에 연결합니다.</summary>
    public void OnCardClick()
    {
        if (!_isRevealed)
        {
            Reveal();
            OnRevealedCallback?.Invoke();
        }
        else
        {
            // 앞면 클릭 → 카드 정보 패널 표시
            CPetCardInfoPanel.Instance?.Show(_instance);
        }
    }

    /// <summary>카드가 공개될 때 호출할 콜백. CPetCardRevealPanel에서 등록합니다.</summary>
    public System.Action OnRevealedCallback { get; set; }

    #endregion

    #region Private Methods

    private void SetupFrontFace()
    {
        if (_instance == null) return;

        if (_frontGradeImage != null && _gradeSprites != null && _instance._rank < _gradeSprites.Length)
            _frontGradeImage.sprite = _gradeSprites[_instance._rank];

        if (_frontPetIcon != null)
        {
            if (_instance._data?.ItemSprite != null)
                _frontPetIcon.sprite = _instance._data.ItemSprite;
        }
    }

    #endregion
}
