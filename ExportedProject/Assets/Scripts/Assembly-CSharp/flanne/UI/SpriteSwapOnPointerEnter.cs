using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace flanne.UI
{
	public class SpriteSwapOnPointerEnter : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		[SerializeField]
		private Image targetImage;

		[SerializeField]
		private Sprite mouseOverSprite;

		private Sprite _originalSprite;

		public void Awake()
		{
			_originalSprite = targetImage.sprite;
		}

		public void OnPointerEnter(PointerEventData pointerEventData)
		{
			targetImage.sprite = mouseOverSprite;
		}

		public void OnPointerExit(PointerEventData pointerEventData)
		{
			targetImage.sprite = _originalSprite;
		}
	}
}
