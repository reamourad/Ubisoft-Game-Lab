using UnityEngine;

public class HiderGrabableReactionItem : MonoBehaviour, IHiderGrabableItem, IReactionItem
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Transform WireAnchor { get; }
    public void OnTrigger()
    {
        throw new System.NotImplementedException();
    }
}
