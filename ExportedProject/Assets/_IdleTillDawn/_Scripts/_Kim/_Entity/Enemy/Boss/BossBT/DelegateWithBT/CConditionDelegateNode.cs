using System;

public class CConditionDelegateNode : CNode
{
    private Func<bool> _condition;

    public CConditionDelegateNode(Func<bool> condition)
    {
        _condition = condition;
    }

    public override ENodeState Evaluate()
    {
        if (_condition != null && _condition.Invoke())
        {
            State = ENodeState.Success;
            return State;
        }

        State = ENodeState.Failure;
        return State;
    }
}
