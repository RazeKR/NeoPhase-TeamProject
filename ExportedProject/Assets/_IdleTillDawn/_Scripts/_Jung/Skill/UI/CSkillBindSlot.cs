using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CSkillBindSlot : MonoBehaviour, IDropHandler
{
    [Header("슬롯 인덱스")]
    [SerializeField] private int _slotIndex;

    public Image IconImage; // 장착 아이콘

    #region UnityMethods

    private void Start()
    {
        UpdateSlotUI(); // 게임 시작 시 이미 로드된 데이터가 있다면 즉시 갱신
    }

    private void OnEnable()
    {
        StartCoroutine(SetupSlopRoutine());      
    }

    private IEnumerator SetupSlopRoutine()
    {
        while (CSkillSystem.Instance == null || CDataManager.Instance == null)
        {
            yield return null;
        }

        while (!CDataManager.Instance.IsInitialized)
        {
            yield return null;
        }

        CSkillSystem.Instance.OnSkillEquipped -= UpdateSlotUI;
        CSkillSystem.Instance.OnSkillEquipped += UpdateSlotUI;

        UpdateSlotUI();
        Debug.Log("슬롯 UI 갱신 완료");
    }

    private void OnDisable()
    {
        if (CSkillSystem.Instance != null)
        {
            CSkillSystem.Instance.OnSkillEquipped -= UpdateSlotUI;
        }
    }

    #endregion

    public void OnDrop(PointerEventData eventData)
    {
        CSkillDataSO dragged = CSkillSystem.Instance.CurrentlyDraggingSkill;

        if (dragged != null)
        {
            if (CSkillSystem.Instance.EquipSkill(dragged, _slotIndex))
            {
                UpdateSlotUI();
            }
        }
    }

    public void UpdateSlotUI()
    {
        if (CSkillSystem.Instance == null)
        {
            Debug.Log("CSkillSystem 인스턴스가 아직 존재하지 않습니다.");
            return;
        }

        if (CSkillSystem.Instance._equippedSkills ==  null)
        {
            IconImage.enabled = false;
            Debug.Log("_equippedSkills ==  null");
            return;
        }
        if (_slotIndex < 0)
        {
            IconImage.enabled = false;
            Debug.Log("_slotIndex < 0");
            return;
        }
        if (_slotIndex >= CSkillSystem.Instance._equippedSkills.Count)
        {
            IconImage.enabled = false;
            Debug.Log("_slotIndex >= CSkillSystem.Instance._equippedSkills.Count");
            return;
        }

        int equippedId = CSkillSystem.Instance._equippedSkills[_slotIndex];

        CSkillDataSO so = CDataManager.Instance.GetSkill(equippedId);

        if (so != null)
        {
            IconImage.sprite = so.icon;
            IconImage.enabled = true;
        }

        else IconImage.enabled = false;
    }
}
