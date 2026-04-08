using UnityEngine;

public abstract class CCharacterTraitSO : ScriptableObject
{
	#region 인스펙터
	[Header("고유 특성 정보")]
	[SerializeField] private string _traitName; 
	[SerializeField][TextArea] private string _description; 
	[SerializeField] private Sprite _icon;
	[SerializeField] private float _damage;

    [Header("사운드")]
    [SerializeField] private CSoundData _castSFX;
    #endregion

    #region 프로퍼티
    public string TraitName => _traitName;
	public float Damage => _damage;
	public CSoundData CastSFX => _castSFX;
	#endregion

	public abstract void ApplyTrait(CPlayerController player);

	protected virtual void PlaySFX(CSoundData sfx, CPlayerController player)
	{
		if (CAudioManager.Instance != null && player != null)
		{
			CAudioManager.Instance.Play(sfx, player.transform.position);
		}
	}

	protected virtual void PlaySFX(CSoundData sfx, Vector3 position)
	{
		if (CAudioManager.Instance != null)
		{
			CAudioManager.Instance.Play(sfx, position);
		}
	}
}
