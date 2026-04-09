using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// �κ��丮 �Ŵ����� ������ �޾ƿͼ� UI�� �ݿ��մϴ�.
/// </summary>

public class CInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region Inspectors & PrivateVariables

    [SerializeField] private Image _itemIcon = null;                // �̹���
    [SerializeField] private Image _itemEquipMark = null;           // �������� ������ ǥ��
    [SerializeField] private TextMeshProUGUI _itemTMP = null;       // ���� ���� (����) / ��ȭ�� (����)
    [SerializeField] private Image _itemRank = null;                // ���� ���
    [SerializeField] private Sprite[] _itemRanksSprite = null;      // ��� ǥ�ÿ� ��������Ʈ
    [SerializeField] private Image _selectedHighlight = null;       // 다중선택 하이라이트 이미지

    private CItemInstance _item;
    private GameObject _dragIcon;
    #endregion

    #region PublicMethods

    public void SetSlot(CItemInstance item)
    {
        _itemIcon.sprite = item._itemData.ItemSprite;
        _item = item;

        // ���� ��� ǥ��
        if (item is CWeaponInstance weapon)
        {
            _itemRank.sprite = _itemRanksSprite[weapon._rank];

            _itemTMP.text = "+" + weapon._upgrade.ToString();

            if (weapon._isEquipped)
            {
                _itemEquipMark.gameObject.SetActive(true);
            }
            else
            {
                _itemEquipMark.gameObject.SetActive(false);
            }
        }

        else if (item is CPotionInstance potion)
        {
            _itemRank.sprite = _itemRanksSprite[0];

            _itemTMP.text = potion._amount.ToString();
            _itemEquipMark.gameObject.SetActive(false);
        }

        else if (item is CScrollInstance scroll)
        {
            _itemRank.sprite = _itemRanksSprite[0];

            _itemTMP.text = scroll._amount.ToString();
            _itemEquipMark.gameObject.SetActive(false);
        }

        // 이미 선택된 상태 복원
        SetSelected(CInventoryUI.Instance.IsSelected(_item._instanceID));
    }

    /// <summary>선택 하이라이트를 켜거나 끕니다.</summary>
    public void SetSelected(bool selected)
    {
        if (_selectedHighlight != null)
            _selectedHighlight.gameObject.SetActive(selected);
    }

    /// <summary>
    /// �� ���� UI�� Ȱ��ȭ�մϴ�.
    /// ��ư ������Ʈ�� �����Ͽ� ȣ���մϴ�.
    /// </summary>
    public void OnSlotClick()
    {
        if (CInventoryUI.Instance.IsChoiceUpgrade)
        {
            if (_item is CWeaponInstance weapon)
            {

                CInventorySystemJ.Instance.UseScroll(weapon._instanceID);

                CInventoryUI.Instance.IsChoiceUpgrade = false;
                CInventoryUI.Instance.RefreshUI();
                return;
            }
            else
            {
                CInventoryUI.Instance.IsChoiceUpgrade = false;
            }
            return;
        }

        // Shift+클릭: 다중선택 토글
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            // 장착 중인 무기는 다중선택 불가
            if (_item is CWeaponInstance w && w._isEquipped) return;

            bool selected = CInventoryUI.Instance.ToggleMultiSelect(_item._instanceID);
            SetSelected(selected);
            return;
        }

        // 일반 클릭: 다중선택 모드가 활성 중이면 해제 후 단일 선택
        if (CInventoryUI.Instance.IsMultiSelectMode)
        {
            CInventoryUI.Instance.ClearMultiSelect();
        }
        CInventoryUI.Instance.Item = _item;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_item._itemData.ItemType != EItemType.Potion) return;

        _dragIcon = CInventorySystemJ.Instance.DragIconVisual;
        _dragIcon.SetActive(true);
        _dragIcon.GetComponent<Image>().sprite = _item._itemData.ItemSprite;
        _dragIcon.GetComponent<CanvasGroup>().blocksRaycasts = false;

        CInventorySystemJ.Instance.CurrenlyDraggingPotion = _item._itemData as CPotionDataSO;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_dragIcon != null) _dragIcon.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_dragIcon != null) _dragIcon.SetActive(false);
        CInventorySystemJ.Instance.CurrenlyDraggingPotion = null;
    }
        
    #endregion
}
