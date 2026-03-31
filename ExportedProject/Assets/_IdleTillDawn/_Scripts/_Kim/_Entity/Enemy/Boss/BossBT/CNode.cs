using System.Collections.Generic;

[System.Serializable]
public abstract class CNode
{
    public ENodeState State { get; protected set; }

    protected List<CNode> children = new List<CNode>();

    public CNode() { }
    public CNode(List<CNode> children)
    {
        this.children = children;
    }

    public abstract ENodeState Evaluate();
}
