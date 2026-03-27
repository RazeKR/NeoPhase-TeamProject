using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CSkillBindSlot : MonoBehaviour, IDropHandler
{
    [Header("ΩΩ∑‘ ¿Œµ¶Ω∫")]
    [SerializeField] private int _slotIndex;

    public Image IconImage; // ¿Â¬¯ æ∆¿Ãƒ‹


    private void Start()
    {
        UpdateSlotUI();
    }


    public void OnDrop(PointerEventData eventData)
    {
        CSkillDataSO dragged = CSkillManager.Instance.CurrentlyDraggingSkill;

        if (dragged != null)
        {
            if (CSkillManager.Instance.EquipSkill(dragged, _slotIndex))
            {
                UpdateSlotUI();
            }
        }
    }

    public void UpdateSlotUI()
    {
        CSkillDataSO equipped = CSkillManager.Instance._equippedSkills[_slotIndex];

        if (equipped != null)
        {
            IconImage.sprite = equipped.icon;
            IconImage.enabled = true;
        }

        else IconImage.enabled = false;
    }
}
