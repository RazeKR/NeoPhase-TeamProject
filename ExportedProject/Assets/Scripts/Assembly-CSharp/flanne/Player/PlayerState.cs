using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace flanne.Player
{
	public abstract class PlayerState : State
	{
		protected PlayerController owner;

		protected PlayerInput playerInput => owner.playerInput;

		protected SpriteRenderer playerSprite => owner.playerSprite;

		protected Animator playerAnimator => owner.playerAnimator;

		protected StatsHolder stats => owner.stats;

		protected Gun gun => owner.gun;

		protected Ammo ammo => owner.ammo;

		protected Slider reloadBar => owner.reloadBar;

		private void Awake()
		{
			owner = GetComponentInParent<PlayerController>();
		}
	}
}
