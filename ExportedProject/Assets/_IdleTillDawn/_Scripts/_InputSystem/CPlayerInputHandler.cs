using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPlayerInputHandler : MonoBehaviour
{
    #region 인스펙터
    [Header("플레이어 설정")]
    [SerializeField] private float _moveSpeed = 6.0f;

    [Header("스킬 참조")]
    // 나중에 스킬 클래스 타입에 맞게 수정해야 함
    [SerializeField] private MonoBehaviour[] _equippedSkills = new MonoBehaviour[3];
    #endregion

    #region 내부 변수
    private Rigidbody2D _rb;
    private Coroutine _bindCo;
    private Vector2 _moveInput;
    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (_bindCo != null)
        {
            StopCoroutine(_bindCo);
        }

        _bindCo = StartCoroutine(CoBindDispatcher());
    }

    private void OnDisable()
    {
        if (_bindCo != null)
        {
            StopCoroutine(_bindCo);
        }

        _bindCo = null;

        if (CInputDispatcher.Instance != null)
        {
            CInputDispatcher.Instance.OnMove -= HandleMove;
        }
    }

    // 디스패쳐가 준비될 때까지 대기
    private IEnumerator CoBindDispatcher()
    {
        while (CInputDispatcher.Instance == null) yield return null;

        CInputDispatcher.Instance.OnMove += HandleMove;
        CInputDispatcher.Instance.OnSkill += HandleSkill;

        Debug.Log("CPlayerMoveHandler : 구독 완료");
    }

    void FixedUpdate()
    {
        UpdateMove(_moveInput);
    }

    // 이동 입력이 들어올 때 실행
    private void HandleMove(Vector2 v)
    {
        _moveInput = Vector2.ClampMagnitude(v, 1.0f);
    }

    private void UpdateMove(Vector2 input)
    {
        Vector2 velocity = input * _moveSpeed;

        _rb.velocity = velocity;
    }

    // 스킬 입력이 들어올 때 실행
    private void HandleSkill(int index)
    {
        if (index < 0 || index >= _equippedSkills.Length) return;

        var skillToUse = _equippedSkills[index];

        if (skillToUse != null)
        {
            // 스킬 사용 로직

            Debug.Log($"PlayerInputHandler {index + 1}번 슬롯 스킬 사용");
        }
        else
        {
            Debug.Log($"PlayerInputHandler {index + 1}번 슬롯 비어있음");
        }
    }
}
