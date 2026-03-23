using UnityEngine;

[CreateAssetMenu(menuName = "2D(SO)/Data/Player Data", fileName = "PlayerDataSO_")]
public class CPlayerDataSO : ScriptableObject
{
	#region 인스펙터
	[Header("기본 정보")]
	[SerializeField] private string _characterName = "Character Name";
	[SerializeField] private string _description = "Character Description";
	[SerializeField] private Sprite _characterPortrait = null;

	[Header("캐릭터 스탯")]
	[SerializeField] private float _baseHealth = 50f;
	[SerializeField] private float _baseMana = 20f;
	[SerializeField] private float _baseDamage = 10f;
	[SerializeField] private float _baseAttackSpeed = 1f;
	[SerializeField] private float _baseHealthRegen = 1f;
	[SerializeField] private float _baseManaRegen = 4f;
	[SerializeField] private float _baseMoveSpeed = 3f;
	[SerializeField] private float _baseExpMultiplier = 1f;

	[Header("소환할 오브젝트")]
	[SerializeField] GameObject _prefab;
	#endregion

	#region 내부 변수
	public string CharacterName => _characterName;
	public string Description => _description;
	public Sprite CharacterPortrait => _characterPortrait;

	public float BaseHealth => _baseHealth;
	public float BaseMana => _baseMana;
	public float BaseDamage => _baseDamage;
	public float BaseAttackSpeed => _baseAttackSpeed;
	public float BaseHealthRegen => _baseHealthRegen;
	public float BaseManaRegen => _baseManaRegen;
	public float BaseMoveSpeed => _baseMoveSpeed;
	public float BaseExpMultiplier => _baseExpMultiplier;

	public GameObject Prefab => _prefab;
	#endregion
}
