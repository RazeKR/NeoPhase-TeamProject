using UnityEngine;

namespace flanne
{
	public class AutoKillBelowHP : MonoBehaviour
	{
		[Range(0f, 1f)]
		public float autoKillPercent;

		private void Start()
		{
			this.AddObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnImpact(object sender, object args)
		{
			if (!((sender as MonoBehaviour).gameObject.tag == "Bullet"))
			{
				return;
			}
			GameObject gameObject = args as GameObject;
			if (gameObject.tag.Contains("Enemy"))
			{
				Health component = gameObject.GetComponent<Health>();
				if ((float)(component?.HP).Value / (float)(component?.maxHP).Value <= autoKillPercent && component.HP != 0)
				{
					component.AutoKill();
				}
			}
		}
	}
}
