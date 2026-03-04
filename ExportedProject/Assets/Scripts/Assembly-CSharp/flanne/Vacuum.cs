using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class Vacuum : MonoBehaviour
	{
		[SerializeField]
		private string hitTag;

		[SerializeField]
		private float vacuumStrength;

		private List<MoveComponent2D> _inRangeMoveComponents;

		private void OnEnable()
		{
			_inRangeMoveComponents = new List<MoveComponent2D>();
		}

		private void Update()
		{
			for (int i = 0; i < _inRangeMoveComponents.Count; i++)
			{
				if (!_inRangeMoveComponents[i].knockbackImmune && _inRangeMoveComponents[i].gameObject.activeSelf)
				{
					Vector2 vector = ((Vector2)(base.transform.position - _inRangeMoveComponents[i].transform.position)).normalized * vacuumStrength * Time.deltaTime;
					_inRangeMoveComponents[i].vector += vector;
				}
			}
		}

		private void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.tag.Contains(hitTag))
			{
				MoveComponent2D component = other.gameObject.GetComponent<MoveComponent2D>();
				if (component != null)
				{
					_inRangeMoveComponents.Add(component);
				}
			}
		}

		private void OnCollisionExit2D(Collision2D other)
		{
			if (other.gameObject.tag.Contains(hitTag))
			{
				MoveComponent2D component = other.gameObject.GetComponent<MoveComponent2D>();
				if (component != null)
				{
					_inRangeMoveComponents.Remove(component);
				}
			}
		}
	}
}
