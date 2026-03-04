using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class PlaySoundAction : Action
	{
		[SerializeField]
		private SoundEffectSO soundFX;

		public override void Activate(GameObject target)
		{
			soundFX.Play();
		}
	}
}
