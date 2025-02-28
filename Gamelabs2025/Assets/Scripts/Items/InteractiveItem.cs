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
        //Debug.Log("Player entered interactionsphere.");
        this.player = player;
        //InputReader.Instance.OnGrabEvent += HandleGrab;  
    }

    private void OnSphereExited() 
    {
        //Debug.Log("Player exited interactionsphere.");
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
        if(player == null){return;}
        
        if (PlayerController.grabbedObject == null)
        {
           SetUpGrab();
        }
        else
        {
            //Put the currently grabbed object to the new grabbed object 
            InteractiveItem previousObject = PlayerController.grabbedObject;
            previousObject.transform.SetParent(null);
            previousObject.transform.position = this.transform.position;
            previousObject.inputSphere.gameObject.SetActive(true);
    
            // Setup new object
            SetUpGrab();
            
            //Debug.Log("The object are switched");
        }
    }

    private void SetUpGrab()
    {
        PlayerController.grabbedObject = this;
        this.transform.position = player.GetComponent<PlayerController>().grabPlacement.position;
        this.transform.SetParent(player.GetComponent<PlayerController>().grabPlacement);
        inputSphere.gameObject.SetActive(false);
    }
}