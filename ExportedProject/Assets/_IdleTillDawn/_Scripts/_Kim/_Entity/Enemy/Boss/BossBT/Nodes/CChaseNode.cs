using UnityEngine;

public class CChaseNode : CNode
{
    private CBossBase _boss;

    public CChaseNode(CBossBase boss)
    {
        _boss = boss;
    }

    public override ENodeState Evaluate()
    {
        if (_boss.CurrentTarget == null) return ENodeState.Failure;

        float distance = Vector2.Distance(_boss.transform.position, _boss.CurrentTarget.position);
        Vector2 dirToPlayer = (_boss.CurrentTarget.position - _boss.transform.position).normalized;

        if (distance > _boss.AttackRange * 0.8f)
        {
            _boss.Rb.velocity = dirToPlayer * _boss.CurrentMoveSpeed;

            _boss.FlipCharacter(dirToPlayer.x);
        }
        else
        {
            _boss.Rb.velocity = Vector2.zero;
        }

        State = ENodeState.Success;
        return State;
    }
}
