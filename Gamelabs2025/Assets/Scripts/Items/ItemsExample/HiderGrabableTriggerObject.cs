using UnityEngine;

public class HiderGrabableTriggerObject : MonoBehaviour, IHiderGrabableItem, ITriggerItem
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Connect(Transform target)
    {
        throw new System.NotImplementedException();
    }

    public void Connect(IReactionItem reactionItem)
    {
        throw new System.NotImplementedException();
    }
}
