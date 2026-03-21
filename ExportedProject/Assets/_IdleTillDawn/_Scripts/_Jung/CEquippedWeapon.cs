using UnityEngine;

/*
ㆍCEquippedWeapon
- 인스펙터로 지정된 게임 오브젝트 스프라이트를 현재 장착중인 스프라이트로 교체해줌
- 가벼운 무기 연출 (animator)
*/

public class CEquippedWeapon : MonoBehaviour
{
    [SerializeField] private GameObject _targetObject = null;

    private SpriteRenderer _sprite;

    private void Awake()
    {
        if (_targetObject == null)
        {
            Debug.Log("_targetObject 인스펙터 비어있음");
            enabled = false;
        }

        bool getSpriteRenderer = _targetObject.TryGetComponent<SpriteRenderer>(out _sprite);

        if (!getSpriteRenderer)
        {
            Debug.Log("_targetObject가 SpriteRenderer를 포함하고 있지 않음");
            enabled = false;
        }

        LoadEquippedWeapon();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            WeaponRebound();
        }
    }


    // 장착중인 무기 정보를 불러옴
    public void LoadEquippedWeapon()
    {
        _sprite.sprite = CInventoryManager.Instance.EquippedWeapon._data.ItemSprite;
    }


    public void WeaponRebound()
    {
        Animator anim = _targetObject.GetComponent<Animator>();

        anim.Play("Fire", 0, 0f);

        if (CInventoryManager.Instance.EquippedWeapon._data.ItemId == "weapon_05")
        {
            anim.Play("Swing", 0, 0f);
        }
    }
}
