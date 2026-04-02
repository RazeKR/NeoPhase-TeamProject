using System;

public class CActionDelegateNode : CNode
{
    private Func<ENodeState> _action;

    public CActionDelegateNode(Func<ENodeState> action)
    {
        _action = action;
    }

    public override ENodeState Evaluate()
    {
        if (_action != null)
        {
            State = _action.Invoke();
            return State;
        }

        State = ENodeState.Failure;
        return State;
    }
}
