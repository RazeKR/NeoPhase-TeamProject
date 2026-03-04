using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using flanne.Core;
using flanne.PerkSystem;
using flanne.Player;

namespace flanne
{
	public class PlayerController : StateMachine
	{
		public static PlayerController Instance;

		public UnityEvent onDestroyed;

		public PlayerInput playerInput;

		public SpriteRenderer playerSprite;

		public Animator playerAnimator;

		public PlayerHealth playerHealth;

		public PlayerBuffs playerBuffs;

		public StatsHolder stats;

		public Gun gun;

		public Ammo ammo;

		public Slider reloadBar;

		public CharacterData loadedCharacter;

		public PlayerPerks playerPerks;

		public GameObject knockbackObject;

		public SoundEffectSO reloadStartSFX;

		public SoundEffectSO reloadEndSFX;

		public float movementSpeed = 8f;

		[SerializeField]
		private SoundEffectSO footstepSFX;

		[SerializeField]
		private float footstepSFXCooldown;

		private float timerFootStepSFX;

		[NonSerialized]
		public float moveSpeedMultiplier;

		[NonSerialized]
		public bool faceMouse;

		public BoolToggle disableMove;

		public BoolToggle disableAction;

		public BoolToggle disableAnimation;

		public BoolToggle disableFacing;

		private ShootingCursor SC;

		private InputAction _moveAction;

		public Vector3 moveVec { get; private set; }

		public float finalMoveSpeed => stats[StatType.MoveSpeed].Modify(movementSpeed);

		private void OnMove(InputAction.CallbackContext obj)
		{
			Vector2 vector = obj.ReadValue<Vector2>();
			moveVec = new Vector3(vector.x, vector.y, 0f).normalized;
		}

		private void Awake()
		{
			if (Instance != null)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			Instance = this;
			disableMove = new BoolToggle(b: false);
			disableAction = new BoolToggle(b: false);
			disableAnimation = new BoolToggle(b: false);
			disableFacing = new BoolToggle(b: false);
			ChangeState<IdleState>();
			_moveAction = playerInput.actions["Move"];
		}

		private void Start()
		{
			SC = ShootingCursor.Instance;
		}

		private void Update()
		{
			if (!PauseController.isPaused && playerHealth.hp != 0)
			{
				MovePlayer();
				UpdateSprite();
			}
		}

		private void OnEnable()
		{
			_moveAction.performed += OnMove;
		}

		private void OnDisable()
		{
			_moveAction.performed -= OnMove;
			_currentState.Exit();
		}

		private void OnDestroy()
		{
			onDestroyed.Invoke();
		}

		public void SetPosition(Vector2 pos)
		{
			playerSprite.transform.localPosition = Vector3.zero;
			base.transform.position = pos;
		}

		public void KnockbackNearby()
		{
			knockbackObject.SetActive(value: true);
		}

		private void MovePlayer()
		{
			if (!disableMove.value)
			{
				Vector3 vector = playerSprite.transform.position + moveSpeedMultiplier * moveVec * stats[StatType.MoveSpeed].Modify(movementSpeed) * Time.deltaTime;
				SetPosition(vector);
			}
		}

		private void UpdateSprite()
		{
			UpdateAnimation();
			UpdateFacing();
		}

		private void UpdateAnimation()
		{
			if (disableAnimation.value)
			{
				return;
			}
			if (moveVec == Vector3.zero || disableMove.value)
			{
				playerAnimator.ResetTrigger("Run");
				playerAnimator.ResetTrigger("Walk");
				playerAnimator.SetTrigger("Idle");
			}
			else if (moveSpeedMultiplier >= 1f)
			{
				playerAnimator.ResetTrigger("Idle");
				playerAnimator.ResetTrigger("Walk");
				playerAnimator.SetTrigger("Run");
				timerFootStepSFX += Time.deltaTime;
				if (timerFootStepSFX > footstepSFXCooldown)
				{
					timerFootStepSFX -= footstepSFXCooldown;
					footstepSFX.Play();
				}
			}
			else
			{
				playerAnimator.ResetTrigger("Idle");
				playerAnimator.ResetTrigger("Run");
				playerAnimator.SetTrigger("Walk");
			}
		}

		private void UpdateFacing()
		{
			if (disableFacing.value)
			{
				return;
			}
			if (playerSprite != null)
			{
				if (faceMouse)
				{
					Vector3 vector = Camera.main.ScreenToWorldPoint(SC.cursorPosition);
					Vector3 position = base.transform.position;
					Vector3 vector2 = vector - position;
					if (vector2.x < 0f)
					{
						playerSprite.flipX = true;
					}
					else if (vector2.x > 0f)
					{
						playerSprite.flipX = false;
					}
				}
				else if (moveVec.x < 0f)
				{
					playerSprite.flipX = true;
				}
				else if (moveVec.x > 0f)
				{
					playerSprite.flipX = false;
				}
			}
			else
			{
				Debug.LogError("No sprite renderer on player");
			}
		}
	}
}
