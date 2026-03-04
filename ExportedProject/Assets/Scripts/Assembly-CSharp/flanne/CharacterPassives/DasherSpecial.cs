using System.Collections;
using UnityEngine;
using flanne.Core;

namespace flanne.CharacterPassives
{
	public class DasherSpecial : MonoBehaviour
	{
		[SerializeField]
		private HarmfulOnContact hitboxPrefab;

		[SerializeField]
		private GameObject transformToDeerAnimPrefab;

		[SerializeField]
		private GameObject transformToHumanAnimPrefab;

		[SerializeField]
		private int baseDamage = 100;

		[SerializeField]
		private int baseKnockback = 10;

		[SerializeField]
		private float baseDuration = 10f;

		[SerializeField]
		private float warningDuration = 2f;

		[SerializeField]
		private float timeToActivate = 10f;

		[SerializeField]
		private float moveSpeedMultiplier = 1.75f;

		[SerializeField]
		private SoundEffectSO transformToDeerSFX;

		[SerializeField]
		private SoundEffectSO transformToHumanSFX;

		[SerializeField]
		private SoundEffectSO transformWarningSFX;

		[Header("Projectile")]
		public int projectiles;

		[SerializeField]
		private Projectile projectilePrefab;

		[SerializeField]
		private Transform firepoint;

		[SerializeField]
		private float shootCD;

		[SerializeField]
		private float projDamageMultiplier;

		private PlayerController player;

		private PlayerFlasher flasher;

		private SpriteTrail spriteTrail;

		private HarmfulOnContact hitbox;

		private Knockback knockback;

		private GameObject transformToDeerAnim;

		private GameObject transformToHumanAnim;

		private ObjectPooler OP;

		private PauseController PC;

		private bool _isTransformed;

		private float _transformTimer;

		private float _shootTimer;

		public int finalDamage => Mathf.FloorToInt(player.stats[StatType.SummonDamage].Modify(player.stats[StatType.MoveSpeed].Modify(baseDamage)));

		public int finalKnockback => Mathf.Max(baseKnockback, Mathf.FloorToInt(player.stats[StatType.CharacterSize].Modify(baseKnockback)));

		private void Start()
		{
			player = PlayerController.Instance;
			flasher = player.GetComponentInChildren<PlayerFlasher>();
			spriteTrail = player.playerSprite.GetComponentInChildren<SpriteTrail>();
			hitbox = Object.Instantiate(hitboxPrefab);
			SetParentToPlayerSprite(hitbox.transform);
			hitbox.gameObject.SetActive(value: false);
			knockback = hitbox.GetComponent<Knockback>();
			transformToDeerAnim = Object.Instantiate(transformToDeerAnimPrefab);
			SetParentToPlayerSprite(transformToDeerAnim.transform);
			transformToDeerAnim.gameObject.SetActive(value: false);
			transformToHumanAnim = Object.Instantiate(transformToHumanAnimPrefab);
			SetParentToPlayerSprite(transformToHumanAnim.transform);
			transformToHumanAnim.gameObject.SetActive(value: false);
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(projectilePrefab.name, projectilePrefab.gameObject, 100);
			PC = PauseController.SharedInstance;
		}

		private void Update()
		{
			if (player.playerHealth.hp == 0)
			{
				return;
			}
			if (!_isTransformed)
			{
				_transformTimer += Time.deltaTime;
				if (_transformTimer > timeToActivate)
				{
					_transformTimer -= timeToActivate;
					StartCoroutine(TransformCR());
				}
				return;
			}
			player.playerSprite.transform.position += moveSpeedMultiplier * player.moveVec * player.finalMoveSpeed * Time.deltaTime;
			Vector3 position = player.playerSprite.transform.position;
			player.playerSprite.transform.localPosition = Vector3.zero;
			player.transform.position = position;
			_shootTimer += Time.deltaTime;
			if (_shootTimer >= shootCD)
			{
				_shootTimer -= shootCD;
				for (int i = 0; i < projectiles; i++)
				{
					Shoot();
				}
			}
		}

		private IEnumerator TransformCR()
		{
			PC.Pause(0.2f);
			_isTransformed = true;
			player.gun.SetVisible(visible: false);
			hitbox.gameObject.SetActive(value: true);
			hitbox.damageAmount = finalDamage;
			knockback.knockbackForce = finalKnockback;
			transformToDeerAnim.SetActive(value: true);
			player.playerHealth.isInvincible.Flip();
			player.disableAction.Flip();
			player.disableMove.Flip();
			player.disableAnimation.Flip();
			transformToDeerSFX.Play();
			yield return new WaitForSeconds(0.2f);
			player.playerAnimator.ResetTrigger("Idle");
			player.playerAnimator.ResetTrigger("Run");
			player.playerAnimator.ResetTrigger("Walk");
			player.playerAnimator.SetTrigger("Special");
			spriteTrail.SetEnabled(enabled: true);
			yield return new WaitForSeconds(baseDuration - warningDuration);
			flasher.Flash();
			transformWarningSFX.Play();
			yield return new WaitForSeconds(warningDuration);
			flasher.StopFlash();
			player.gun.SetVisible(visible: true);
			hitbox.gameObject.SetActive(value: false);
			transformToHumanAnim.SetActive(value: true);
			player.disableAction.UnFlip();
			player.disableMove.UnFlip();
			player.disableAnimation.UnFlip();
			player.playerAnimator.ResetTrigger("Special");
			player.playerAnimator.SetTrigger("Idle");
			spriteTrail.SetEnabled(enabled: false);
			transformToHumanSFX.Play();
			PC.Pause(0.2f);
			_isTransformed = false;
			yield return new WaitForSeconds(0.5f);
			player.playerHealth.isInvincible.UnFlip();
		}

		private void Shoot()
		{
			GameObject pooledObject = OP.GetPooledObject(projectilePrefab.name);
			pooledObject.transform.position = firepoint.position;
			pooledObject.SetActive(value: true);
			pooledObject.GetComponent<Projectile>().damage = (float)finalDamage * projDamageMultiplier;
		}

		private void SetParentToPlayerSprite(Transform t)
		{
			t.SetParent(player.playerSprite.transform);
			t.localPosition = Vector3.zero;
			t.localScale = Vector3.one;
		}
	}
}
