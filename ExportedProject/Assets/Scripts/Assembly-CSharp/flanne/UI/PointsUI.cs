using System;
using TMPro;
using UnityEngine;

namespace flanne.UI
{
	public class PointsUI : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text ptsTMP;

		private void OnPointChanged(object sender, int pts)
		{
			Refresh(pts);
		}

		private void Start()
		{
			Refresh(PointsTracker.pts);
			PointsTracker.PointsChangedEvent = (EventHandler<int>)Delegate.Combine(PointsTracker.PointsChangedEvent, new EventHandler<int>(OnPointChanged));
		}

		private void OnDestroy()
		{
			PointsTracker.PointsChangedEvent = (EventHandler<int>)Delegate.Remove(PointsTracker.PointsChangedEvent, new EventHandler<int>(OnPointChanged));
		}

		private void Refresh(int pts)
		{
			ptsTMP.text = pts.ToString();
		}
	}
}
