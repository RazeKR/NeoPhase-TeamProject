using UnityEngine;
using UnityEngine.EventSystems;

namespace flanne.UI
{
	public class ScaleOnPointerEnter : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		[SerializeField]
		private Vector3 scaleTo;

		private Vector3 _originalScale;

		public void Awake()
		{
			_originalScale = base.transform.localScale;
		}

		public void OnPointerEnter(PointerEventData pointerEventData)
		{
			base.transform.localScale = scaleTo;
		}

		public void OnPointerExit(PointerEventData pointerEventData)
		{
			base.transform.localScale = _originalScale;
		}
	}
}
