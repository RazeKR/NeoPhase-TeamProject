using UnityEngine;

public class CSpinLaserActionNode : CNode
{
    private ILaserCaster _boss;
    private float _duration;
    private bool _hasStarted = false;

    public CSpinLaserActionNode(ILaserCaster boss, float duration)
    {
        _boss = boss;
        _duration = duration;
    }

    public override ENodeState Evaluate()
    {
        if (!_hasStarted)
        {
            _boss.FireSpinLaser(_duration);
            _hasStarted = true;

            StopBossMovement();

            State = ENodeState.Running;
            return State;
        }

        if (_boss.IsFiringLaser)
        {
            StopBossMovement();

            State = ENodeState.Running;
            return State;
        }

        _hasStarted = false;
        State = ENodeState.Success;
        return State;
    }

    private void StopBossMovement()
    {
        if (_boss is CBossBase bossBase)
        {
            bossBase.Rb.velocity = Vector2.zero;
        }
    }
}
