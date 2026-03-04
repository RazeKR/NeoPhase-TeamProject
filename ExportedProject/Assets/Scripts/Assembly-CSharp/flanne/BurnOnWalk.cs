using UnityEngine;

namespace flanne
{
	public class BurnOnWalk : MonoBehaviour
	{
		[SerializeField]
		private ParticleSystem particles;

		[SerializeField]
		private SoundEffectSO soundFX;

		[SerializeField]
		private float range;

		[SerializeField]
		private float distanceToCast;

		[SerializeField]
		private int burnDamage;

		private BurnSystem BS;

		private float _distanceCtr;

		private Vector2 _lastPos;

		private Vector2 _currPos;

		private void Start()
		{
			BS = BurnSystem.SharedInstance;
			_lastPos = base.transform.position;
			_currPos = base.transform.position;
		}

		private void Update()
		{
			_lastPos = _currPos;
			_currPos = base.transform.position;
			_distanceCtr += (_lastPos - _currPos).magnitude;
			if (!(_distanceCtr >= distanceToCast))
			{
				return;
			}
			_distanceCtr -= distanceToCast;
			particles.Play();
			soundFX.Play();
			Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, range);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].gameObject.tag == "Enemy")
				{
					BS.Burn(array[i].gameObject, burnDamage);
					FlashSprite componentInChildren = array[i].gameObject.GetComponentInChildren<FlashSprite>();
					if (componentInChildren != null)
					{
						componentInChildren.Flash();
					}
				}
			}
		}
	}
}
