using FishNet.Demo.AdditiveScenes;
using UnityEngine;

public abstract class InteractiveItem : Item
{
    private InputSphereCollider inputSphere;
    private GameObject player;

    private void Awake()
    {
        inputSphere = GetComponentInChildren<InputSphereCollider>();
        
        // Subscribe to events
        inputSphere.onPlayerEnter.AddListener(OnSphereEntered);
        inputSphere.onPlayerExit.AddListener(OnSphereExited);
    }

    private void OnSphereEntered(GameObject player)
    {
        Debug.Log("Player entered interaction area.");
        this.player = player;
        //InputReader.Instance.OnGrabEvent += HandleGrab;  
    }

    private void OnSphereExited() 
    {
        Debug.Log("Player exited interaction area.");
        this.player = null;
        //InputReader.Instance.OnGrabEvent -= HandleGrab;  
    }

    private void OnDestroy()
    {
        if (inputSphere != null)
        {
            inputSphere.onPlayerEnter.RemoveListener(OnSphereEntered);
            inputSphere.onPlayerExit.RemoveListener(OnSphereExited);
        }
        //InputReader.Instance.OnGrabEvent -= HandleGrab; 
    }

    //grab behaviour (x to grab): can only pick up one item at a time, it goes on the side of your screen, if you go 
    //into placement mode you can see where you can place the object (is placement mode needed to place the object, let's say yes for now) 
    //@Skye 
    //TODO: move to the player 
    /*private void HandleGrab()
    {
        if (PlayerController.grabbedObject == null)
        {
            this.transform.SetParent(player.GetComponent<PlayerController>().grabPlacement);
            this.transform.localPosition = Vector3.zero;
            GetComponentInChildren<InputSphereCollider>().gameObject.SetActive(false);
        }
        else
        {
            //make them switch later on
            Debug.Log("The object are switched");
        }
    }*/
}