using UnityEngine;

namespace flanne
{
	public class PerkTest : MonoBehaviour
	{
		[SerializeField]
		private PlayerController player;

		[SerializeField]
		private Powerup perk;

		private void AddPerk()
		{
			perk.Apply(player);
		}

		private void SerializePerk()
		{
			Debug.Log(JsonUtility.ToJson(perk));
		}
	}
}
