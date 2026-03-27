using UnityEngine;

public enum EPlayerStatType
{
    Health,
    Mana,
    Damage,
    AttackSpeed,
    HealthRegen,
    ManaRegen,
    MoveSpeed,
    ExpMultiplier,
    Count // 배열 크기용
}

[System.Serializable]
public struct PlayerStatInfo
{
    public EPlayerStatType StatType;
    public float BaseValue;      // 기본 1레벨 수치
    public float GrowthPerLevel; // 레벨당 오르는 수치
}

[CreateAssetMenu(menuName = "2D(SO)/Data/Player Data", fileName = "PlayerDataSO_")]
public class CPlayerDataSO : ScriptableObject
{
	#region 인스펙터
	[Header("기본 정보")]
	[SerializeField] private string _characterName = "Character Name";
	[SerializeField] private string _description = "Character Description";
	[SerializeField] private Sprite _characterPortrait = null;

    [Header("캐릭터 스탯 설정")]
    [SerializeField] private PlayerStatInfo[] _statSettings;

    [Header("소환할 오브젝트")]
	[SerializeField] GameObject _prefab;
	#endregion

	#region 내부 변수
	public string CharacterName => _characterName;
	public string Description => _description;
	public Sprite CharacterPortrait => _characterPortrait;

    public GameObject Prefab => _prefab;
    #endregion

    public PlayerStatInfo GetStatInfo(EPlayerStatType type)
    {
        if (_statSettings == null) return new PlayerStatInfo();

        for (int i = 0; i < _statSettings.Length; i++)
        {
            if (_statSettings[i].StatType == type) return _statSettings[i];
        }

        return new PlayerStatInfo();
    }
}
