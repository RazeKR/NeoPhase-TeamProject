using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 캐릭터 선택 패널 전체를 제어합니다.
///
/// [클릭 흐름]
/// 1회 클릭 → 해당 버튼 포커스 + 설명 / 포트레이트 표시
/// 같은 버튼 2회 클릭 → 캐릭터 확정 → Stage 씬 로드
/// 다른 버튼 클릭 → 이전 버튼 포커스 해제, 새 버튼 포커스
///
/// [필요 UI 구성]
/// DescriptionPanel  — 이름 / 설명 텍스트를 담은 패널 (처음엔 비활성)
/// PortraitImage     — 오른쪽 여백의 큰 캐릭터 이미지 (처음엔 비활성)
/// </summary>
public class CCharacterSelectUI : MonoBehaviour
{
    #region Inspector

    [Header("캐릭터 버튼 목록")]
    [Tooltip("씬에 배치된 CCharacterSelectButton 배열 (순서 무관)")]
    [SerializeField] private CCharacterSelectButton[] _buttons;

    [Header("설명 패널")]
    [Tooltip("캐릭터 이름 / 설명을 표시할 패널 루트 오브젝트")]
    [SerializeField] private GameObject _descriptionPanel;

    [Tooltip("캐릭터 이름 텍스트 (TMP)")]
    [SerializeField] private TextMeshProUGUI _nameText;

    [Tooltip("캐릭터 설명 텍스트 (TMP)")]
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Header("포트레이트 (오른쪽 큰 이미지)")]
    [Tooltip("큰 캐릭터 초상화 Image 컴포넌트")]
    [SerializeField] private Image _portraitImage;

    [Tooltip("포트레이트 위치를 제어할 RectTransform")]
    [SerializeField] private RectTransform _portraitRect;

    [Tooltip("위아래 핑퐁 진폭 (px)")]
    [SerializeField] private float _bobAmount = 12f;

    [Tooltip("위아래 핑퐁 속도")]
    [SerializeField] private float _bobSpeed  = 1.4f;

    [Header("씬 전환")]
    [Tooltip("캐릭터 확정 후 로드할 스테이지 씬 이름")]
    [SerializeField] private string _stageSceneName = "Stage1_KSH";

    [Header("기본 무기")]
    [Tooltip("최초 게임 시작 시 자동 지급할 무기 ID (WeaponData_Revolver._id = 6)")]
    [SerializeField] private int _defaultWeaponId = 6;

    #endregion

    #region Private

    private CCharacterSelectButton _focusedButton;  // 현재 포커스된 버튼

    /// <summary>현재 선택(포커스)된 캐릭터 ID. 선택 전이면 -1을 반환합니다.</summary>
    public int SelectedCharacterId => _focusedButton != null ? _focusedButton.Data.Id : -1;
    private Vector2   _portraitOrigin;
    private Coroutine _bobCoroutine;

    #endregion

    #region Unity

    private void Awake()
    {
        if (_portraitRect  != null) _portraitOrigin = _portraitRect.anchoredPosition;
        if (_descriptionPanel != null) _descriptionPanel.SetActive(false);
        SetPortraitVisible(false);
    }

    private void Start()
    {
        // 모든 버튼의 onClick에 클릭 핸들러 등록
        foreach (CCharacterSelectButton btn in _buttons)
        {
            CCharacterSelectButton captured = btn;
            Button uiBtn = btn.GetComponent<Button>();
            if (uiBtn != null)
                uiBtn.onClick.AddListener(() => OnButtonClicked(captured));
        }
    }

    private void OnDisable()
    {
        // 패널이 닫힐 때 상태 초기화
        ClearFocus();
    }

    #endregion

    #region Click Logic

    private void OnButtonClicked(CCharacterSelectButton button)
    {
        if (_focusedButton == button)
        {
            // 같은 버튼 두 번째 클릭 → 확정
            ConfirmSelection();
            return;
        }

        // 다른 버튼 클릭 → 포커스 이동
        if (_focusedButton != null)
            _focusedButton.SetFocused(false);

        _focusedButton = button;
        _focusedButton.SetFocused(true);

        ShowInfo(button.Data);
    }

    private void ConfirmSelection()
    {
        if (_focusedButton == null || _focusedButton.Data == null) return;

        if (CJsonManager.Instance != null)
        {
            // 기존 데이터에 의존하지 않고 파일에서 다시 로드 후 덮어쓰기
            CSaveData saveData = CJsonManager.Instance.Load();

            // 선택한 캐릭터 저장
            saveData.playerStatId = _focusedButton.Data.Id;

            // 선택한 캐릭터 타입 저장
            saveData.characterType = _focusedButton.Data.CharacterType;

            // 기본 무기 미지급 상태면 Revolver(id=6) 인벤토리에 추가 + 장착
            if (saveData.equippedWeaponId == 0)
            {
                saveData.equippedWeaponId = _defaultWeaponId;

                // 인벤토리 아이템 리스트에도 추가해야 RestoreFromSaveData에서 정상 복원됨
                saveData.inventorySaveData.items.Add(new CItemSaveData
                {
                    itemID     = _defaultWeaponId,
                    instanceID = System.Guid.NewGuid().ToString(),
                    rank       = 0,
                    isEquipped = true,
                    upgrade    = 0,
                    amount     = 1,
                    type       = EItemType.Weapon
                });
            }

            CJsonManager.Instance.Save(saveData);
        }

        CGameManager.Instance.MarkGameEntered(_focusedButton.Data.Id);
        SceneManager.LoadScene(_stageSceneName);
    }

    #endregion

    #region Info Display

    private void ShowInfo(CPlayerDataSO data)
    {
        if (data == null) return;

        // 텍스트
        if (_nameText        != null) _nameText.text        = data.CharacterName;
        if (_descriptionText != null) _descriptionText.text = data.Description;
        if (_descriptionPanel != null) _descriptionPanel.SetActive(true);

        // 포트레이트
        if (_portraitImage != null && data.CharacterPortrait != null)
        {
            _portraitImage.sprite = data.CharacterPortrait;
            SetPortraitVisible(true);
        }

        // 핑퐁 재시작
        if (_bobCoroutine != null) StopCoroutine(_bobCoroutine);
        _bobCoroutine = StartCoroutine(Co_BobPortrait());
    }

    private void ClearFocus()
    {
        if (_focusedButton != null)
        {
            _focusedButton.SetFocused(false);
            _focusedButton = null;
        }

        if (_descriptionPanel != null) _descriptionPanel.SetActive(false);
        SetPortraitVisible(false);

        if (_bobCoroutine != null)
        {
            StopCoroutine(_bobCoroutine);
            _bobCoroutine = null;
        }
    }

    private void SetPortraitVisible(bool visible)
    {
        if (_portraitImage != null)
            _portraitImage.gameObject.SetActive(visible);
    }

    #endregion

    #region Bob Coroutine

    private IEnumerator Co_BobPortrait()
    {
        if (_portraitRect == null) yield break;

        float elapsed = 0f;
        while (true)
        {
            elapsed += Time.deltaTime;
            float offsetY = Mathf.Sin(elapsed * _bobSpeed * Mathf.PI) * _bobAmount;
            _portraitRect.anchoredPosition = _portraitOrigin + new Vector2(0f, offsetY);
            yield return null;
        }
    }

    #endregion
}
