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
            State = ENodeState.Running;
            return State;
        }

        if (_boss.IsFiringLaser)
        {
            State = ENodeState.Running;
            return State;
        }

        _hasStarted = false;
        State = ENodeState.Success;
        return State;
    }
}
