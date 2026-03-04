using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class SmiteHPDamageScaling : MonoBehaviour
	{
		private PlayerHealth playerHealth;

		private void OnTweakDamage(object sender, object args)
		{
			List<ValueModifier> obj = args as List<ValueModifier>;
			int num = playerHealth.hp * 10;
			obj.Add(new AddValueModifier(0, num));
		}

		private void Start()
		{
			PlayerController componentInParent = GetComponentInParent<PlayerController>();
			playerHealth = componentInParent.playerHealth;
			this.AddObserver(OnTweakDamage, SmitePassive.SmiteTweakDamageNotification);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnTweakDamage, SmitePassive.SmiteTweakDamageNotification);
		}
	}
}
