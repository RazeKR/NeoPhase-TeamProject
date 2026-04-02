using UnityEngine;

public class CStateAutoChase : IPlayerState
{
    private CPlayerController _player;
    private Transform _lastTarget;
    private bool _isApproaching = false;

    public CStateAutoChase(CPlayerController player)
    {
        _player = player;
    }

    public void Enter()
    {
        //Debug.Log("CStateAutoChase : Enter");
        _isApproaching = false;
        _lastTarget = null;
    }

    public void Exit()
    {
        //Debug.Log("CStateAutoChase : Exit");
        _player.Rb.velocity = Vector2.zero;
    }

    public void Update()
    {
        if (_player.InputHandler.IsManualMove)
        {
            _player.StateMachine.ChangeState(_player.StateManual);
        }
    }

    public void FixedUpdate()
    {
        Collider2D[] threats = Physics2D.OverlapCircleAll
        (
            _player.transform.position,
            _player.EvadeRadius,
            _player.HazardLayer
        );

        if (threats.Length > 0)
        {
            _player.StateMachine.ChangeState(_player.StateAutoEvade);
            return;
        }

        if (_player.CurrentTarget == null)
        {
            _isApproaching = false;
            _lastTarget = null;
            _player.Rb.velocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(_player.transform.position, _player.CurrentTarget.position);
        Vector2 dirToTarget = (_player.CurrentTarget.position - _player.transform.position).normalized;

        float maxAttackRange = _player.CurrentAttackRange;
        float stopApproachRange = maxAttackRange * 0.7f;

        if (_player.CurrentTarget != _lastTarget)
        {
            _isApproaching = distance > maxAttackRange;
            _lastTarget = _player.CurrentTarget;
        }
        else
        {
            if (distance > maxAttackRange) _isApproaching = true;
            else if (distance <= stopApproachRange) _isApproaching = false;
        }

        _player.Rb.velocity = _isApproaching ? (dirToTarget * _player.MoveSpeed) : Vector2.zero;
    }
}
