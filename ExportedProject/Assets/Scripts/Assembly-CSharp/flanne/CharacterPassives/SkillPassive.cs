using UnityEngine;
using UnityEngine.InputSystem;
using flanne.Core;

namespace flanne.CharacterPassives
{
	public abstract class SkillPassive : MonoBehaviour
	{
		[SerializeField]
		private InputActionAsset inputs;

		[SerializeField]
		private float cooldown;

		[SerializeField]
		private SoundEffectSO soundFX;

		private InputAction _skillAction;

		private float _timer;

		private void PerformSkillCallback(InputAction.CallbackContext context)
		{
			if (_timer <= 0f && !PauseController.isPaused)
			{
				_timer += cooldown;
				PerformSkill();
				soundFX?.Play();
			}
		}

		private void Start()
		{
			_skillAction = inputs.FindActionMap("PlayerMap").FindAction("Skill");
			_skillAction.performed += PerformSkillCallback;
			Init();
		}

		private void OnDestroy()
		{
			_skillAction.performed -= PerformSkillCallback;
		}

		private void Update()
		{
			if (_timer > 0f)
			{
				_timer -= Time.deltaTime;
			}
		}

		protected virtual void Init()
		{
		}

		protected abstract void PerformSkill();
	}
}
