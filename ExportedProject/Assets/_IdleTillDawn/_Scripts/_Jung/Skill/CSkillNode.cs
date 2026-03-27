using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CSkillNode : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("데이터")]
    [SerializeField] private CSkillDataSO _skillData;

    [Header("아이콘 UI")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private Text _levelText;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] public GameObject _lockOverlay;

    [Header("정보 UI")]
    [SerializeField] private GameObject _infoUIGO;
    [SerializeField] private Text _skillNameText; 
    [SerializeField] private Text _skillTypeText;
    [SerializeField] private Text _skillInfoText;

    [Header("드래그용 아이콘")]
    [SerializeField] private GameObject _dragIcon;  // 드래그 전용 UI

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


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CSkillManager.Instance.GetSkillLevel(SkillData.skillName) <= 0) return;
        if (SkillData.skillType == ESkillType.Passive) return;

        _dragIcon = CSkillManager.Instance.DragIconVisual;
        _dragIcon.SetActive(true);
        _dragIcon.GetComponent<Image>().sprite = _skillData.icon;
        _dragIcon.GetComponent<CanvasGroup>().blocksRaycasts = false;
        
        CSkillManager.Instance.CurrentlyDraggingSkill = _skillData;
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (_dragIcon != null) _dragIcon.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_dragIcon != null) _dragIcon.SetActive(false);
    }
}
