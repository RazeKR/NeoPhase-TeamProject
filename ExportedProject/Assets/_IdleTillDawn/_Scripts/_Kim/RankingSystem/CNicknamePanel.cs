using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 메인 메뉴에서 캐릭터 선택 완료 후 표시되는 닉네임 입력 패널입니다.
/// CCharacterSelectUI에서 SetUp()을 호출하여 활성화합니다.
///
/// [입력 규칙]
/// - 최대 6글자
/// - 영문(대소문자) 및 완성형 한글(가~힣)만 허용
///
/// [의존 관계]
/// - CJsonManager.Instance : 닉네임 저장
/// - CGameManager.Instance : 씬 전환 진입 처리
/// - CRankingManager.Instance : (P_RankingSystem.prefab 기반) 랭킹 서버에 등록 (확장성)
/// </summary>
public class CNicknamePanel : MonoBehaviour
{
    #region Inspector

    [Header("UI 연결")]
    [SerializeField] private TMP_InputField _nicknameInputField;
    [SerializeField] private Button         _submitButton;
    [SerializeField] private TextMeshProUGUI _errorText;

    [Header("닉네임 규칙")]
    [SerializeField] private int _maxLength = 6;

    [Header("씬 전환")]
    [SerializeField] private string _stageSceneName = "Stage1_KSH";

    #endregion

    #region Private

    // 캐릭터 선택 화면에서 전달받은 값
    private int         _selectedPlayerId  = -1;
    private EPlayerType _selectedCharType;

    // 허용 문자 패턴: 영문 대소문자 + 완성형 한글
    private static readonly Regex ValidPattern = new Regex(@"^[a-zA-Z가-힣]+$");

    #endregion

    #region Unity

    private void Awake()
    {
        gameObject.SetActive(false); // 시작 시 비활성화

        if (_submitButton != null)
            _submitButton.onClick.AddListener(OnSubmitClicked);

        if (_errorText != null)
            _errorText.gameObject.SetActive(false);

        if (_nicknameInputField != null)
        {
            _nicknameInputField.characterLimit = _maxLength;
            _nicknameInputField.onValueChanged.AddListener(OnInputChanged);
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// 캐릭터 선택 완료 후 CCharacterSelectUI에서 호출합니다.
    /// 패널을 활성화하고, 이전에 저장된 닉네임을 불러옵니다.
    /// </summary>
    public void SetUp(int playerId, EPlayerType charType)
    {
        _selectedPlayerId = playerId;
        _selectedCharType = charType;

        // 기존 닉네임이 있으면 InputField에 미리 채워줌
        if (CJsonManager.Instance != null)
        {
            string saved = CJsonManager.Instance.GetOrCreateSaveData().nickname;
            if (!string.IsNullOrEmpty(saved) && saved != "이름 없는 플레이어")
                _nicknameInputField.text = saved;
            else
                _nicknameInputField.text = string.Empty;
        }

        HideError();
        gameObject.SetActive(true);

        // 바로 InputField에 포커스
        if (_nicknameInputField != null)
            _nicknameInputField.Select();
    }

    #endregion

    #region Private

    private void OnInputChanged(string value)
    {
        // 6글자 초과 분은 characterLimit이 이미 막아주므로 실시간 오류만 처리
        HideError();
    }

    private void OnSubmitClicked()
    {
        string nickname = _nicknameInputField != null ? _nicknameInputField.text.Trim() : string.Empty;

        if (!TryValidateNickname(nickname, out string errorMsg))
        {
            ShowError(errorMsg);
            return;
        }

        // 서버에서 닉네임 중복 확인 후 진입
        StartCoroutine(CoCheckNicknameAndEnter(nickname));
    }

    /// <summary>
    /// 서버 랭킹 목록에서 닉네임 중복을 확인한 뒤 게임에 진입합니다.
    /// 본인의 기존 닉네임(같은 UID)은 중복으로 처리하지 않습니다.
    /// </summary>
    private IEnumerator CoCheckNicknameAndEnter(string nickname)
    {
        // 버튼 비활성화 후 확인 중 메시지 표시
        if (_submitButton != null) _submitButton.interactable = false;
        ShowError("닉네임 확인 중...");

        bool isDuplicate = false;
        bool fetchDone   = false;

        string myUid = CJsonManager.Instance != null
            ? CJsonManager.Instance.GetOrCreateSaveData().uid
            : string.Empty;

        if (CRankingManager.Instance != null)
        {
            CRankingManager.Instance.GetRankingData((rankList) =>
            {
                if (rankList != null)
                {
                    foreach (CRankData data in rankList)
                    {
                        // 자기 자신의 UID는 중복 검사에서 제외 (닉네임 변경 허용)
                        if (data.uid == myUid) continue;

                        if (data.nickname == nickname)
                        {
                            isDuplicate = true;
                            break;
                        }
                    }
                }
                fetchDone = true;
            });
        }
        else
        {
            fetchDone = true; // RankingManager 없으면 중복 검사 생략
        }

        // 콜백 완료까지 대기
        yield return new WaitUntil(() => fetchDone);

        if (_submitButton != null) _submitButton.interactable = true;

        if (isDuplicate)
        {
            ShowError("이미 사용 중인 닉네임입니다.");
            yield break;
        }

        HideError();
        SaveAndEnterGame(nickname);
    }

    /// <summary>닉네임 유효성을 검사하고 오류 메시지를 out으로 반환합니다.</summary>
    private bool TryValidateNickname(string nickname, out string errorMsg)
    {
        if (string.IsNullOrEmpty(nickname))
        {
            errorMsg = "닉네임을 입력해 주세요.";
            return false;
        }

        if (nickname.Length > _maxLength)
        {
            errorMsg = $"닉네임은 최대 {_maxLength}글자까지 입력 가능합니다.";
            return false;
        }

        if (!ValidPattern.IsMatch(nickname))
        {
            errorMsg = "닉네임은 영문과 한글만 입력 가능합니다.";
            return false;
        }

        errorMsg = string.Empty;
        return true;
    }

    private void SaveAndEnterGame(string nickname)
    {
        if (CJsonManager.Instance != null)
        {
            // 닉네임 + 캐릭터 타입을 세이브 데이터에 반영
            CJsonManager.Instance.SavePlayerProfile(nickname, _selectedCharType);

            // CRankingManager가 있으면 서버에도 즉시 동기화 (P_RankingSystem.prefab 기반)
            if (CRankingManager.Instance != null)
                CRankingManager.Instance.SaveMyRanking(CJsonManager.Instance.CurrentSaveData);
        }

        // 씬 전환 처리
        if (CGameManager.Instance != null)
            CGameManager.Instance.MarkGameEntered(_selectedPlayerId);

        SceneManager.LoadScene(_stageSceneName);
    }

    private void ShowError(string message)
    {
        if (_errorText == null) return;
        _errorText.text = message;
        _errorText.gameObject.SetActive(true);
    }

    private void HideError()
    {
        if (_errorText != null)
            _errorText.gameObject.SetActive(false);
    }

    #endregion
}
