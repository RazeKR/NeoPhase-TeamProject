using UnityEngine;

namespace flanne.PowerupSystem
{
	public class SpreadCurseOnKill : MonoBehaviour
	{
		[SerializeField]
		private float range = 2f;

		private CurseSystem CS;

		private void OnCurseKill(object sender, object args)
		{
			GameObject gameObject = args as GameObject;
			Collider2D[] array = Physics2D.OverlapCircleAll(gameObject.transform.position, range, 1 << (int)TagLayerUtil.Enemy);
			foreach (Collider2D collider2D in array)
			{
				if (collider2D.gameObject != gameObject)
				{
					CS.Curse(collider2D.gameObject);
				}
			}
		}

		private void Start()
		{
			CS = CurseSystem.Instance;
			this.AddObserver(OnCurseKill, CurseSystem.CurseKillNotification);
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnCurseKill, CurseSystem.CurseKillNotification);
		}
	}
}
