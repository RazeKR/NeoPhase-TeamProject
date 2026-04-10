using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CItemBindSlot : MonoBehaviour, IDropHandler
{
    [Header("Slot Index")]
    [SerializeField] private int _slotIndex;
    [Header("Amount")]
    [SerializeField] private Text _amount;
    [Header("Icon Image")]
    [SerializeField] private Image _iconImage;

    private int _currentItemId = 0;
    private GameObject _dragIcon;

    private void Start()
    {
        _dragIcon = CInventorySystemJ.Instance.DragIconVisual;
        UpdateSlotUI();
    }

    private void OnEnable()
    {
        StartCoroutine(SetupSlotRoutine());
    }

    private IEnumerator SetupSlotRoutine()
    {
        while (CInventorySystemJ.Instance == null || CDataManager.Instance == null)
        {
            yield return null;
        }

        while (!CDataManager.Instance.IsInitialized)
        {
            yield return null;
        }

        CInventorySystemJ.Instance.OnInventoryChanged -= UpdateSlotUI;
        CInventorySystemJ.Instance.OnInventoryChanged += UpdateSlotUI;

        UpdateSlotUI();
    }

    private void OnDisable()
    {
        if (CInventorySystemJ.Instance != null)
            CInventorySystemJ.Instance.OnInventoryChanged -= UpdateSlotUI;
    }

    public void OnDrop(PointerEventData eventData)
    {
        CPotionDataSO dragged = CInventorySystemJ.Instance.CurrenlyDraggingPotion;

        if (dragged != null)
        {
            if (CInventorySystemJ.Instance.EquipPotion(dragged, _slotIndex))
            {
                UpdateSlotUI();
            }
        }
    }

    private void UpdateSlotUI()
    {
        if (CInventorySystemJ.Instance == null)
        {
            CDebug.Log("CInventorySystemJ.Instance ==  null");
            return;
        }

        if (CInventorySystemJ.Instance._equippedPotionIds == null
            || _slotIndex < 0
            || _slotIndex >= CInventorySystemJ.Instance._equippedPotionIds.Count)
        {
            ClearSlot();
            return;
        }

        int equippedId = CInventorySystemJ.Instance._equippedPotionIds[_slotIndex];
        _currentItemId = equippedId;

        if (equippedId == 0)
        {
            ClearSlot();
            return;
        }

        CItemDataSO so = CDataManager.Instance.GetItem(equippedId);
        if (so != null)
        {
            _iconImage.sprite = so.ItemSprite;
            _iconImage.enabled = true;

            var exist = CInventorySystemJ.Instance._inventory.Find(i => i._itemData.Id == _currentItemId) as CPotionInstance;

            if (exist != null)
            {
                _amount.text = exist._amount.ToString();
                _amount.enabled = true;
            }
            else
            {
                _amount.text = "0";
            }
        }
        else ClearSlot();
        _dragIcon.SetActive(false);
    }

    private void ClearSlot()
    {
        _iconImage.enabled = false;
        _amount.text = "";
        _amount.enabled = false;
        _currentItemId = 0;
    }
}
