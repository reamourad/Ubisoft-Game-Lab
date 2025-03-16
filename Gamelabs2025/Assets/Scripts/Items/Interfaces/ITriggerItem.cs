using UnityEngine;

public interface ITriggerItem
{
    public void Connect(Transform target);
    public void Connect(IReactionItem reactionItem);
}