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
    [SerializeField] private TextMeshProUGUI _itemTMP = null;       // 갯수 스택 (물약) / 강화도 (무기)
    [SerializeField] private Image _itemRank = null;                // 무기 등급
    [SerializeField] private Sprite[] _itemRanksSprite = null;      // 등급 표시용 스프라이트
    
    private CItemInstance _item;



    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스에서 레이를 쏘고, 이 오브젝트가 걸리는지 체크
            if (
                EventSystem.current.IsPointerOverGameObject()
                && RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition)
               )
                return;
        }
    }



    public void SetSlot(CItemInstance item)
    {
        _itemIcon.sprite = item._itemData.ItemSprite;
        _item = item;

        // 무기 등급 표시 (이미지는 아마 색깔 테두리로 or 이니셜로 설정도 가능)
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

        // 포션 수량 표시
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
    }


    // 버튼 컴포넌트에 연결하여 호출
    // 클릭 시 상세 정보 UI 활성화
    public void OnSlotClick()
    {
        if (CInventoryUI.Instance.IsChoiceUpgrade)
        {
            Debug.Log("스크롤 클릭 상태 확인됨");

            if (_item is CWeaponInstance weapon)
            {
                Debug.Log("강화");

                CInventoryManager.Instance.UseScroll(weapon._instanceID);

                CInventoryUI.Instance.IsChoiceUpgrade = false;
                CInventoryUI.Instance.RefreshUI();
                return;
            }
            else
            {
                Debug.Log("무기 외 다른 아이템 선택");
                CInventoryUI.Instance.IsChoiceUpgrade = false;
            }
        }

        else
        {
            Debug.Log("슬롯에 저장된 정보 전송");
            CInventoryUI.Instance.Item = _item;
        }
                  
    }

}
