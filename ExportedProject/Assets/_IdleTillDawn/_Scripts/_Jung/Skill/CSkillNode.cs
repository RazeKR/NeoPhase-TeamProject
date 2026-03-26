using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSkillNode : MonoBehaviour
{
    [Header("µ¥ÀÌÅÍ")]
    [SerializeField] private CSkillDataSO _skillData;

    [Header("UI")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private Text _levelText;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] public GameObject _lockOverlay;

    private int _currentLevel = 0;


    public CSkillDataSO SkillData => _skillData;


    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        _currentLevel = CSkillManager.Instance.GetSkillLevel(_skillData.skillName);

        _iconImage.sprite = _skillData.icon;

        if (_currentLevel == _skillData.maxLevel)
        {
            _levelText.text = "MAX";
            _levelText.color = Color.yellow;
        }

        else
        {
            _levelText.text = $"{_currentLevel} / {_skillData.maxLevel}";
            _levelText.color = Color.white;

        }

        bool isUnlockable = CSkillManager.Instance.CanUnlock(this);
        _lockOverlay.SetActive(!isUnlockable && _currentLevel == 0);
        _upgradeButton.interactable = isUnlockable || _currentLevel > 0;
    }

    public void ClickUpgrade()
    {
        if (CSkillManager.Instance.TryUpgradeSkill(this))
        {
            _currentLevel++;
            UpdateUI();
        
            CSkillManager.Instance.RefreshAllNodes();
        }
    }
}
