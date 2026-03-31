using UnityEngine;

/// <summary>아이템의 종류를 구분하는 열거형입니다.</summary>
public enum EItemType
{
    Weapon, // 무기
    Potion, // 포션
    Scroll  // 스크롤
}

/// <summary>
/// 모든 아이템 데이터의 기반 ScriptableObject입니다.
/// CDataManager.GetItem(id)로 int ID 기반 접근을 지원합니다.
/// 기존 인벤토리 시스템과의 호환을 위해 string _itemId 필드를 유지합니다.
/// </summary>
public class CItemDataSO : CBaseDataSO
{
    #region InspectorVariables

    [Header("기본 아이템 정보")]
    [SerializeField] protected string _itemId;      // 직렬화용 문자열 ID (기존 CInventoryManager 호환)
    [SerializeField] protected string _itemName;    // 아이템 이름
    [SerializeField] protected EItemType _type;     // 아이템 종류
    [SerializeField] protected Sprite _sprite;      // 아이템 아이콘 스프라이트

    #endregion

    #region Properties

    /// <summary>기존 인벤토리 직렬화 호환용 문자열 ID. 신규 코드는 int Id를 사용합니다.</summary>
    public string    ItemId     => _itemId;
    public string    ItemName   => _itemName;
    public EItemType ItemType   => _type;
    public Sprite    ItemSprite => _sprite;

    #endregion
}
