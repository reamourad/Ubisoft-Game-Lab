using System;
using FishNet.Object;
using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

//should always spawn in pairs 
public class TripWirePole :NetworkBehaviour, ITriggerItem, IHiderGrabableItem
{
    [SerializeField] private float connectionRange = 5f; 
    [SerializeField] private GameObject ropePrefab; 
    [SerializeField] private Transform ropeAttachPoint; 
    
    public TripWirePole connectedPole;
    private Rope ropeInstance;
    public bool isConnectedToAnotherPole = false;
    public bool isConnectedToAReactionItem = false;
    

    // Update is called once per frame
    void Update()
    {
        if (!isConnectedToAnotherPole)
        {
            //find nearby poles
            Collider[] colliders = Physics.OverlapSphere(transform.position, connectionRange);
            
            foreach (Collider col in colliders)
            {
                TripWirePole otherPole = col.GetComponent<TripWirePole>();
                
                //skip if it's not a pole or it's this pole
                if (otherPole == null || otherPole == this)
                    continue;
                    
                // Connect the poles
                ConnectPoles(otherPole);
                break;
            }
        }
        else if (connectedPole != null)
        {
            //check if poles are still in range
            float distance = Vector3.Distance(transform.position, connectedPole.transform.position);
            if (distance > connectionRange)
            {
                DisconnectPoles();
            }
        }

    }
    
    public void SetConnected(TripWirePole pole, Rope ropeObj)
    {
        connectedPole = pole;
        ropeInstance = ropeObj;
        isConnectedToAnotherPole = true;
        
        connectedPole.isConnectedToAnotherPole = true;
        connectedPole.ropeInstance =  ropeObj;
        connectedPole.connectedPole = this;
    }

    
    private void DisconnectPoles()
    {
        if (connectedPole != null)
        {
            connectedPole.isConnectedToAnotherPole = false;
            connectedPole.ropeInstance = null;
            connectedPole.connectedPole = null;
        }
        
        if (ropeInstance != null)
        {
            Destroy(ropeInstance.gameObject);
        }
        
        isConnectedToAnotherPole = false;
        ropeInstance = null;
        connectedPole = null;
    }

    
    private void ConnectPoles(TripWirePole otherPole)
    {
        if (isConnectedToAnotherPole || ropePrefab == null)
            return;

        if (otherPole.isConnectedToAnotherPole)
            return; 
        //create rope between poles
        ropeInstance = Rope.CreateRope(ropePrefab, ropeAttachPoint, otherPole.ropeAttachPoint);
        
        //set the rope instance poles
        ropeInstance.GetComponent<TripWireRope>().ClearConnectedPoles();
        ropeInstance.GetComponent<TripWireRope>().connectedPoles.Add(this);
        ropeInstance.GetComponent<TripWireRope>().connectedPoles.Add(otherPole);

        if (ropeInstance != null)
        {
            //mark as connected
            SetConnected(otherPole, ropeInstance);
            otherPole.SetConnected(this, ropeInstance);
        }
    }

    public void OnRopeTriggered()
    {
        Debug.Log("Rope triggered");
        OnTriggerActivated?.Invoke(this);
    }


    public Rope rope { get; set; }
    public event Action<ITriggerItem> OnTriggerActivated;
}