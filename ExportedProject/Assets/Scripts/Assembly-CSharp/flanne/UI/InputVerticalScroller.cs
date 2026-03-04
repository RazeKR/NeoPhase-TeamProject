using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace flanne.UI
{
	[RequireComponent(typeof(Scrollbar))]
	public class InputVerticalScroller : MonoBehaviour
	{
		[SerializeField]
		private InputActionAsset inputs;

		[SerializeField]
		private float scrollSpeed = 1f;

		private Scrollbar scrollbar;

		private float scrollVec;

		private void OnInput(InputAction.CallbackContext context)
		{
			scrollVec = context.ReadValue<Vector2>().y;
		}

		private void Start()
		{
			scrollbar = GetComponent<Scrollbar>();
			inputs.FindAction("UI/Move").performed += OnInput;
		}

		private void OnDestroy()
		{
			inputs.FindAction("UI/Move").performed -= OnInput;
		}

		private void Update()
		{
			float num = scrollVec * scrollSpeed * Time.unscaledDeltaTime;
			scrollbar.value += num;
		}
	}
}
