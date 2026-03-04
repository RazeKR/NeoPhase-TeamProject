using System;
using System.Collections.Generic;
using UnityEngine;

namespace flanne.PerkSystem
{
	public class PlayerPerks : MonoBehaviour
	{
		[SerializeField]
		private PlayerController player;

		private List<Powerup> _equippedPerks;

		private void Start()
		{
			_equippedPerks = new List<Powerup>();
		}

		public void Equip(Powerup perk)
		{
			if (!_equippedPerks.Contains(perk))
			{
				Powerup powerup = perk.Copy();
				powerup.Apply(player);
				_equippedPerks.Add(powerup);
			}
			else
			{
				perk.Copy().ApplyStack(player);
			}
		}

		public void Equip(GunEvolution gunEvo)
		{
			gunEvo.Copy().Apply(player);
		}
	}
}
