using UnityEngine;

public class CCheckDashConditionNode : CNode
{
    private CBossBase _boss;

    public CCheckDashConditionNode(CBossBase boss)
    {
        _boss = boss;
    }

    public override ENodeState Evaluate()
    {
        if (_boss.CurrentTarget == null) return ENodeState.Failure;

        if (_boss.IsAttacking) return ENodeState.Success;

        float distance = Vector2.Distance(_boss.transform.position, _boss.CurrentTarget.position);

        bool isCooltimeReady = Time.time > _boss.LastAttackTime + _boss.AttackCooltime;
        bool isWithinRange = distance <= _boss.AttackRange;

        State = (isCooltimeReady && isWithinRange) ? ENodeState.Success : ENodeState.Failure;
        return State;
    }
}
