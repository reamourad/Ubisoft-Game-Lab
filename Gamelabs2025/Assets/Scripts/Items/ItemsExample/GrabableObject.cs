using UnityEngine;

public class GrabableObject : MonoBehaviour, IGrabableItem
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGrab(PlayerController grabber)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrop()
    {
        throw new System.NotImplementedException();
    }

    public void OnThrow()
    {
        throw new System.NotImplementedException();
    }
}
