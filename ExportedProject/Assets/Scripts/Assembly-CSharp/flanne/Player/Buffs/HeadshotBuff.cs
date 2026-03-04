using UnityEngine;

namespace flanne.Player.Buffs
{
	public class HeadshotBuff : Buff
	{
		[SerializeField]
		[Range(0f, 1f)]
		private float chance;

		[SerializeField]
		private float damageMultplier = 1f;

		[SerializeField]
		private float sizeMultiplier = 1f;

		[SerializeField]
		private float speedMultiplier = 1f;

		private PlayerController player;

		public override void OnAttach()
		{
			player = PlayerController.Instance;
			this.AddObserver(OnShoot, Gun.ShootEvent);
		}

		public override void OnUnattach()
		{
			this.RemoveObserver(OnShoot, Gun.ShootEvent);
		}

		private void OnShoot(object sender, object args)
		{
			float baseValue = player.stats[StatType.ProjectileSpeed].Modify(chance);
			baseValue = player.stats[StatType.MoveSpeed].Modify(baseValue);
			if (Random.Range(0f, 1f) < baseValue)
			{
				ProjectileRecipe obj = args as ProjectileRecipe;
				obj.damage *= damageMultplier;
				obj.size *= sizeMultiplier;
				obj.projectileSpeed *= speedMultiplier;
			}
		}
	}
}
