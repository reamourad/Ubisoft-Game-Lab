using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

public class HiderGrabableReactionItem : MonoBehaviour, IHiderGrabableItem, IReactionItem
{
    public Rope rope { get; set; }
    public void OnTrigger(ITriggerItem triggerItem)
    {
        Debug.Log("I reacted to the object trigger.");
    }
}
