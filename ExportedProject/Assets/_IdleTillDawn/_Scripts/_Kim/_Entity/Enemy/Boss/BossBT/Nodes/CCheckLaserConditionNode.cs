using UnityEngine;

public class CCheckLaserConditionNode : CNode
{
    private ILaserCaster _boss;

    public CCheckLaserConditionNode(ILaserCaster boss)
    {
        _boss = boss;
    }

    public override ENodeState Evaluate()
    {
        if (_boss.IsFiringLaser) return ENodeState.Failure;

        if (_boss.CheckLaserCooldown())
        {
            State = ENodeState.Success;
            return State;
        }

        State = ENodeState.Failure;
        return State;
    }
}
