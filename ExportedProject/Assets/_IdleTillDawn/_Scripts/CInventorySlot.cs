using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
ㆍCInventorySlot
- 인벤토리 매니저의 정보를 받아와서 UI에 반영
- UI 클릭한 뒤 -> 인벤토리 매니저의 SwapWeapon로 던저줌 or UsePotion로 던져줌
*/

public class CInventorySlot : MonoBehaviour
{
    [SerializeField] private Image _itemIcon = null;                // 이미지
    [SerializeField] private Image _itemEquipMark = null;           // 장착중인 아이템 표시
    [SerializeField] private TextMeshProUGUI _itemAmount = null;    // 갯수 스택 (물약)
    [SerializeField] private Image _itemRank = null;                // 무기 등급
    [SerializeField] private Sprite[] _itemRanksSprite = null;      // 등급 표시용 스프라이트
    
    private CItemInstance _item;
    

    public void SetSlot(CItemInstance item)
    {
        _itemIcon.sprite = item._itemData.ItemSprite;
        _item = item;

        // 무기 등급 표시 (이미지는 아마 색깔 테두리로 or 이니셜로 설정도 가능)
        if (item is CWeaponInstance weapon)
        {
            _itemRank.sprite = _itemRanksSprite[weapon._rank];
            _itemRank.gameObject.SetActive(true);
            _itemAmount.gameObject.SetActive(false);

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
            _itemAmount.text = potion._amount.ToString();
            _itemRank.gameObject.SetActive(false);
            _itemAmount.gameObject.SetActive(true);
            _itemEquipMark.gameObject.SetActive(false);
        }

        else
        {
            _itemRank.gameObject.SetActive(false);
            _itemAmount.gameObject.SetActive(false);
            _itemEquipMark.gameObject.SetActive(false);
        }
    }


    // 버튼 컴포넌트에 연결하여 호출
    // 클릭 시 클릭한 무기로 스왑 / 포션 사용
    public void OnSlotClick()
    {
        if (_item is CWeaponInstance weapon)
        {
            Debug.Log("무기 판정");
            CInventoryManager.Instance.SwapWeapon(weapon._instanceID);
        }

        else if (_item is CPotionInstance potion)
        {
            Debug.Log("포션 판정");
            CInventoryManager.Instance.UsePotion(potion._instanceID);
        }
    }
}
