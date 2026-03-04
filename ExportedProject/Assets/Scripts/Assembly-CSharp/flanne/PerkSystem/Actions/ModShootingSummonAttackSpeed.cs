using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace flanne.PerkSystem.Actions
{
	public class ModShootingSummonAttackSpeed : Action
	{
		[SerializeField]
		private string SummonTypeID;

		[SerializeField]
		private float attackSpeedMod;

		public override void Activate(GameObject target)
		{
			_ = PlayerController.Instance;
			foreach (AttackingSummon item in FindObjectsOfTypeAll<AttackingSummon>())
			{
				if (item.SummonTypeID == SummonTypeID)
				{
					item.attackSpeedMod.AddMultiplierBonus(attackSpeedMod);
				}
			}
		}

		private List<T> FindObjectsOfTypeAll<T>()
		{
			List<T> list = new List<T>();
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene sceneAt = SceneManager.GetSceneAt(i);
				if (sceneAt.isLoaded)
				{
					GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
					foreach (GameObject gameObject in rootGameObjects)
					{
						list.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive: true));
					}
				}
			}
			return list;
		}
	}
}
