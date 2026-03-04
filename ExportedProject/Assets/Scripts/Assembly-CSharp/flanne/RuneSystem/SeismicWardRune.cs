using UnityEngine;

namespace flanne.RuneSystem
{
	public class SeismicWardRune : Rune
	{
		[SerializeField]
		private GameObject seismicWardPrefab;

		[SerializeField]
		private int cdrPerLevel;

		protected override void Init()
		{
			AttackingSummon attackingSummon = AttachSeismicWard(new Vector3(0f, 1f, 0f));
			AttackingSummon attackingSummon2 = AttachSeismicWard(new Vector3(0f, -1f, 0f));
			attackingSummon.attackSpeedMod.AddFlatBonus(cdrPerLevel * level);
			attackingSummon2.attackSpeedMod.AddFlatBonus(cdrPerLevel * level);
		}

		private AttackingSummon AttachSeismicWard(Vector3 localPosition)
		{
			GameObject obj = Object.Instantiate(seismicWardPrefab);
			obj.transform.SetParent(player.transform);
			obj.transform.localPosition = localPosition;
			obj.SetActive(value: true);
			return obj.GetComponent<AttackingSummon>();
		}
	}
}
