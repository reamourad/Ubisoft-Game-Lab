using Items.Interfaces;
using UnityEngine;

public interface ITriggerItem : IConnectable
{
    public void Connect(Transform target);
    public void Connect(IReactionItem reactionItem);
}