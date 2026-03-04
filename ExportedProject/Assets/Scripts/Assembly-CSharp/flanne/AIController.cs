using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class AIController : MonoBehaviour
	{
		public static AIController SharedInstance;

		public bool playerRepel;

		[SerializeField]
		private Transform followTransform;

		[SerializeField]
		private float flockDistance;

		private static List<AIComponent> aiComponents;

		private Collider2D[] _colliders = new Collider2D[2];

		private int _layer;

		public List<AIComponent> enemies => new List<AIComponent>(aiComponents);

		private void Awake()
		{
			SharedInstance = this;
			aiComponents = new List<AIComponent>();
		}

		private void Start()
		{
			_layer = 1 << (int)TagLayerUtil.Enemy;
		}

		private void FixedUpdate()
		{
			for (int i = 0; i < aiComponents.Count; i++)
			{
				if (aiComponents[i].frozen)
				{
					aiComponents[i].moveComponent.vector = Vector2.zero;
				}
				else
				{
					if (!aiComponents[i].gameObject.activeInHierarchy)
					{
						continue;
					}
					AIComponent aIComponent = aiComponents[i];
					Vector2 vector = followTransform.position - aIComponent.transform.position;
					if (playerRepel)
					{
						Vector2 vector2 = -1f * vector;
						if (Vector3.Dot(aIComponent.moveComponent.vector, vector2.normalized) < aIComponent.maxMoveSpeed)
						{
							aIComponent.moveComponent.vector += vector2.normalized * aIComponent.acceleration * Time.fixedDeltaTime;
						}
					}
					else if (vector.sqrMagnitude < aIComponent.specialRangeSqr || aIComponent.specialTimer > 0f)
					{
						if (!aIComponent.dontFaceDuringSpecial)
						{
							AILookTowards(aIComponent, vector);
						}
						if (aIComponent.specialTimer <= 0f)
						{
							aIComponent.specialTimer += aIComponent.specialCooldown;
							aIComponent.specialSO.Use(aIComponent, followTransform);
							if (!aIComponent.dontLookAtPlayer)
							{
								AILookTowards(aIComponent, vector);
							}
						}
						aIComponent.specialTimer -= Time.fixedDeltaTime;
					}
					else
					{
						if (!aIComponent.dontLookAtPlayer)
						{
							AILookTowards(aIComponent, vector);
						}
						Vector2 zero = Vector2.zero;
						Transform closestAI = GetClosestAI(aIComponent);
						zero = ((!(closestAI != null) || aIComponent.ignoreFlock) ? vector : ((Vector2)(aIComponent.transform.position - closestAI.position)));
						if (Vector3.Dot(aIComponent.moveComponent.vector, zero.normalized) < aIComponent.maxMoveSpeed)
						{
							aIComponent.moveComponent.vector += zero.normalized * aIComponent.acceleration * Time.fixedDeltaTime;
						}
					}
				}
			}
		}

		public void Register(AIComponent ai)
		{
			aiComponents.Add(ai);
		}

		public void UnRegister(AIComponent ai)
		{
			aiComponents.Remove(ai);
		}

		private void AILookTowards(AIComponent ai, Vector2 direction)
		{
			if (ai.rotateTowardsPlayer)
			{
				float angle = Mathf.Atan2(direction.y, direction.x) * 57.29578f;
				ai.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
			}
			else if (direction.x < 0f)
			{
				ai.transform.localScale = new Vector2(-1f, 1f);
			}
			else if (direction.x > 0f)
			{
				ai.transform.localScale = new Vector2(1f, 1f);
			}
		}

		private Transform GetClosestAI(AIComponent ai)
		{
			if (ai.maxMoveSpeed <= 0f)
			{
				return null;
			}
			if (ai.gameObject.layer != (int)TagLayerUtil.Enemy)
			{
				return null;
			}
			int num = Physics2D.OverlapCircleNonAlloc(ai.transform.position, flockDistance, _colliders, _layer);
			if (num < 2)
			{
				return null;
			}
			for (int i = 0; i < num; i++)
			{
				if (_colliders[i].gameObject != ai.gameObject)
				{
					return _colliders[i].gameObject.transform;
				}
			}
			return null;
		}

		public static Vector2 GetClosestAIPos(Vector2 center)
		{
			AIComponent aIComponent = null;
			float num = float.PositiveInfinity;
			for (int i = 0; i < aiComponents.Count; i++)
			{
				if (aiComponents[i].gameObject.activeSelf && !aiComponents[i].tag.Contains("Passive"))
				{
					Vector2 vector = aiComponents[i].transform.position;
					float sqrMagnitude = (center - vector).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						aIComponent = aiComponents[i];
					}
				}
			}
			Vector2 result = Vector2.zero;
			if (aIComponent != null)
			{
				result = aIComponent.gameObject.transform.position;
			}
			return result;
		}
	}
}
