using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CInputDispatcher : MonoBehaviour
{
    #region 인스펙터
    [Header("액션 참조")]
    [SerializeField] private InputActionReference _move;
    [SerializeField] private InputActionReference[] _skills = new InputActionReference[3];
    [SerializeField] private InputActionReference _option;
    [SerializeField] private InputActionReference _shop;
    [SerializeField] private InputActionReference _inventory;

    [Header("디버깅")]
    [SerializeField] private bool _logInput = true;
    #endregion

    #region 내부 변수
    public static CInputDispatcher Instance { get; private set; }
    public event Action<Vector2> OnMove;
    public event Action<int> OnSkill;
    public event Action OnOption;
    public event Action OnShop;
    public event Action OnInventory;

    // 중복 방지용 변수
    private bool _isReady = false;
    // 스킬 인덱스 저장
    private Dictionary<InputAction, int> _skillIndexMap = new Dictionary<InputAction, int>();
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

        _isReady = true;

        if (_logInput)
        {
            Debug.Log("바인드 완료");
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

        _isReady = false;

        if (_logInput)
        {
            Debug.Log("바인드 해제 완료");
        }
    }

    private void EnableActions(bool enable)
    {
        if (!_isReady) return;

        if (enable)
        {
            _move.action.Enable();
            
            foreach (var skill in _skills)
            {
                if (skill != null && skill.action != null)
                {
                    skill.action.Enable();
                }
            }

            _option.action.Enable();
            _shop.action.Enable();
            _inventory.action.Enable();
        }
        else
        {
            _move.action.Disable();

            foreach (var skill in _skills)
            {
                if (skill != null && skill.action != null)
                {
                    skill.action.Disable();
                }
            }

            _option.action.Disable();
            _shop.action.Disable();
            _inventory.action.Disable();
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 v = ctx.ReadValue<Vector2>();

        if (_logInput)
        {
            Debug.Log($"이동 입력 {v}");
        }

        OnMove?.Invoke(v);
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        if (_logInput)
        {
            Debug.Log("입력 취소");
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
                Debug.Log($"{index + 1}번 슬롯 스킬 사용");
            }
        }
    }

    private void OnOptionPerformed(InputAction.CallbackContext ctx) => OnOption?.Invoke();
    private void OnShopPerformed(InputAction.CallbackContext ctx) => OnShop?.Invoke();
    private void OnInventoryPerformed(InputAction.CallbackContext ctx) => OnInventory?.Invoke();
}
