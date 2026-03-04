using UnityEngine;

public class NotificationOnDisable : MonoBehaviour
{
	[SerializeField]
	private string notification;

	private bool _isInit;

	private void Start()
	{
		_isInit = true;
	}

	private void OnDisable()
	{
		if (_isInit)
		{
			this.PostNotification(notification, base.gameObject);
		}
	}
}
