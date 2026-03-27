using System;
using UnityEngine;

public enum EPlayerType
{
    Dasher,
    Hastur,
    Spark,
}

[Serializable]
public class CPlayerSaveData
{
    #region 인스펙터
    [Header("유저 고유 정보")]
    [SerializeField] private string _uid = "";
    [SerializeField] private string _nickName = "";
    [SerializeField] private EPlayerType _characterType;

    [Header("진행도 데이터")]
    [SerializeField] private int _level = 1;
    [SerializeField] private int _finalStage = 1;
    [SerializeField] private int _totalKills = 0;
    [SerializeField] private float _currentExp = 0f;

    [Header("상태 데이터")]
    [SerializeField] private float _savedHealth = 100f;
    #endregion

    #region 내부 변수
    private float[] _bonusStats;
    #endregion

    #region 프로퍼티
    public string Uid => _uid;
    public string NickName => _nickName;
    public EPlayerType CharacterType => _characterType;
    public int Level => _level;
    public int FinalStage => _finalStage;
    public float CurrentExp => _currentExp;
    public int TotalKills => _totalKills;
    public float SavedHealth => _savedHealth;
    public float[] BonusStats => _bonusStats;
    #endregion

    /// <summary>
    /// 플레이어 데이터 초기화
    /// </summary>
    /// <param name="newUid"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    public CPlayerSaveData(string newUid, string name, EPlayerType type)
    {
        _uid = newUid;
        _nickName = name;
        _characterType = type;

        _bonusStats = new float[(int)EPlayerStatType.Count];
    }

    /// <summary>
    /// 저장하기 직전 진행도를 갱신
    /// </summary>
    /// <param name="currentLevel">현재 레벨</param>
    /// <param name="finalStage">최종 도달 스테이지</param>
    /// <param name="currentExp">현재 경험치</param>
    /// <param name="savedHealth">저장 직전 체력</param>
    public void UpdateProgress(int currentLevel, int finalStage, float currentExp, float savedHealth, float[] currentBonusStats)
    {
        _level = currentLevel;
        _finalStage = finalStage;
        _currentExp = currentExp;
        _savedHealth = savedHealth;

        if (currentBonusStats != null)
        {
            currentBonusStats.CopyTo(_bonusStats, 0);
        }
    }

    /// <summary>
    /// 킬 수 증가 메서드
    /// </summary>
    public void AddKill() => _totalKills++;
}
