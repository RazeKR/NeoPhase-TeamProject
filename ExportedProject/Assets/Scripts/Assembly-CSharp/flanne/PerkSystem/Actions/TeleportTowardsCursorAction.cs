using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class TeleportTowardsCursorAction : Action
	{
		[SerializeField]
		private float distance;

		private PlayerController player;

		private ShootingCursor SC;

		public override void Init()
		{
			player = PlayerController.Instance;
			SC = ShootingCursor.Instance;
		}

		public override void Activate(GameObject target)
		{
			Vector2 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
			Vector2 vector2 = player.transform.position;
			Vector2 vector3 = vector - vector2;
			Vector2 position = vector2 + vector3 * distance;
			player.SetPosition(position);
		}
	}
}
