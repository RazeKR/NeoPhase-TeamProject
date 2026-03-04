using UnityEngine;

namespace flanne.PowerupSystem
{
	public class ThunderDuringHolyShield : MonoBehaviour
	{
		[SerializeField]
		private int baseDamage;

		[SerializeField]
		private float cooldown;

		[SerializeField]
		private int thundersPerWave;

		[SerializeField]
		private Vector2 range;

		private ThunderGenerator TGen;

		private Transform playerTransform;

		private PreventDamage holyShield;

		private float _timer;

		private void Start()
		{
			TGen = ThunderGenerator.SharedInstance;
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			playerTransform = componentInParent.transform;
			holyShield = componentInParent.GetComponentInChildren<PreventDamage>();
		}

		private void Update()
		{
			if (holyShield.isActive)
			{
				_timer += Time.deltaTime;
			}
			if (!(_timer > cooldown))
			{
				return;
			}
			_timer -= cooldown;
			for (int i = 0; i < thundersPerWave; i++)
			{
				GameObject randomEnemy = EnemyFinder.GetRandomEnemy(playerTransform.position, range);
				if (randomEnemy != null)
				{
					TGen.GenerateAt(randomEnemy, baseDamage);
				}
			}
		}
	}
}
