using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 리더보드의 한 행(Row)을 표시하는 컴포넌트입니다.
/// CLeaderboardPanel이 생성하고 SetData()로 데이터를 주입합니다.
/// </summary>
public class CLeaderboardRow : MonoBehaviour
{
    #region Inspector

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private TextMeshProUGUI _characterText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _stageText;

    [Header("배경 이미지")]
    [SerializeField] private Image _background;
    [SerializeField] private Color _evenRowColor = new Color(0.12f, 0.14f, 0.18f, 0.85f);
    [SerializeField] private Color _oddRowColor  = new Color(0.08f, 0.10f, 0.13f, 0.85f);

    [Header("1~3위 순위 색상")]
    [SerializeField] private Color _goldColor        = new Color(1f,    0.84f, 0f,    1f);
    [SerializeField] private Color _silverColor      = new Color(0.75f, 0.75f, 0.75f, 1f);
    [SerializeField] private Color _bronzeColor      = new Color(0.8f,  0.5f,  0.2f,  1f);
    [SerializeField] private Color _defaultRankColor = Color.white;

    [Header("내 행 닉네임 색상")]
    [SerializeField] private Color _myNicknameColor     = new Color(1f, 0.92f, 0.4f, 1f);
    [SerializeField] private Color _normalNicknameColor = Color.white;

    #endregion

    #region Public API

    /// <summary>행에 표시할 데이터를 주입합니다.</summary>
    /// <param name="rank">1-based 순위</param>
    /// <param name="data">서버 랭킹 데이터</param>
    /// <param name="isMyRow">내 UID와 일치하면 true</param>
    public void SetData(int rank, CRankData data, bool isMyRow)
    {
        SetRank(rank);
        SetNickname(data.nickname, isMyRow);
        SetCharacter(data.characterType);
        SetLevel(data.playerLevel);
        SetStage(data.highestStageIdx);
        SetBackground(rank);
    }

    #endregion

    #region Private

    private void SetRank(int rank)
    {
        if (_rankText == null) return;
        _rankText.text  = rank.ToString();
        _rankText.color = rank switch
        {
            1 => _goldColor,
            2 => _silverColor,
            3 => _bronzeColor,
            _ => _defaultRankColor
        };
    }

    private void SetNickname(string nickname, bool isMyRow)
    {
        if (_nicknameText == null) return;
        _nicknameText.text  = string.IsNullOrEmpty(nickname) ? "—" : nickname;
        _nicknameText.color = isMyRow ? _myNicknameColor : _normalNicknameColor;
    }

    private void SetCharacter(EPlayerType type)
    {
        if (_characterText == null) return;
        _characterText.text = type switch
        {
            EPlayerType.Dasher => "Dasher",
            EPlayerType.Hastur => "Hastur",
            EPlayerType.Spark  => "Spark",
            _                  => type.ToString()
        };
    }

    private void SetLevel(int level)
    {
        if (_levelText == null) return;
        _levelText.text = $"Lv.{level}";
    }

    private void SetStage(int stageIdx)
    {
        if (_stageText == null) return;
        _stageText.text = $"Stage {stageIdx + 1}";
    }

    // 짝수/홀수 행 배경색 교번
    private void SetBackground(int rank)
    {
        if (_background == null) return;
        _background.color = (rank % 2 == 0) ? _evenRowColor : _oddRowColor;
    }

    #endregion
}
