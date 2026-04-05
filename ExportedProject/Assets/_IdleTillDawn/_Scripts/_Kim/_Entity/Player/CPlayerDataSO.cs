using UnityEngine;

/// <summary>플레이어 스탯의 종류를 구분하는 열거형입니다.</summary>
public enum EPlayerStatType
{
    Health,         // 최대 체력
    Mana,           // 최대 마나
    Damage,         // 공격력
    AttackSpeed,    // 공격 속도
    HealthRegen,    // 체력 재생
    ManaRegen,      // 마나 재생
    MoveSpeed,      // 이동 속도
    ExpMultiplier,  // 경험치 배율
    Count           // 배열 크기용 (실제 스탯 아님)
}

/// <summary>
/// 스탯 타입과 기본값·성장치를 묶은 구조체입니다.
/// CPlayerDataSO의 _statSettings 배열 요소로 사용됩니다.
/// </summary>
[System.Serializable]
public struct PlayerStatInfo
{
    public EPlayerStatType StatType;    // 스탯 종류
    public float BaseValue;             // 1레벨 기본 수치
    public float GrowthPerLevel;        // 레벨당 증가 수치
}

/// <summary>
/// 플레이어 캐릭터의 기본 정보와 스탯 설정을 정의하는 ScriptableObject입니다.
/// Resources/_SO/Player 경로에 있는 에셋을 CDataManager가 자동 로드합니다.
/// CDataManager.GetPlayerData(id)를 통해서만 접근합니다.
/// </summary>
[CreateAssetMenu(menuName = "IdleTillDawn/Data/PlayerData", fileName = "PlayerDataSO_")]
public class CPlayerDataSO : CBaseDataSO
{
    #region InspectorVariables

    [Header("기본 정보")]
    [SerializeField] private string _characterName = "Character Name";      // 캐릭터 이름
    [SerializeField] private string _description = "Character Description"; // 캐릭터 설명
    [SerializeField] private Sprite _characterPortrait = null;               // 캐릭터 초상화

    [Header("캐릭터 스탯 설정")]
    [SerializeField] private PlayerStatInfo[] _statSettings; // EPlayerStatType별 기본값·성장치 배열

    [Header("소환 오브젝트")]
    [SerializeField] private GameObject _prefab; // 플레이어 프리팹

    [Header("사운드")]
    [SerializeField] private CSoundData _hitSFX; // 플레이어 피격 시 재생 사운드

    #endregion

    #region Properties

    public string     CharacterName     => _characterName;
    public string     Description       => _description;
    public Sprite     CharacterPortrait => _characterPortrait;
    public GameObject Prefab            => _prefab;

    /// <summary>플레이어 피격 시 재생할 사운드 데이터. null이면 무음</summary>
    public CSoundData HitSFX            => _hitSFX;

    #endregion

    #region PublicMethods

    /// <summary>
    /// 특정 스탯 타입의 PlayerStatInfo를 반환합니다.
    /// _statSettings에 해당 타입이 없으면 기본값(0) 구조체를 반환합니다.
    /// </summary>
    public PlayerStatInfo GetStatInfo(EPlayerStatType type)
    {
        if (_statSettings == null) return new PlayerStatInfo();

        for (int i = 0; i < _statSettings.Length; i++)
        {
            if (_statSettings[i].StatType == type) return _statSettings[i];
        }

        return new PlayerStatInfo();
    }

    /// <summary>레벨을 반영한 특정 스탯의 최종 수치를 반환합니다.</summary>
    public float GetStatAtLevel(EPlayerStatType type, int level)
    {
        PlayerStatInfo info = GetStatInfo(type);
        return info.BaseValue + info.GrowthPerLevel * (level - 1);
    }

    #endregion
}
