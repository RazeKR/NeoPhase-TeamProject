using UnityEngine;

namespace flanne
{
	public class ArrowRetrievalPoint : MonoBehaviour
	{
		private void Start()
		{
			ArrowRetrieveOnReload.RegisterRetrievalPoint(this);
		}

		private void OnDestroy()
		{
			ArrowRetrieveOnReload.RemoveRetrievalPoint(this);
		}
	}
}
