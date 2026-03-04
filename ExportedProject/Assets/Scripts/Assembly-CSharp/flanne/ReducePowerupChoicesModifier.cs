using UnityEngine;
using flanne.Core;

namespace flanne
{
	[CreateAssetMenu(fileName = "ReducePowerupChoicesModifier", menuName = "DifficultyMods/ReducePowerupChoicesModifier")]
	public class ReducePowerupChoicesModifier : DifficultyModifier
	{
		[SerializeField]
		private int amountToReduce;

		public override void ModifyGame(GameController gameController)
		{
			gameController.numPowerupChoices -= amountToReduce;
		}
	}
}
