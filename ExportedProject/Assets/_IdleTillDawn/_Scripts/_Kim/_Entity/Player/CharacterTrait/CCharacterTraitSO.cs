using UnityEngine;

public abstract class CCharacterTraitSO : ScriptableObject
{
	#region 인스펙터
	[Header("고유 특성 정보")]
	[SerializeField] private string _traitName; 
	[SerializeField][TextArea] private string _description; 
	[SerializeField] private Sprite _icon;
	[SerializeField] private float _damage;
	#endregion

	#region 프로퍼티
	public string TraitName => _traitName;
	public float Damage => _damage;
	#endregion

	public abstract void ApplyTrait(CPlayerController player);
}
