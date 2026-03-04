using System.Collections.Generic;
using UnityEngine;

namespace flanne.Player
{
	public class PlayerBuffs : MonoBehaviour
	{
		private List<Buff> buffs;

		private void Start()
		{
			buffs = new List<Buff>();
		}

		public void Add(Buff buff)
		{
			buffs.Add(buff);
			buff.owner = this;
			buff.OnAttach();
		}

		public void Remove(Buff buff)
		{
			buffs.Remove(buff);
			buff.OnUnattach();
		}

		private void OnDestroy()
		{
			foreach (Buff buff in buffs)
			{
				buff.OnUnattach();
			}
		}
	}
}
