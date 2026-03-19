using System;
using System.Collections;
using UnityEngine;

public class CPlayerInputHandler : MonoBehaviour
{
    #region 내부 변수
    public Vector2 MoveInput { get; private set; }
    public event Action<int> OnSkillInput;
    public bool IsManualMove => MoveInput.sqrMagnitude > 0.001f;
    public bool CanControl { get; set; } = true;

    private Coroutine _bindCo;
    #endregion

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
            CInputDispatcher.Instance.OnSkill -= HandleSkill;
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

    // 이동 입력이 들어올 때 실행
    private void HandleMove(Vector2 v)
    {
        if (!CanControl)
        {
            MoveInput = Vector2.zero;
            return;
        }

        MoveInput = Vector2.ClampMagnitude(v, 1.0f);
    }

    // 스킬 입력이 들어올 때 실행
    private void HandleSkill(int index)
    {
        if (!CanControl) return;

        OnSkillInput?.Invoke(index);
    }
}
