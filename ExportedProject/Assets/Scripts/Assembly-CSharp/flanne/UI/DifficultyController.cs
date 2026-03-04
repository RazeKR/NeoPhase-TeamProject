using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace flanne.UI
{
	public class DifficultyController : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		[SerializeField]
		private InputActionAsset inputs;

		[SerializeField]
		private TMP_Text labelTMP;

		[SerializeField]
		private TMP_Text descriptionTMP;

		[SerializeField]
		private TMP_Text unlockTMP;

		[SerializeField]
		private DifficultyModList modList;

		[SerializeField]
		private LocalizedString darknessLabel;

		private int _maxDiffictuly;

		private InputAction _moveAction;

		public int difficulty { get; private set; }

		public void OnMove(InputAction.CallbackContext context)
		{
			float x = context.ReadValue<Vector2>().x;
			if (x > 0.5f)
			{
				IncreaseDifficulty();
			}
			else if (x < -0.5f)
			{
				DecreaseDifficulty();
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			_moveAction.performed += OnMove;
		}

		public void OnDeselect(BaseEventData data)
		{
			_moveAction.performed -= OnMove;
		}

		public void Init(int maxDiff)
		{
			_moveAction = inputs.FindActionMap("UI").FindAction("Move");
			_maxDiffictuly = maxDiff;
			int num = PlayerPrefs.GetInt("LastSelectedDifficulty", -1);
			if (num < 0)
			{
				SetDifficulty(maxDiff);
			}
			else
			{
				SetDifficulty(num);
			}
			unlockTMP.text = "Unlocked " + _maxDiffictuly + "/" + (modList.mods.Length - 1);
		}

		private void SetDifficulty(int diff)
		{
			diff = Mathf.Clamp(diff, 0, _maxDiffictuly);
			difficulty = diff;
			labelTMP.text = LocalizationSystem.GetLocalizedValue(darknessLabel.key) + " " + difficulty;
			descriptionTMP.text = modList.mods[difficulty].description;
			Loadout.difficultyLevel = diff;
		}

		public void IncreaseDifficulty()
		{
			SetDifficulty(difficulty + 1);
			PlayerPrefs.SetInt("LastSelectedDifficulty", difficulty);
		}

		public void DecreaseDifficulty()
		{
			SetDifficulty(difficulty - 1);
			PlayerPrefs.SetInt("LastSelectedDifficulty", difficulty);
		}

		public void RefreshText()
		{
			SetDifficulty(difficulty);
		}
	}
}
