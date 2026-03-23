using UnityEngine;

public enum EItemType
{
    Weapon,
    Potion,
    Scroll
}

public class CItemDataSO : ScriptableObject
{
    [SerializeField] protected string _itemId;
    [SerializeField] protected string _itemName;
    [SerializeField] protected EItemType _type;
    [SerializeField] protected Sprite _sprite;

    public string ItemId => _itemId;        // 아이템 아이디
    public string ItemName => _itemName;    // 아이템 이름
    public EItemType ItemType => _type;     // 아이템 타입
    public Sprite ItemSprite => _sprite;    // 아이템 스프라이트
}
