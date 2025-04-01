using UnityEngine;
using System.Collections.Generic;

// Status enum for behavior nodes
public enum BTStatus
{
    Success,
    Failure,
    Running
}

// Base interface for all behavior nodes
public interface IBTNode
{
    BTStatus Update();
    void OnEnter();
    void OnExit();
}

// Base class for action nodes
public abstract class BTAction : IBTNode
{
    public abstract BTStatus Update();
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
}

// Base class for condition nodes
public abstract class BTCondition : IBTNode
{
    public abstract bool Check();

    public BTStatus Update() => Check() ? BTStatus.Success : BTStatus.Failure;
    public void OnEnter() { }
    public void OnExit() { }
}

// Composite nodes
public class BTSequence : IBTNode
{
    private List<IBTNode> children = new List<IBTNode>();
    private int currentChildIndex = 0;

    public BTSequence(params IBTNode[] nodes) => children.AddRange(nodes);

    public BTStatus Update()
    {
        if (currentChildIndex >= children.Count)
            return BTStatus.Success;

        var status = children[currentChildIndex].Update();

        switch (status)
        {
            case BTStatus.Success:
                children[currentChildIndex].OnExit();
                currentChildIndex++;
                return currentChildIndex < children.Count ? BTStatus.Running : BTStatus.Success;

            case BTStatus.Failure:
                children[currentChildIndex].OnExit();
                currentChildIndex = 0;
                return BTStatus.Failure;

            case BTStatus.Running:
            default:
                return BTStatus.Running;
        }
    }

    public void OnEnter() => currentChildIndex = 0;
    public void OnExit() => currentChildIndex = 0;
}

public class BTSelector : IBTNode
{
    private List<IBTNode> children = new List<IBTNode>();
    private int currentChildIndex = 0;

    public BTSelector(params IBTNode[] nodes) => children.AddRange(nodes);

    public BTStatus Update()
    {
        if (currentChildIndex >= children.Count)
            return BTStatus.Failure;

        var status = children[currentChildIndex].Update();

        switch (status)
        {
            case BTStatus.Success:
                children[currentChildIndex].OnExit();
                currentChildIndex = 0;
                return BTStatus.Success;

            case BTStatus.Failure:
                children[currentChildIndex].OnExit();
                currentChildIndex++;
                return currentChildIndex < children.Count ? BTStatus.Running : BTStatus.Failure;

            case BTStatus.Running:
            default:
                return BTStatus.Running;
        }
    }

    public void OnEnter() => currentChildIndex = 0;
    public void OnExit() => currentChildIndex = 0;
}

// Decorator nodes
public class BTInverter : IBTNode
{
    private IBTNode child;

    public BTInverter(IBTNode node) => child = node;

    public BTStatus Update()
    {
        var status = child.Update();
        return status == BTStatus.Success ? BTStatus.Failure :
               status == BTStatus.Failure ? BTStatus.Success :
               BTStatus.Running;
    }

    public void OnEnter() => child.OnEnter();
    public void OnExit() => child.OnExit();
}

// Blackboard for shared data
public class BTBlackboard
{
    private Dictionary<string, object> data = new Dictionary<string, object>();

    public T Get<T>(string key) => data.ContainsKey(key) ? (T)data[key] : default;
    public void Set<T>(string key, T value) => data[key] = value;
    public bool Has(string key) => data.ContainsKey(key);
}