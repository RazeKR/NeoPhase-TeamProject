using System.Collections.Generic;

public class CSequence : CNode
{
    public CSequence(List<CNode> children) : base(children) { }

    public override ENodeState Evaluate()
    {
        bool anyChildIsRunning = false;

        foreach (CNode node in children)
        {
            switch (node.Evaluate())
            {
                case ENodeState.Failure:
                    State = ENodeState.Failure;
                    return State;
                case ENodeState.Success:
                    continue;
                case ENodeState.Running:
                    anyChildIsRunning = true;
                    continue;
                default:
                    State = ENodeState.Success;
                    return State;
            }
        }

        State = anyChildIsRunning ? ENodeState.Running : ENodeState.Success;
        return State;
    }
}
