using UnityEngine;
using flanne.UIExtensions;

namespace flanne.UI
{
	public class CharacterIconUI : DataUI<CharacterData>
	{
		[SerializeField]
		private Animator animator;

		protected override void SetProperties()
		{
			animator.runtimeAnimatorController = base.data.uiAnimController;
		}
	}
}
