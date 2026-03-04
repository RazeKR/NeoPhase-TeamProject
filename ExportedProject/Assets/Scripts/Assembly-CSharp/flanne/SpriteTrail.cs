using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class SpriteTrail : MonoBehaviour
	{
		public SpriteRenderer mLeadingSprite;

		public int mTrailSegments;

		public float mTrailTime;

		public GameObject mTrailObject;

		private float mSpawnInterval;

		private float mSpawnTimer;

		private BoolToggle mbEnabled;

		private List<GameObject> mTrailObjectsInUse;

		private Queue<GameObject> mTrailObjectsNotInUse;

		private void Start()
		{
			mSpawnInterval = mTrailTime / (float)mTrailSegments;
			mTrailObjectsInUse = new List<GameObject>();
			mTrailObjectsNotInUse = new Queue<GameObject>();
			for (int i = 0; i < mTrailSegments + 1; i++)
			{
				GameObject gameObject = Object.Instantiate(mTrailObject);
				gameObject.transform.SetParent(base.transform);
				mTrailObjectsNotInUse.Enqueue(gameObject);
			}
			mbEnabled = new BoolToggle(b: false);
		}

		private void Update()
		{
			if (!mbEnabled.value)
			{
				return;
			}
			mSpawnTimer += Time.deltaTime;
			if (mSpawnTimer >= mSpawnInterval)
			{
				GameObject gameObject = mTrailObjectsNotInUse.Dequeue();
				if (gameObject != null)
				{
					gameObject.GetComponent<SpriteTrailObject>().Initiate(mTrailTime, mLeadingSprite.sprite, base.transform.position, this);
					mTrailObjectsInUse.Add(gameObject);
					mSpawnTimer = 0f;
				}
			}
		}

		public void RemoveTrailObject(GameObject obj)
		{
			mTrailObjectsInUse.Remove(obj);
			mTrailObjectsNotInUse.Enqueue(obj);
		}

		public void SetEnabled(bool enabled)
		{
			if (enabled)
			{
				mbEnabled.Flip();
			}
			else
			{
				mbEnabled.UnFlip();
			}
			if (mbEnabled.value)
			{
				mSpawnTimer = mSpawnInterval;
			}
		}
	}
}
