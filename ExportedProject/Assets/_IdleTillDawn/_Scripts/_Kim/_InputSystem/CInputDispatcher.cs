using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CInputDispatcher : MonoBehaviour
{
    #region 인스펙터
    [Header("액션 참조")]
    [SerializeField] private InputActionReference _move;
    [SerializeField] private InputActionReference[] _skills = new InputActionReference[3];
    [SerializeField] private InputActionReference[] _items = new InputActionReference[2];
    [SerializeField] private InputActionReference _option;
    [SerializeField] private InputActionReference _shop;
    [SerializeField] private InputActionReference _inventory;
    [SerializeField] private InputActionReference _skillTree;

    [Header("디버깅")]
    [SerializeField] private bool _logInput = true;
    #endregion

    #region 내부 변수
    public static CInputDispatcher Instance { get; private set; }
    public event Action<Vector2> OnMove;
    public event Action<int> OnSkill;
    public event Action<int> OnItemUse;
    public event Action OnOption;
    public event Action OnShop;
    public event Action OnInventory;
    public event Action OnSkillTree;

    // 중복 방지용 변수
    private bool _isReady = false;
    // 스킬 인덱스 저장
    private Dictionary<InputAction, int> _skillIndexMap = new Dictionary<InputAction, int>();
    // 아이템 인덱스 저장
    private Dictionary<InputAction, int> _itemIndexMap = new Dictionary<InputAction, int>();
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        TryBind();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            UnBind();
            Instance = null;
        }
    }

    private void OnEnable()
    {
        TryBind();

        EnableActions(true);
    }

    private void OnDisable()
    {
        EnableActions(false);
    }

    // 바인드 시도
    private void TryBind()
    {
        // 중복 방지
        if (_isReady) return;

        if (_move != null && _move.action != null)
        {
            _move.action.performed += OnMovePerformed;
            _move.action.canceled += OnMoveCanceled;
        }

        if (_skills.Length > 0)
        {
            for (int i = 0; i < _skills.Length; i++)
            {
                if (_skills[i] != null && _skills[i].action != null)
                {
                    _skillIndexMap[_skills[i].action] = i;
                    _skills[i].action.performed += OnSkillPerformed;
                }
            }
        }

        if (_items.Length > 0)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i] != null && _items[i].action != null)
                {
                    _itemIndexMap[_items[i].action] = i;
                    _items[i].action.performed += OnItemPerformed;
                }
            }
        }

        if (_option != null && _option.action != null)
        {
            _option.action.performed += OnOptionPerformed;
        }

        if (_shop != null && _shop.action != null)
        {
            _shop.action.performed += OnShopPerformed;
        }

        if (_inventory != null && _inventory.action != null)
        {
            _inventory.action.performed += OnInventoryPerformed;
        }

        if (_skillTree != null && _skillTree.action != null)
        {
            _skillTree.action.performed += OnSkillTreePerformed;
        }

        _isReady = true;

        if (_logInput)
        {
            CDebug.Log("바인드 완료");
        }
    }

    // 바인드 해제
    private void UnBind()
    {
        if (!_isReady) return;

        if (_move != null && _move.action != null)
        {
            _move.action.performed -= OnMovePerformed;
            _move.action.canceled -= OnMoveCanceled;
        }

        if (_skills.Length > 0)
        {
            for (int i = 0; i < _skills.Length; i++)
            {
                if (_skills[i] != null && _skills[i].action != null)
                {
                    _skills[i].action.performed -= OnSkillPerformed;
                }
            }
            _skillIndexMap.Clear();
        }

        if (_items.Length > 0)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i] != null && _items[i].action != null)
                {
                    _items[i].action.performed -= OnItemPerformed;
                }
            }
            _itemIndexMap.Clear();
        }

        if (_option != null && _option.action != null)
        {
            _option.action.performed -= OnOptionPerformed;
        }

        if (_shop != null && _shop.action != null)
        {
            _shop.action.performed -= OnShopPerformed;
        }

        if (_inventory != null && _inventory.action != null)
        {
            _inventory.action.performed -= OnInventoryPerformed;
        }

        if (_skillTree != null && _skillTree.action != null)
        {
            _skillTree.action.performed -= OnSkillTreePerformed;
        }

        _isReady = false;

        if (_logInput)
        {
            CDebug.Log("바인드 해제 완료");
        }
    }

    private void EnableActions(bool enable)
    {
        if (!_isReady) return;

        if (enable)
        {    
            foreach (var skill in _skills)
            {
                if (skill != null && skill.action != null)
                {
                    skill.action.Enable();
                }
            }

            foreach (var item in _items)
            {
                if (item != null && item.action != null)
                {
                    item.action.Enable();
                }
            }

            if (_move != null && _move.action != null) _move.action.Enable();
            if (_option != null && _option.action != null) _option.action.Enable();
            if (_shop != null && _shop.action != null) _shop.action.Enable();
            if (_inventory != null && _inventory.action != null) _inventory.action.Enable();
            if (_skillTree != null && _skillTree.action != null) _skillTree.action.Enable();
        }
        else
        {
            foreach (var skill in _skills)
            {
                if (skill != null && skill.action != null)
                {
                    skill.action.Disable();
                }
            }

            foreach (var item in _items)
            {
                if (item != null && item.action != null)
                {
                    item.action.Disable();
                }
            }

            if (_move != null && _move.action != null) _move.action.Disable();
            if (_option != null && _option.action != null) _option.action.Disable();
            if (_shop != null && _shop.action != null) _shop.action.Disable();
            if (_inventory != null && _inventory.action != null) _inventory.action.Disable();
            if (_skillTree != null && _skillTree.action != null) _skillTree.action.Disable();
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 v = ctx.ReadValue<Vector2>();

        if (_logInput)
        {
            CDebug.Log($"이동 입력 {v}");
        }

        OnMove?.Invoke(v);
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        if (_logInput)
        {
            CDebug.Log("입력 취소");
        }

        OnMove?.Invoke(Vector2.zero);
    }

    private void OnSkillPerformed(InputAction.CallbackContext ctx)
    {
        if (_skillIndexMap.TryGetValue(ctx.action, out int index))
        {
            OnSkill?.Invoke(index);
            
            if (_logInput)
            {
                CDebug.Log($"{index + 1}번 슬롯 스킬 사용");
            }
        }
    }

    private void OnItemPerformed(InputAction.CallbackContext ctx)
    {
        if (_itemIndexMap.TryGetValue(ctx.action, out int index))
        {
            OnItemUse?.Invoke(index);

            if (_logInput)
            {
                CDebug.Log($"{index + 1}번 아이템 슬롯 사용");
            }
        }
    }

    private void OnOptionPerformed(InputAction.CallbackContext ctx) => OnOption?.Invoke();
    private void OnShopPerformed(InputAction.CallbackContext ctx) => OnShop?.Invoke();
    private void OnInventoryPerformed(InputAction.CallbackContext ctx) => OnInventory?.Invoke();
    private void OnSkillTreePerformed(InputAction.CallbackContext ctx) => OnSkillTree?.Invoke();
}
