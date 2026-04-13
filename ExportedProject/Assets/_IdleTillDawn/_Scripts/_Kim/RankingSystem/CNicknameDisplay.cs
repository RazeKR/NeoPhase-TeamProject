using TMPro;
using UnityEngine;

/// <summary>
/// 게임 씬에서 플레이어 닉네임을 TextMeshProUGUI에 표시하는 컴포넌트입니다.
/// 씬에 배치된 TMP Text 오브젝트에 부착하여 사용합니다.
///
/// [우선순위]
/// 1. CJsonManager.CurrentSaveData.nickname 사용
/// 2. CJsonManager 미존재 시 PlayerPrefs "PlayerNickname" 폴백
/// 3. 둘 다 없으면 "플레이어" 기본값 표시
///
/// [확장성]
/// - 닉네임 외에 캐릭터 이름, 레벨, 최고 스테이지 등을 추가 표시하려면
///   ShowCharacterName(), ShowLevel(), ShowHighestStage() 메서드를 인스펙터에서 연결하세요.
/// </summary>
public class CNicknameDisplay : MonoBehaviour
{
    #region Inspector

    [Header("닉네임 텍스트")]
    [SerializeField] private TextMeshProUGUI _nicknameText;

    [Header("포맷 (선택)")]
    [Tooltip("닉네임 앞뒤에 붙일 텍스트. 예: '{0}' or '[ {0} ]'")]
    [SerializeField] private string _format = "{0}";

    [Header("추가 표시 (확장)")]
    [Tooltip("캐릭터 이름을 표시할 Text (미연결 시 무시)")]
    [SerializeField] private TextMeshProUGUI _characterNameText;

    [Tooltip("레벨을 표시할 Text (미연결 시 무시)")]
    [SerializeField] private TextMeshProUGUI _levelText;

    [Tooltip("최고 스테이지를 표시할 Text (미연결 시 무시)")]
    [SerializeField] private TextMeshProUGUI _highestStageText;

    #endregion

    #region Unity

    private void Start()
    {
        Refresh();
    }

    #endregion

    #region Public API

    /// <summary>세이브 데이터를 다시 읽어 모든 텍스트를 갱신합니다.</summary>
    public void Refresh()
    {
        CSaveData data = GetSaveData();

        ShowNickname(data);
        ShowCharacterName(data);
        ShowLevel(data);
        ShowHighestStage(data);
    }

    #endregion

    #region Private

    private CSaveData GetSaveData()
    {
        if (CJsonManager.Instance != null)
            return CJsonManager.Instance.GetOrCreateSaveData();
        return null;
    }

    private void ShowNickname(CSaveData data)
    {
        if (_nicknameText == null) return;

        string nickname;

        if (data != null && !string.IsNullOrEmpty(data.nickname))
            nickname = data.nickname;
        else
            nickname = PlayerPrefs.GetString("PlayerNickname", "플레이어");

        _nicknameText.text = string.Format(_format, nickname);
    }

    private void ShowCharacterName(CSaveData data)
    {
        if (_characterNameText == null || data == null) return;

        // EPlayerType을 문자열로 변환 — 기획 이름이 있다면 DataManager에서 조회 가능
        _characterNameText.text = data.characterType.ToString();
    }

    private void ShowLevel(CSaveData data)
    {
        if (_levelText == null || data == null) return;
        _levelText.text = $"Lv.{data.playerLevel}";
    }

    private void ShowHighestStage(CSaveData data)
    {
        if (_highestStageText == null || data == null) return;
        _highestStageText.text = $"Best: Stage {data.highestStageId + 1}";
    }

    #endregion
}
