using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "IdleTillDawn/Trait/PlayerTrait/Dasher", fileName = "Trait_Deer")]
public class CTraitDeerSO : CCharacterTraitSO
{
	#region 인스펙터
	[Header("사슴 변신 설정")]
	[SerializeField] private float _transformInterval = 10f;
	[SerializeField] private float _duration = 3f;
	[SerializeField] private float _speedMultiplier = 1.5f;
    [SerializeField] private RuntimeAnimatorController _deerAnimator;

    [Header("애니메이션 설정")]
    [SerializeField] private float _transformToDeerDuration = 0.5f;
    [SerializeField] private float _transformToHumanDuration = 0.5f;
    [SerializeField] private string _transformToHumanTrigger = "tDurationOver";

    [Header("변신 해제 설정")]
    [SerializeField] private CSoundData _castOffSFX;
    [SerializeField] private float _knockbackRadius = 3f;
    [SerializeField] private float _knockbackForce = 10f;
    [SerializeField] private float _knockbackDuration = 0.3f;
    #endregion

    public override void ApplyTrait(CPlayerController player)
    {
        player.StartCoroutine(CoDeerTransformation(player));
    }

    private IEnumerator CoDeerTransformation(CPlayerController player)
    {
        RuntimeAnimatorController originAnimator = null;
        Animator playerAnimator = null;

        while (true)
        {
            yield return new WaitForSeconds(_transformInterval);

            PlaySFX(CastSFX, player);

            if (originAnimator == null)
            {
                originAnimator = player.CurrentAnimator;
                playerAnimator = player.GetComponentInChildren<Animator>();

                Debug.Log($"originAnimator : {originAnimator.name}");
            }

            player.IsWeaponDisabled = true;
            player.IsAutoEvadeDisabled = true;
            player.AddStatus(EStatusEffect.Invincible);

            player.SetTraitSpeedMultiplier(0f);
            player.Rb.velocity = Vector2.zero;

            if (_deerAnimator != null)
            {
                player.ChangeAnimator(_deerAnimator);
                yield return new WaitForSeconds(_transformToDeerDuration);
            }

            player.SetTraitSpeedMultiplier(_speedMultiplier);

            GameObject ramObj = new GameObject("DeerRamHitbox");
            ramObj.transform.SetParent(player.transform);
            ramObj.transform.localPosition = Vector3.zero;

            Rigidbody2D ramRb = ramObj.AddComponent<Rigidbody2D>();
            ramRb.isKinematic = true;

            CDeerRamAttack ramAttack = ramObj.gameObject.AddComponent<CDeerRamAttack>();
            ramAttack.Damage = Damage;
            ramAttack.EnemyLayer = player.TargetLayer;

            yield return new WaitForSeconds(_duration);

            if (ramObj != null)
            {
                Destroy(ramObj);
            }

            player.SetTraitSpeedMultiplier(0f);
            player.Rb.velocity = Vector2.zero;

            Collider2D[] targets = Physics2D.OverlapCircleAll(player.transform.position, _knockbackRadius, player.TargetLayer);
            foreach (Collider2D col in targets)
            {
                CEntityBase entity = col.GetComponentInParent<CEntityBase>();
                if (entity != null)
                {
                    Vector2 dir = (col.transform.position - player.transform.position).normalized;
                    entity.ApplyKnockback(dir * _knockbackForce, _knockbackDuration);
                }
            }

            if (_deerAnimator != null && playerAnimator != null)
            {
                Debug.Log("사람으로 변신");
                playerAnimator.SetTrigger(_transformToHumanTrigger);
                PlaySFX(_castOffSFX, player);
                yield return new WaitForSeconds(_transformToHumanDuration);
            }

            player.SetTraitSpeedMultiplier(1.0f);
            player.IsWeaponDisabled = false;
            player.IsAutoEvadeDisabled = false;
            player.RemoveStatus(EStatusEffect.Invincible);


            if (originAnimator != null)
            {
                player.ChangeAnimator(originAnimator);
            }
        }
    }
}
