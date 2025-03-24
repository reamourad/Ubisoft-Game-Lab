using Items.Interfaces;
using UnityEngine;

public interface IReactionItem : IConnectable
{
    public void OnTrigger(ITriggerItem triggerItem);
}
