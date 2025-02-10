using UnityEngine;

public abstract class InteractiveItem : Item
{
    private InputSphereCollider inputSphere;

    private void Awake()
    {
        inputSphere = GetComponentInChildren<InputSphereCollider>();
        
        // Subscribe to events
        inputSphere.onPlayerEnter.AddListener(OnSphereEntered);
        inputSphere.onPlayerExit.AddListener(OnSphereExited);
    }

    private void OnSphereEntered()
    {
        Debug.Log("Player entered interaction area.");
        InputReader.Instance.OnGrabEvent += HandleGrab;  
    }

    private void OnSphereExited() 
    {
        Debug.Log("Player exited interaction area.");
        InputReader.Instance.OnGrabEvent -= HandleGrab;  
    }

    private void OnDestroy()
    {
        if (inputSphere != null)
        {
            inputSphere.onPlayerEnter.RemoveListener(OnSphereEntered);
            inputSphere.onPlayerExit.RemoveListener(OnSphereExited);
        }
        InputReader.Instance.OnGrabEvent -= HandleGrab; 
    }

    //grab behaviour (x to grab): can only pick up one item at a time, it goes on the side of your screen, if you go 
    //into placement mode you can see where you can place the object (is placement mode needed to place the object, let's say yes for now) 
    //@Skye 
    private void HandleGrab()
    {
        Debug.Log("The object is grabbed.");
        // Implement grab logic here
    }
}