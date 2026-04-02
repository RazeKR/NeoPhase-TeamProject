using System.Collections.Generic;

public class CSelector : CNode
{
    public CSelector(List<CNode> children) : base(children) { }

    public override ENodeState Evaluate()
    {
        foreach (CNode node in children)
        {
            switch (node.Evaluate())
            {
                case ENodeState.Failure:
                    continue;
                case ENodeState.Success:
                    State = ENodeState.Success;
                    return State;
                case ENodeState.Running:
                    State = ENodeState.Running;
                    return State;
                default:
                    continue;
            }
        }

        State = ENodeState.Failure;
        return State;
    }
}
