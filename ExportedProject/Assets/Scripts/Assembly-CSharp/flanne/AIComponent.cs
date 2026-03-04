using System;
using UnityEngine;

namespace flanne
{
	public class AIComponent : MonoBehaviour
	{
		public MoveComponent2D moveComponent;

		public float baseMaxMoveSpeed;

		public float baseAcceleration;

		public bool ignoreFlock;

		public bool rotateTowardsPlayer;

		public bool dontLookAtPlayer;

		public Animator animator;

		public AISpecial specialSO;

		public float specialRangeSqr = -1f;

		public float specialCooldown;

		[NonSerialized]
		public float specialTimer;

		public Transform specialPoint;

		public bool dontFaceDuringSpecial;

		[NonSerialized]
		public bool frozen;

		[NonSerialized]
		public int damageToPlayer = 1;

		[NonSerialized]
		public float maxMoveSpeed;

		[NonSerialized]
		public float acceleration;

		private void Awake()
		{
			maxMoveSpeed = baseMaxMoveSpeed;
			acceleration = baseAcceleration;
		}

		private void Start()
		{
			AIController.SharedInstance.Register(this);
		}

		private void OnEnable()
		{
			specialTimer = 0f;
		}

		private void OnDestroy()
		{
			AIController.SharedInstance.UnRegister(this);
		}
	}
}
