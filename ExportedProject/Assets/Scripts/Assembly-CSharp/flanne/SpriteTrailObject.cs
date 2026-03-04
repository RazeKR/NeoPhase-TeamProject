using UnityEngine;

namespace flanne
{
	public class SpriteTrailObject : MonoBehaviour
	{
		public SpriteRenderer mRenderer;

		public Color mStartColor;

		public Color mEndColor;

		private bool mbInUse;

		private Vector2 mPosition;

		private float mDisplayTime;

		private float mTimeDisplayed;

		private SpriteTrail mSpawner;

		private void Start()
		{
			mRenderer.enabled = false;
		}

		private void Update()
		{
			if (mbInUse)
			{
				base.transform.position = mPosition;
				mTimeDisplayed += Time.deltaTime;
				mRenderer.color = Color.Lerp(mStartColor, mEndColor, mTimeDisplayed / mDisplayTime);
				if (mTimeDisplayed >= mDisplayTime)
				{
					mSpawner.RemoveTrailObject(base.gameObject);
					mbInUse = false;
					mRenderer.enabled = false;
				}
			}
		}

		public void Initiate(float displayTime, Sprite sprite, Vector2 position, SpriteTrail trail)
		{
			mDisplayTime = displayTime;
			mRenderer.sprite = sprite;
			mRenderer.enabled = true;
			mPosition = position;
			mTimeDisplayed = 0f;
			mSpawner = trail;
			mbInUse = true;
		}
	}
}
