using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "IdleTillDawn/Trait/PlayerTrait", fileName = "Trait_Deer")]
public class CTraitDeerSO : CCharacterTraitSO
{
	#region 인스펙터
	[Header("사슴 변신 설정")]
	[SerializeField] private float _transformInterval = 10f;
	[SerializeField] private float _duration = 3f;
	[SerializeField] private float _speedMultiplier = 1.5f;
    [SerializeField] private RuntimeAnimatorController _deerAnimator;
    #endregion

    public override void ApplyTrait(CPlayerController player)
    {
        player.StartCoroutine(CoDeerTransformation(player));
    }

    private IEnumerator CoDeerTransformation(CPlayerController player)
    {
        while (true)
        {
            yield return new WaitForSeconds(_transformInterval);

            player.IsWeaponDisabled = true;
            player.SetTraitSpeedMultiplier(_speedMultiplier);

            player.AddStatus(EStatusEffect.Invincible);

            player.IsAutoEvadeDisabled = true;

            CDeerRamAttack ramAttack = player.gameObject.AddComponent<CDeerRamAttack>();
            ramAttack.Damage = Damage;
            ramAttack.EnemyLayer = player.TargetLayer;

            RuntimeAnimatorController originAnimator = player.CurrentAnimator;

            if (_deerAnimator != null)
            {
                player.ChangeAnimator(_deerAnimator);
            }

            yield return new WaitForSeconds(_duration);

            player.SetTraitSpeedMultiplier(1.0f);
            player.IsWeaponDisabled = false;

            player.RemoveStatus(EStatusEffect.Invincible);

            player.IsAutoEvadeDisabled = false;

            if (ramAttack != null)
            {
                Destroy(ramAttack);
            }

            if (originAnimator != null)
            {
                player.ChangeAnimator(originAnimator);
            }
        }
    }
}
