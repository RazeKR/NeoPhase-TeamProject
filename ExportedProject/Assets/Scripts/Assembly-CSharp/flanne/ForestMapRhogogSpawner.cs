using UnityEngine;
using UnityEngine.UI;
using flanne.Core;
using flanne.UI;

namespace flanne
{
	public class ForestMapRhogogSpawner : MonoBehaviour
	{
		[SerializeField]
		private GameObject rhogogPrefab;

		[SerializeField]
		private GameObject arenaMonsterPrefab;

		[SerializeField]
		private GameObject walkingTreePrefab;

		[SerializeField]
		private Panel sporeGunUnlockedUI;

		[SerializeField]
		private Button sporeGunUIConfirm;

		private bool treesActivated;

		private Health[] treeHealths;

		private GameTimer timer;

		private void Start()
		{
			timer = GameTimer.SharedInstance;
			this.AddObserver(OnOneSecondLeft, GameTimer.OneSecondLeftNotification);
			treeHealths = GetComponentsInChildren<Health>();
			Health[] array = treeHealths;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].onDeath.AddListener(OnTreeKilled);
			}
		}

		private void OnTreeKilled()
		{
			if (treesActivated)
			{
				return;
			}
			treesActivated = true;
			Health[] array = treeHealths;
			foreach (Health health in array)
			{
				if (health.gameObject.activeSelf)
				{
					health.gameObject.SetActive(value: false);
					Object.Instantiate(walkingTreePrefab).transform.position = health.transform.position;
				}
			}
		}

		private void OnOneSecondLeft(object sender, object args)
		{
			if (treesActivated && !SelectedMap.MapData.endless)
			{
				timer.Stop();
				GameObject obj = Object.Instantiate(rhogogPrefab);
				Vector3 position = PlayerController.Instance.transform.position + new Vector3(20f, 0f, 0f);
				obj.transform.position = position;
				obj.GetComponent<Health>().onDeath.AddListener(OnRhogogDefeated);
			}
		}

		private void OnRhogogDefeated()
		{
			if (!SaveSystem.data.gunUnlocks.unlocks[8])
			{
				sporeGunUnlockedUI.Show();
				sporeGunUIConfirm.onClick.AddListener(OnSporeGunUIConfirm);
				PauseController.SharedInstance.Pause();
				SaveSystem.data.gunUnlocks.unlocks[8] = true;
			}
			else
			{
				timer.Start();
			}
		}

		private void OnSporeGunUIConfirm()
		{
			sporeGunUnlockedUI.Hide();
			PauseController.SharedInstance.UnPause();
			timer.Start();
		}
	}
}
