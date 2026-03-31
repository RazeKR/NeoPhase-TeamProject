using UnityEngine;

public class CDashActionNode : CNode
{
    private enum DashState
    {
        Telegraph,
        Dashing
    }
    private DashState _currentState = DashState.Telegraph;
    private float _timer;

    private CBossBase _boss;
    private Animator _animator;

    private float _prepareTime;
    private float _slidingTime;
    private float _dashForce;
    private int _dashLayer;
    private int _originLayer;

    public CDashActionNode(CBossBase boss, Animator animator, float prepareTime, float slidingTime, float dashForce, int dashLayer, int originLayer)
    {
        _boss = boss;
        _animator = animator;
        _prepareTime = prepareTime;
        _slidingTime = slidingTime;
        _dashForce = dashForce;
        _dashLayer = dashLayer;
        _originLayer = originLayer;
    }

    public override ENodeState Evaluate()
    {
        if (_currentState == DashState.Telegraph)
        {
            if (_timer == 0f)
            {
                _animator.SetTrigger("tAttack");
                _boss.IsAttacking = true;
            }

            _boss.Rb.velocity = Vector2.zero;
            _timer += Time.fixedDeltaTime;

            if (_timer >= _prepareTime)
            {
                _currentState = DashState.Dashing;
                _timer = 0f;

                SetLayerRecursively(_boss.gameObject, _dashLayer);

                Vector2 dashDir = (_boss.CurrentTarget.position - _boss.transform.position).normalized;
                _boss.Rb.AddForce(dashDir * _dashForce, ForceMode2D.Impulse);
            }
            State = ENodeState.Running;
            return State;
        }
        else if (_currentState == DashState.Dashing)
        {
            _timer += Time.fixedDeltaTime;

            if (_timer >= _slidingTime)
            {
                _boss.Rb.velocity = Vector2.zero;
                SetLayerRecursively(_boss.gameObject, _originLayer);

                _boss.LastAttackTime = Time.time;
                _boss.IsAttacking = false;

                _currentState = DashState.Telegraph;
                _timer = 0f;

                State = ENodeState.Success;
                return State;
            }
            State = ENodeState.Running;
            return State;
        }

        State = ENodeState.Failure;
        return State;
    }

    /// <summary>
    /// 자신을 포함한 모든 자식의 레이어를 바꾸는 재귀함수
    /// </summary>
    /// <param name="go"></param>
    /// <param name="newLayer"></param>
    public void SetLayerRecursively(GameObject go, int newLayer)
    {
        go.layer = newLayer;

        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
