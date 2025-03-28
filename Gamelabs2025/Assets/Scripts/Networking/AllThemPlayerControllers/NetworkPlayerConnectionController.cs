using System;
using FishNet.Object;
using GogoGaga.OptimizedRopesAndCables;
using Items.Interfaces;
using UnityEngine;

namespace Networking
{
    [RequireComponent(typeof(HiderLookManager))]
    public class NetworkPlayerConnectionController : NetworkBehaviour
    {
        private GameObject lookingAtObject = null;
        private GameObject connectedToObject = null;
        
        private bool lookingAtObjectIsATrigger = false;
        private bool connectedToObjectIsATrigger = false;
        
        private HiderLookManager hiderLookManager;
        public bool isInConnectionMode = false;

        [SerializeField] private GameObject ropePrefab;
        [SerializeField] private Transform wireGrab;
        private Rope currentRope; 
        
        
        //when looking at a trigger or reaction object it should say "Press A to grab a wire"
        //Once you pressed A, the rope should follow you around 
        //when you look at the contrary item, it should say "Press A to connect" 
        //you can press A anytime to cancel the connection 

        void Start()
        {
            hiderLookManager = GetComponent<HiderLookManager>();
        }

        public void CreateNewRopeAndDestroyOldOne(Transform objectToConnectTo)
        {
            if (objectToConnectTo == null)
            {
                Debug.Log("No object to connect to");
                return; 
            }
            isInConnectionMode = true;
            //remove any previous rope 
            if (objectToConnectTo.GetComponent<IConnectable>().rope != null)
            {
                //get the rope endpoint and delete its rope reference 
                Rope rope = objectToConnectTo.GetComponent<IConnectable>().rope;
                rope.EndPoint.gameObject.GetComponent<IConnectable>().rope = null;
                rope.StartPoint.gameObject.GetComponent<IConnectable>().rope = null;
                
                //Destroy the rope 
                Destroy(objectToConnectTo.GetComponent<IConnectable>().rope.gameObject);
            }
                
            //we want to create a rope from the object to the ghost 
            currentRope = Rope.CreateRope(ropePrefab, objectToConnectTo.transform, this.wireGrab);
            objectToConnectTo.GetComponent<IConnectable>().rope = currentRope;
            
            connectedToObjectIsATrigger = lookingAtObjectIsATrigger; 
            Debug.Log("You are now in connection mode");
        }
        
        
        public void OnConnectButtonPressed()
        {
            
            Debug.Log("Pressed Connect Button");
            //this is the first object you connect to, connect the rope from the object to you 
            if (!isInConnectionMode)
            {
               CreateNewRopeAndDestroyOldOne(lookingAtObject.transform); 
               connectedToObject = lookingAtObject;
               RPC_InformServerOnRopeCreate(lookingAtObject.transform);
            }
            else
            {
                isInConnectionMode = false;
                //not looking at a relevant object, cancel the connection
                if (!lookingAtObject)
                {
                    Destroy(currentRope.gameObject); 
                }
                else
                {
                    //connected two items together 
                    currentRope.SetEndPoint(lookingAtObject.transform);

                    ITriggerItem trigger = null;
                    IReactionItem reaction = null; 
                    lookingAtObject.GetComponent<IConnectable>().rope = currentRope;
                    // subscribe to the trigger's event
                    if (connectedToObjectIsATrigger)
                    {
                        trigger = connectedToObject.GetComponent<ITriggerItem>();
                        reaction = lookingAtObject.GetComponent<IReactionItem>();
                    }
                    else
                    {
                        trigger = lookingAtObject.GetComponent<ITriggerItem>();
                        reaction = connectedToObject.GetComponent<IReactionItem>();
                    }
                    
                    trigger.OnTriggerActivated += (t) => reaction.OnTrigger(t);
                }
            }
        }

        public void Update()
        { 
            if (hiderLookManager.GetCurrentLookTarget()?.GetComponent<IConnectable>() != null)
            {
                lookingAtObject = hiderLookManager.GetCurrentLookTarget();
                //check if its a trigger or a reaction object 
                if (lookingAtObject.GetComponent<ITriggerItem>() != null)
                {
                    lookingAtObjectIsATrigger = true;
                }
                else
                {
                    lookingAtObjectIsATrigger = false;
                }
                
                //if you are in connection mode, you need to check if you can connect to that item
                if (isInConnectionMode)
                {
                    if ((connectedToObjectIsATrigger && !lookingAtObjectIsATrigger) ||
                        (!connectedToObjectIsATrigger && lookingAtObjectIsATrigger))
                    {
                        InScreenUI.Instance.SetToolTipText("Press " +
                                                           InputReader.GetCurrentBindingText(InputReader.Instance
                                                               .inputMap.Gameplay.ConnectItems)
                                                           + " to connect");
                    }
                    else
                    {
                        lookingAtObject = null;
                    }
                }
            }
            else
            {
                lookingAtObject = null;
                if (isInConnectionMode)
                {
                    InScreenUI.Instance.SetToolTipText("Press " + 
                                                       InputReader.GetCurrentBindingText(InputReader.Instance.inputMap.Gameplay.ConnectItems) 
                                                       + " to cancel connection");
                }
            }
        }
        
        
        [ServerRpc]
        private void RPC_InformServerOnRopeCreate(Transform objectToConnectTo)
        {
            Debug.Log("Received rope creation request from client");
        
            // Server creates the rope
            //CreateNewRopeAndDestroyOldOne(objectToConnectTo);
        
            // Tell all other clients to create the rope as well
            BroadcastRopeCreateToClients(objectToConnectTo);
        }
        
        [ObserversRpc(ExcludeOwner = true)]
        void BroadcastRopeCreateToClients(Transform objectToConnectTo)
        {
            // All clients except the owner will create the rope locally
            CreateNewRopeAndDestroyOldOne(objectToConnectTo);
        }
    }
}

