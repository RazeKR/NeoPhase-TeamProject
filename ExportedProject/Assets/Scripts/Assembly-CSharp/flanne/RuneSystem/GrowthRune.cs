using System.Collections;
using UnityEngine;
using flanne.Player;

namespace flanne.RuneSystem
{
	public class GrowthRune : Rune
	{
		[SerializeField]
		private float xpMultiplierPerLevel;

		[SerializeField]
		private int levelPerHPDrop;

		[SerializeField]
		private GameObject hpPrefab;

		private ObjectPooler OP;

		private PlayerXP playerXP;

		private void OnLevelChanged(int level)
		{
			if (level % levelPerHPDrop == 0)
			{
				StartCoroutine(WaitToDropHP());
			}
		}

		protected override void Init()
		{
			OP = ObjectPooler.SharedInstance;
			OP.AddObject(hpPrefab.name, hpPrefab, 100);
			playerXP = player.GetComponentInChildren<PlayerXP>();
			playerXP.xpMultiplier.AddMultiplierBonus(xpMultiplierPerLevel * (float)level);
			playerXP.OnLevelChanged.AddListener(OnLevelChanged);
		}

		private void OnDestroy()
		{
			playerXP.OnLevelChanged.RemoveListener(OnLevelChanged);
		}

		private IEnumerator WaitToDropHP()
		{
			yield return new WaitForSeconds(0.1f);
			GameObject pooledObject = OP.GetPooledObject(hpPrefab.name);
			pooledObject.transform.position = base.transform.position;
			pooledObject.SetActive(value: true);
		}
	}
}
