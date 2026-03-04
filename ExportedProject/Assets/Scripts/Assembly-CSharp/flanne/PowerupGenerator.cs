using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class PowerupGenerator : MonoBehaviour
	{
		public static PowerupGenerator Instance;

		public static bool CanReroll;

		[SerializeField]
		private PowerupPoolProfile profile;

		[SerializeField]
		private PowerupPoolProfile devilDealProfile;

		private List<PowerupPoolItem> powerupPool;

		private List<PowerupPoolItem> devilPool;

		private List<PowerupPoolItem> characterPool;

		private List<Powerup> takenPowerups;

		private int _defaultNumTimesRepeatable;

		private void Awake()
		{
			CanReroll = false;
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}

		public void InitPowerupPool(int numTimesRepeatable)
		{
			powerupPool = GetPowerupPool(profile.powerupPool, numTimesRepeatable);
			devilPool = GetPowerupPool(devilDealProfile.powerupPool, -1);
			_defaultNumTimesRepeatable = numTimesRepeatable;
			takenPowerups = new List<Powerup>();
		}

		public void SetCharacterPowerupPool(PowerupPoolProfile characterExclusiveProfile)
		{
			characterPool = GetPowerupPool(characterExclusiveProfile.powerupPool, _defaultNumTimesRepeatable);
		}

		public List<Powerup> GetRandomCharacterProfile(int num = 1)
		{
			if (characterPool == null || characterPool.Count < num)
			{
				return GetRandom(num, powerupPool);
			}
			return GetRandom(num, characterPool);
		}

		public List<Powerup> GetRandomDevilProfile(int num)
		{
			return GetRandom(num, devilPool);
		}

		public List<Powerup> GetRandom(int num)
		{
			return GetRandom(num, powerupPool);
		}

		public List<Powerup> GetRandom(int num, List<PowerupPoolItem> pool)
		{
			List<Powerup> list = new List<Powerup>();
			for (int i = 0; i < num; i++)
			{
				PowerupPoolItem powerupPoolItem = null;
				while (powerupPoolItem == null)
				{
					PowerupPoolItem powerupPoolItem2 = pool[Random.Range(0, pool.Count)];
					if (!list.Contains(powerupPoolItem2.powerup) && PrereqsMet(powerupPoolItem2.powerup))
					{
						powerupPoolItem = powerupPoolItem2;
					}
				}
				list.Add(powerupPoolItem.powerup);
			}
			return list;
		}

		public void AddToPool(List<Powerup> powerups, int amount)
		{
			powerupPool.AddRange(GetPowerupPool(powerups, amount));
		}

		public void RemoveFromCharacterPool(Powerup powerup)
		{
			PowerupPoolItem powerupPoolItem = characterPool.Find((PowerupPoolItem x) => x.powerup == powerup);
			if (powerupPoolItem != null)
			{
				powerupPoolItem.numTimeRepeatable--;
				if (powerupPoolItem.numTimeRepeatable == 0)
				{
					characterPool.Remove(powerupPoolItem);
				}
			}
		}

		public void RemoveFromDevilPool(Powerup powerup)
		{
			PowerupPoolItem powerupPoolItem = devilPool.Find((PowerupPoolItem x) => x.powerup == powerup);
			if (powerupPoolItem != null)
			{
				powerupPoolItem.numTimeRepeatable--;
				if (powerupPoolItem.numTimeRepeatable == 0)
				{
					devilPool.Remove(powerupPoolItem);
				}
			}
		}

		public void RemoveFromPool(Powerup powerup)
		{
			PowerupPoolItem powerupPoolItem = powerupPool.Find((PowerupPoolItem x) => x.powerup == powerup);
			if (powerupPoolItem != null)
			{
				powerupPoolItem.numTimeRepeatable--;
				if (powerupPoolItem.numTimeRepeatable == 0)
				{
					powerupPool.Remove(powerupPoolItem);
				}
				takenPowerups.Add(powerup);
			}
		}

		private bool PrereqsMet(Powerup powerup)
		{
			if (powerup.anyPrereqFulfill)
			{
				foreach (Powerup prereq in powerup.prereqs)
				{
					if (takenPowerups.Contains(prereq))
					{
						return true;
					}
				}
				return false;
			}
			foreach (Powerup prereq2 in powerup.prereqs)
			{
				if (!takenPowerups.Contains(prereq2))
				{
					return false;
				}
			}
			return true;
		}

		private List<PowerupPoolItem> GetPowerupPool(List<Powerup> powerups, int numTimesRepeatable)
		{
			List<PowerupPoolItem> list = new List<PowerupPoolItem>();
			foreach (Powerup powerup in powerups)
			{
				PowerupPoolItem powerupPoolItem = new PowerupPoolItem();
				powerupPoolItem.powerup = powerup;
				powerupPoolItem.numTimeRepeatable = numTimesRepeatable;
				list.Add(powerupPoolItem);
			}
			return list;
		}
	}
}
