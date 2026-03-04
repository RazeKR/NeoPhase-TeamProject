using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace flanne.PerkSystem.Actions
{
	public class TargetSummonsAction : Action
	{
		[SerializeReference]
		private Action action;

		public override void Init()
		{
			action.Init();
		}

		public override void Activate(GameObject target)
		{
			List<Summon> list = FindObjectsOfTypeAll<Summon>();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].gameObject.activeInHierarchy)
				{
					action.Activate(list[i].gameObject);
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
