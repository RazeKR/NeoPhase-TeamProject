using UnityEngine;

namespace flanne
{
	public class Summon : MonoBehaviour
	{
		public static string TweakSummonDamageNotification = "Summon.TweakSummonDamageNotification";

		public static string SummonOnHitNotification = "Summon.SummonOnHitNotification";

		public string SummonTypeID;

		[SerializeField]
		private bool dontParent;

		protected PlayerController player;

		private StatsHolder stats;

		public StatMod summonDamageMod => stats[StatType.SummonDamage];

		public StatMod summonAtkSpdMod => stats[StatType.SummonAttackSpeed];

		private void Start()
		{
			player = PlayerController.Instance;
			stats = player.stats;
			if (dontParent)
			{
				base.transform.SetParent(null);
			}
			Init();
		}

		protected virtual void Init()
		{
		}

		protected int ApplyDamageMods(int damage)
		{
			return damage.NotifyModifiers(TweakSummonDamageNotification, this, SummonTypeID);
		}
	}
}
