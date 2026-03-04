using UnityEngine;

namespace flanne
{
	public class SetGameObjectActiveSummon : AttackingSummon
	{
		[SerializeField]
		private GameObject attackObj;

		protected override bool Attack()
		{
			attackObj.SetActive(value: true);
			return true;
		}
	}
}
