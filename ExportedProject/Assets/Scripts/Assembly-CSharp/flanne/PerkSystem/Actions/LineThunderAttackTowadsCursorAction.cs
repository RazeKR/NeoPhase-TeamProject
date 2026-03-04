using System;
using System.Collections;
using UnityEngine;

namespace flanne.PerkSystem.Actions
{
	public class LineThunderAttackTowadsCursorAction : Action
	{
		[NonSerialized]
		private ThunderGenerator TG;

		[NonSerialized]
		private ShootingCursor SC;

		[NonSerialized]
		private PlayerController player;

		public override void Init()
		{
			TG = ThunderGenerator.SharedInstance;
			SC = ShootingCursor.Instance;
			player = PlayerController.Instance;
		}

		public override void Activate(GameObject target)
		{
			Vector2 normalized = GetDirectionToCursor().normalized;
			Vector2 startPos = (Vector2)player.transform.position + normalized * 0.5f;
			player.StartCoroutine(ThunderAttackCR(normalized, startPos));
		}

		private Vector2 GetDirectionToCursor()
		{
			Vector2 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
			Vector2 vector2 = player.transform.position;
			return vector - vector2;
		}

		private IEnumerator ThunderAttackCR(Vector2 direction, Vector2 startPos)
		{
			int numStrikes = 10;
			float distancePerStrike = 0.8f;
			for (int i = 0; i < numStrikes; i++)
			{
				Vector2 vector = startPos + direction * distancePerStrike * i;
				TG.GenerateAt(vector, 22);
				Vector2 vector2 = startPos + direction.Rotate(15f) * distancePerStrike * i;
				TG.GenerateAt(vector2, 22);
				Vector2 vector3 = startPos + direction.Rotate(-15f) * distancePerStrike * i;
				TG.GenerateAt(vector3, 22);
				yield return new WaitForSeconds(0.03f);
			}
		}
	}
}
