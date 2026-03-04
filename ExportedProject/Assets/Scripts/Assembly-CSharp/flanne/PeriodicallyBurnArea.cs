using UnityEngine;

namespace flanne
{
	public class PeriodicallyBurnArea : MonoBehaviour
	{
		[SerializeField]
		private float timePerTick;

		[SerializeField]
		private int burnDamage;

		[SerializeField]
		private float rangeRadius = 1f;

		private BurnSystem burnSys;

		private Collider2D[] _colliders = new Collider2D[2];

		private int _layer;

		private float _timer;

		private void Start()
		{
			burnSys = BurnSystem.SharedInstance;
			_layer = 1 << (int)TagLayerUtil.Enemy;
		}

		private void Update()
		{
			_timer += Time.deltaTime;
			if (_timer > timePerTick)
			{
				_timer -= timePerTick;
				int num = Physics2D.OverlapCircleNonAlloc(base.transform.position, rangeRadius, _colliders, _layer);
				for (int i = 0; i < num; i++)
				{
					burnSys.Burn(_colliders[i].gameObject, burnDamage);
				}
			}
		}
	}
}
