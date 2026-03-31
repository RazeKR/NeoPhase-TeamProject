using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 매니저의 정보를 받아와서 UI에 반영합니다.
/// </summary>

public class CInventorySlot : MonoBehaviour
{
    #region Inspectors & PrivateVariables

    [SerializeField] private Image _itemIcon = null;                // 이미지
    [SerializeField] private Image _itemEquipMark = null;           // 장착중인 아이템 표시
    [SerializeField] private TextMeshProUGUI _itemTMP = null;       // 갯수 스택 (물약) / 강화도 (무기)
    [SerializeField] private Image _itemRank = null;                // 무기 등급
    [SerializeField] private Sprite[] _itemRanksSprite = null;      // 등급 표시용 스프라이트
    
    private CItemInstance _item;

    #endregion

    #region UnityMethods

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

    #endregion

    #region PublicMethods


    /// <summary>
    /// 아이템 인스턴스를 받아와 슬롯의 정보를 설정합니다.
    /// 이미지, 장착여부, 등급, 수량 정보가 표시됩니다.
    /// </summary>
    public void SetSlot(CItemInstance item)
    {
        _itemIcon.sprite = item._itemData.ItemSprite;
        _item = item;

        // 무기 등급 표시
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

    /// <summary>
    /// 상세 정보 UI를 활성화합니다.
    /// 버튼 컴포넌트에 연결하여 호출합니다.
    /// </summary>
    public void OnSlotClick()
    {
        if (CInventoryUI.Instance.IsChoiceUpgrade)
        {
            Debug.Log("스크롤 클릭 상태 확인됨");

            if (_item is CWeaponInstance weapon)
            {
                Debug.Log("강화");

                CInventorySystemJ.Instance.UseScroll(weapon._instanceID);

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

    #endregion
}
