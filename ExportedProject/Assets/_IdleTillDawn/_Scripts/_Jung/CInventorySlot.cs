using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
ㆍCInventorySlot
- 인벤토리 매니저의 정보를 받아와서 UI에 반영
- UI 클릭한 뒤 -> 인벤토리 매니저의 SwapWeapon로 던저줌 or UsePotion로 던져줌
*/

public class CInventorySlot : MonoBehaviour
{
    [SerializeField] private Image _itemIcon = null;                // 이미지
    [SerializeField] private Image _itemEquipMark = null;           // 장착중인 아이템 표시
    [SerializeField] private TextMeshProUGUI _itemAmountTMP = null;    // 갯수 스택 (물약)
    [SerializeField] private Image _itemRank = null;                // 무기 등급
    [SerializeField] private Sprite[] _itemRanksSprite = null;      // 등급 표시용 스프라이트
    [SerializeField] private GameObject _selectButtonUI = null;     // 사용/버리기 버튼 UI 객체
    [SerializeField] private GameObject _potionAmountUI = null;     // 버릴 수량 선택 UI 객체
    [SerializeField] private TextMeshProUGUI _removeAmountTMP = null;
    
    private CItemInstance _item;
    private int _desiredAmount = 0;



    private void Update()
    {
        // UI 팝업 중에만 감지
        if (_selectButtonUI.activeSelf ||  _potionAmountUI.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 마우스에서 레이를 쏘고, 이 오브젝트가 걸리는지 체크
                if (
                    EventSystem.current.IsPointerOverGameObject()
                    && RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition)
                   )
                    return;

                CloseAllPopups();
            }
        }
    }



    public void SetSlot(CItemInstance item)
    {
        _itemIcon.sprite = item._itemData.ItemSprite;
        _item = item;

        _selectButtonUI.SetActive(false);
        _potionAmountUI.SetActive(false);

        // 무기 등급 표시 (이미지는 아마 색깔 테두리로 or 이니셜로 설정도 가능)
        if (item is CWeaponInstance weapon)
        {
            _itemRank.sprite = _itemRanksSprite[weapon._rank];
            _itemRank.gameObject.SetActive(true);
            _itemAmountTMP.gameObject.SetActive(false);

            if (weapon._isEquipped)
            {
                _itemEquipMark.gameObject.SetActive(true);
            }
            else
            {
                _itemEquipMark.gameObject.SetActive(false);
            }
        }

        // 포션 수량 표시
        else if (item is CPotionInstance potion)
        {
            _itemAmountTMP.text = potion._amount.ToString();
            _itemRank.gameObject.SetActive(false);
            _itemAmountTMP.gameObject.SetActive(true);
            _itemEquipMark.gameObject.SetActive(false);
        }

        else
        {
            _itemRank.gameObject.SetActive(false);
            _itemAmountTMP.gameObject.SetActive(false);
            _itemEquipMark.gameObject.SetActive(false);
        }
    }


    // 버튼 컴포넌트에 연결하여 호출
    // 클릭 시 선택 / 삭제 버튼 활성화
    public void OnSlotClick()
    {
        _potionAmountUI.SetActive(false);
        _desiredAmount = 0;
        _selectButtonUI.SetActive(true);
    }



    // 버튼 컴포넌트에 연결하여 호출
    // 클릭 시 클릭한 무기로 스왑 / 포션 사용
    public void OnSelectClick()
    {
        if (_item is CWeaponInstance weapon)
        {
            CInventoryManager.Instance.SwapWeapon(weapon._instanceID);
        }

        else if (_item is CPotionInstance potion)
        {
            CInventoryManager.Instance.UsePotion(potion._instanceID);
        }

        _selectButtonUI.SetActive(false);

        CInventoryManager.Instance.RefreshUI();
    }


    // 버튼 컴포넌트에 연결하여 호출
    // 클릭 시 아이템 삭제
    // 포션의 경우, 삭제할 수량을 추가로 처리
    public void OnDeleteClick()
    {
        if (_item is CWeaponInstance weapon)
        {
            CInventoryManager.Instance.RemoveItem(weapon._instanceID);

            CInventoryManager.Instance.RefreshUI();
        }

        else if (_item is CPotionInstance potion)
        {
            _potionAmountUI.SetActive(true);
            _removeAmountTMP.text = $"{_desiredAmount} / {(_item as CPotionInstance)._amount}";

            _selectButtonUI.SetActive(false);
        }        
    }
       

    // 버튼 컴포넌트에 연결하여 호출
    public void PotionDeleteAmountUI(bool isDown)
    {     
        if (isDown)
        {
            _desiredAmount -= (_desiredAmount > 0 ? 1 : 0);
        }

        else
        {
            _desiredAmount += (_desiredAmount < (_item as CPotionInstance)._amount ? 1 : 0);
        }

        _removeAmountTMP.text = $"{_desiredAmount} / {(_item as CPotionInstance)._amount}";
    }

    public void ConfirmAmount()
    {
        (_item as  CPotionInstance)._amount -= _desiredAmount;

        if ((_item as CPotionInstance)._amount <= 0)
        {
            CInventoryManager.Instance.RemoveItem(_item._instanceID);
        }

        CInventoryManager.Instance.SaveInventory(CInventoryManager.Instance.Inventory);

        CInventoryManager.Instance.RefreshUI();
    }


    public void CloseAllPopups()
    {
        _selectButtonUI.SetActive(false);
        _potionAmountUI.SetActive(false);
        _desiredAmount = 0;
    }
}
