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

public abstract class BTAction : IBTNode
{
    protected readonly BTBlackboard bb;

    public BTAction(BTBlackboard blackboard)
    {
        this.bb = blackboard;
    }

    public abstract BTStatus Update();
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
}

public abstract class BTCondition : IBTNode
{
    protected readonly BTBlackboard bb;

    public BTCondition(BTBlackboard blackboard)
    {
        this.bb = blackboard;
    }

    public abstract bool Check();

    public BTStatus Update() => Check() ? BTStatus.Success : BTStatus.Failure;
    public void OnEnter() { }
    public void OnExit() { }
}

public class BTSequence : IBTNode
{
    private List<IBTNode> children = new List<IBTNode>();
    private int currentChildIndex = 0;
    private bool isRunning = false;

    public BTSequence(params IBTNode[] nodes) => children.AddRange(nodes);

    public BTStatus Update()
    {
        // First run - start the sequence
        if (!isRunning)
        {
            isRunning = true;
            currentChildIndex = 0;
            children[currentChildIndex].OnEnter();
        }

        if (currentChildIndex >= children.Count)
            return BTStatus.Success;

        var status = children[currentChildIndex].Update();

        switch (status)
        {
            case BTStatus.Success:
                children[currentChildIndex].OnExit();
                currentChildIndex++;

                if (currentChildIndex < children.Count)
                {
                    children[currentChildIndex].OnEnter();
                    return BTStatus.Running;
                }
                else
                {
                    isRunning = false;
                    return BTStatus.Success;
                }

            case BTStatus.Failure:
                children[currentChildIndex].OnExit();
                isRunning = false;
                currentChildIndex = 0;
                return BTStatus.Failure;

            case BTStatus.Running:
            default:
                return BTStatus.Running;
        }
    }

    public void OnEnter() { }
    public void OnExit()
    {
        if (isRunning && currentChildIndex < children.Count)
        {
            children[currentChildIndex].OnExit();
        }
        isRunning = false;
        currentChildIndex = 0;
    }
}

public class BTSelector : IBTNode
{
    private readonly List<IBTNode> children = new List<IBTNode>();
    private int currentChildIndex;
    private bool isChildRunning;

    public BTSelector(params IBTNode[] nodes) => children.AddRange(nodes);

    public BTStatus Update()
    {
        // First run initialization
        if (!isChildRunning)
        {
            StartChild(0);
        }

        // Validate current child
        if (currentChildIndex >= children.Count)
            return BTStatus.Failure;

        // Execute current child
        var status = children[currentChildIndex].Update();

        switch (status)
        {
            case BTStatus.Success:
                StopCurrentChild();
                return BTStatus.Success;

            case BTStatus.Failure:
                StopCurrentChild();
                return TryNextChild();

            case BTStatus.Running:
            default:
                return BTStatus.Running;
        }
    }

    public void OnEnter()
    {
        // Reset state but don't start children yet
        currentChildIndex = 0;
        isChildRunning = false;
    }

    public void OnExit()
    {
        // Cleanup if a child was running
        if (isChildRunning)
        {
            children[currentChildIndex].OnExit();
            isChildRunning = false;
        }
    }

    private void StartChild(int index)
    {
        currentChildIndex = index;
        children[currentChildIndex].OnEnter();
        isChildRunning = true;
    }

    private void StopCurrentChild()
    {
        if (isChildRunning)
        {
            children[currentChildIndex].OnExit();
            isChildRunning = false;
        }
    }

    private BTStatus TryNextChild()
    {
        int nextIndex = currentChildIndex + 1;

        if (nextIndex < children.Count)
        {
            StartChild(nextIndex);
            return BTStatus.Running;
        }

        return BTStatus.Failure;
    }
}

// Decorator nodes
public class BTInverter : IBTNode
{
    private readonly IBTNode child;
    private bool isRunning;

    public BTInverter(IBTNode node) => child = node;

    public BTStatus Update()
    {
        if (!isRunning)
        {
            child.OnEnter();
            isRunning = true;
        }

        var status = child.Update();

        if (status != BTStatus.Running)
        {
            isRunning = false;
            return InvertStatus(status);
        }

        return BTStatus.Running;
    }

    public void OnEnter() => isRunning = false; // Reset on parent re-entry
    public void OnExit()
    {
        if (isRunning)
        {
            child.OnExit();
            isRunning = false;
        }
    }

    private BTStatus InvertStatus(BTStatus status) =>
        status == BTStatus.Success ? BTStatus.Failure : BTStatus.Success;
}

public class MonitoredActionNode : IBTNode
{
    private readonly IBTNode action;
    private readonly BTCondition condition;
    private bool isRunning;

    public MonitoredActionNode(IBTNode action, BTCondition condition)
    {
        this.action = action;
        this.condition = condition;
    }

    public BTStatus Update()
    {
        // Check condition first
        if (!condition.Check())
        {
            if (isRunning) action.OnExit();
            isRunning = false;
            return BTStatus.Failure;
        }

        // Start action if not running
        if (!isRunning)
        {
            action.OnEnter();
            isRunning = true;
        }

        // Run action
        return action.Update();
    }

    public void OnEnter() => isRunning = false; // Reset on parent re-entry
    public void OnExit()
    {
        if (isRunning)
        {
            action.OnExit();
            isRunning = false;
        }
    }
}

public class BTRepeat : IBTNode
{
    private readonly IBTNode child;
    private bool needsRestart;

    public BTRepeat(IBTNode node) => child = node;

    public BTStatus Update()
    {
        // Handle first run or restart
        if (needsRestart)
        {
            child.OnEnter();
            needsRestart = false;
        }

        var status = child.Update();

        // Handle completion
        if (status != BTStatus.Running)
        {
            child.OnExit();
            needsRestart = true;
        }

        return BTStatus.Running;
    }

    public void OnEnter() => needsRestart = true;
    public void OnExit()
    {
        if (!needsRestart) // If currently running
        {
            child.OnExit();
            needsRestart = true;
        }
    }
}

// Blackboard for shared data
public class BTBlackboard
{
    private Dictionary<string, object> data = new Dictionary<string, object>();

    public T Get<T>(string key) => data.ContainsKey(key) ? (T)data[key] : default;
    public void Set<T>(string key, T value) => data[key] = value;
    public bool Has(string key) => data.ContainsKey(key);
}