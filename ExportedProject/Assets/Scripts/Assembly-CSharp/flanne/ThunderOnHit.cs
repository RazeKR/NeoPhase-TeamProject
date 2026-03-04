using UnityEngine;

namespace flanne
{
	public class ThunderOnHit : MonoBehaviour
	{
		[Range(0f, 1f)]
		public float chanceToHit;

		public int baseDamage;

		private ThunderGenerator TGen;

		private void Start()
		{
			this.AddObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
			TGen = ThunderGenerator.SharedInstance;
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnImpact, Projectile.ImpactEvent, PlayerController.Instance.gameObject);
		}

		private void OnImpact(object sender, object args)
		{
			if (Random.Range(0f, 1f) < chanceToHit && (args as GameObject).tag.Contains("Enemy"))
			{
				TGen.GenerateAt(args as GameObject, baseDamage);
			}
		}
	}
}
