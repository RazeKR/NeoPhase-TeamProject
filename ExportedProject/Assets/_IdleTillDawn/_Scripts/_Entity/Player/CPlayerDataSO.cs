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
	[SerializeField] private float _maxHealth = 50f;
	[SerializeField] private float _maxMana = 20f;
	[SerializeField] private float _defaultDamage = 10f;
	[SerializeField] private float _attackSpeed = 1.5f;
	[SerializeField] private float _healthRegen = 1f;
	[SerializeField] private float _manaRegen = 4f;
	[SerializeField] private float _moveSpeed = 3f;
	[SerializeField] private float _expMultiplier = 1f;

	[Header("소환할 오브젝트")]
	[SerializeField] GameObject _prefab;
	#endregion

	#region 내부 변수
	public string CharacterName => _characterName;
	public string Description => _description;
	public Sprite CharacterPortrait => _characterPortrait;

	public float MaxHealth => _maxHealth;
	public float MaxMana => _maxMana;
	public float DefaultDamage => _defaultDamage;
	public float AttackSpeed => _attackSpeed;
	public float HealthRegen => _healthRegen;
	public float ManaRegen => _manaRegen;
	public float MoveSpeed => _moveSpeed;
	public float ExpMultiplier => _expMultiplier;

	public GameObject Prefab => _prefab;
	#endregion
}
