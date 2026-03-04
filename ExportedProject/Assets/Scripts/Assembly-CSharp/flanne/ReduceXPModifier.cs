using UnityEngine;
using flanne.Core;

namespace flanne
{
	[CreateAssetMenu(fileName = "ReduceXPModifier", menuName = "DifficultyMods/ReduceXPModifier")]
	public class ReduceXPModifier : DifficultyModifier
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float xpReduction;

		public override void ModifyGame(GameController gameController)
		{
			gameController.playerXP.xpMultiplier.AddMultiplierReduction(1f - xpReduction);
		}
	}
}
