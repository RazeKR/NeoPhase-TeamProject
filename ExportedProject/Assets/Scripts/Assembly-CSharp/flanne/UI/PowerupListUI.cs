using UnityEngine;

namespace flanne.UI
{
	public class PowerupListUI : MonoBehaviour
	{
		[SerializeField]
		private GameObject powerupIconPrefab;

		private void OnPowerupApplied(object sender, object args)
		{
			GameObject obj = Object.Instantiate(powerupIconPrefab);
			obj.transform.SetParent(base.transform);
			obj.transform.localScale = Vector3.one;
			Powerup data = sender as Powerup;
			obj.GetComponent<PowerupIcon>().data = data;
		}

		private void Start()
		{
			this.AddObserver(OnPowerupApplied, Powerup.AppliedNotifcation);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnPowerupApplied, Powerup.AppliedNotifcation);
		}
	}
}
