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
        private NetworkObject lookingAtObject = null;
        private NetworkObject connectedToObject = null;
        
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

        void Client_DestroyRope(NetworkObject objectToConnectTo)
        {
            if(objectToConnectTo == null) return;
            
            var connectable = objectToConnectTo.GetComponent<IConnectable>();
            
            if (connectable == null || connectable.rope == null) return; 
            
            //Destroy the rope
            if(connectable.rope == null)
                Destroy(connectable.rope.gameObject);
                
            //get the rope endpoint and delete its rope reference 
            Rope rope = connectable.rope;
            if (rope == null) return;
            if (rope.EndPoint != null)
            {
                var eConnectable = rope.EndPoint.gameObject.GetComponent<IConnectable>();
                if (eConnectable != null)
                {
                    eConnectable.rope = null;
                }
            }

            if (rope.StartPoint != null)
            {
                var sConnectable = rope.StartPoint.gameObject.GetComponent<IConnectable>();
                if (sConnectable != null)
                {
                    sConnectable.rope = null;
                }
            }
        }
        public void Client_CreateNewRopeAndDestroyOldOne(NetworkObject objectToConnectTo)
        {
            // First Clear the Connections
            if (connectedToObject != null)
            {
                ITriggerItem trigger = null;
                IReactionItem reaction = null; 
                if (connectedToObjectIsATrigger)
                {
                    trigger = connectedToObject.GetComponent<ITriggerItem>();
                    reaction = objectToConnectTo.GetComponent<IReactionItem>();
                }
                else
                {
                    trigger = objectToConnectTo.GetComponent<ITriggerItem>();
                    reaction = connectedToObject.GetComponent<IReactionItem>();
                }
                
                ClearTriggerReactionEvents(trigger, reaction);
            }
            
            if (objectToConnectTo == null)
            {
                Debug.Log("No object to connect to");
                return; 
            }
            isInConnectionMode = true;
            var connectable = objectToConnectTo.GetComponent<IConnectable>();
            //remove any previous rope 
            if (connectable != null && connectable.rope != null)
            {
                Client_DestroyRope(objectToConnectTo);
            }
                
            //we want to create a rope from the object to the ghost 
            currentRope = Rope.CreateRope(ropePrefab, objectToConnectTo.transform, this.wireGrab);
            objectToConnectTo.GetComponent<IConnectable>().rope = currentRope;
            
            connectedToObjectIsATrigger = lookingAtObjectIsATrigger; 
            Debug.Log("You are now in connection mode");
        }

        private void TripWireConnectionCondition()
        {
            if (lookingAtObject == null) return;
            
            TripWirePole tripWirePole = lookingAtObject.GetComponent<TripWirePole>();
            if (tripWirePole != null && tripWirePole.isConnectedToAnotherPole)
            {
                //check if you have a connected pole 
                TripWirePole connectedPole = tripWirePole.connectedPole;
                if(connectedPole != null &&  connectedPole.rope != null)
                {
                    Client_DestroyRope(connectedPole.GetComponent<NetworkObject>());
                    Debug.Log($":::TripWireConnectionCondition:::");
                    RPC_InformServerOnRopeCreate(connectedPole.GetComponent<NetworkObject>(), false);
                }
            }
        }
        
        public void OnConnectButtonPressed()
        {
            Debug.Log("Pressed Connect Button");
            //this is the first object you connect to, connect the rope from the object to you 
            if (!isInConnectionMode)
            {
                //hack for trip wire 
                TripWireConnectionCondition();
                if (lookingAtObject != null)
                {
                    Client_CreateNewRopeAndDestroyOldOne(lookingAtObject); 
                    connectedToObject = lookingAtObject;
                    Debug.Log($":::OnConnectButtonPressed::: CONMODE: {isInConnectionMode} {connectedToObject.name}");
                    RPC_InformServerOnRopeCreate(lookingAtObject,true);
                }
            }
            else
            {
                isInConnectionMode = false;
                //not looking at a relevant object, cancel the connection
                if (!lookingAtObject)
                {
                    //TODO: Fix NULL ERROR!!!
                    //Honestly not needed, since the cables don't appear at realtime.
                    //RPC_InformServerOnRopeCreate(lookingAtObject,false); --- you are passing null here
                    if(currentRope != null)
                        Destroy(currentRope.gameObject); 
                }
                else
                {
                    //connected two items together 
                    if(currentRope)
                        currentRope.SetEndPoint(lookingAtObject.transform);
                    
                    RPC_InformServerOnRopeAttach(connectedToObject,lookingAtObject, connectedToObjectIsATrigger);

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
                    
                    MakeConnection(trigger, reaction);
                }
            }
        }

        private void Local_ConnectTwoObjects(NetworkObject objectToConnectTo,NetworkObject secondObject, bool firstIsTrigger)
        {
            //connected two items together 

            Debug.Log($"Local_ConnectTwoObjects (Server={IsServerStarted} : {currentRope == null}");
            if (IsClientStarted && currentRope != null)
            {
                currentRope.SetEndPoint(secondObject.transform);
                secondObject.GetComponent<IConnectable>().rope = currentRope;
            }

            ITriggerItem trigger = null;
            IReactionItem reaction = null; 
            // subscribe to the trigger's event
            if (firstIsTrigger)
            {
                trigger = objectToConnectTo.GetComponent<ITriggerItem>();
                reaction = secondObject.GetComponent<IReactionItem>();
            }
            else
            {
                trigger = secondObject.GetComponent<ITriggerItem>();
                reaction = objectToConnectTo.GetComponent<IReactionItem>();
            }
                    
            Debug.Log($"Local_ConnectTwoObjects {objectToConnectTo.name} is connected to {secondObject.name} {firstIsTrigger}");
            Debug.Log($"Local_ConnectTwoObjects Trigger=Null? {trigger == null} is connected to Reacion=Null? {reaction==null}");
            if (trigger != null && reaction != null)
            {
                MakeConnection(trigger, reaction);
            }
        }

        public void Update()
        { 
            if (hiderLookManager.GetCurrentLookTarget()?.GetComponent<IConnectable>() != null)
            {
                lookingAtObject = hiderLookManager.GetCurrentLookTarget().GetComponent<NetworkObject>();
                //check if its a trigger or a reaction object 
                if (lookingAtObject != null && lookingAtObject.GetComponent<ITriggerItem>() != null)
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
                        InScreenUI.Instance.ShowInputPrompt(InputReader.Instance.inputMap.Gameplay.ConnectItems, "Connect");

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
                    InScreenUI.Instance.ShowInputPrompt(InputReader.Instance.inputMap.Gameplay.ConnectItems, "Cancel Connection");

                }
            }
        }
        
        
        [ServerRpc]
        private void RPC_InformServerOnRopeCreate(NetworkObject objectToConnectTo, bool creation)
        {
            Debug.Log($"RPC_InformServerOnRopeCreate {creation} {objectToConnectTo.name}");
            RPC_BroadcastRopeCreateToClients(objectToConnectTo,creation);
        }
        
        [ObserversRpc(ExcludeOwner = true)]
        void RPC_BroadcastRopeCreateToClients(NetworkObject objectToConnectTo, bool creation)
        {
            // All clients except the owner will create the rope locally
            Debug.Log($"BroadcastRopeCreateToClients {creation} {objectToConnectTo.name}");
            if(creation)
                Client_CreateNewRopeAndDestroyOldOne(objectToConnectTo);
            else
                Client_DestroyRope(objectToConnectTo);
        }

        [ServerRpc]
        private void RPC_InformServerOnRopeAttach(NetworkObject objectToConnectTo, NetworkObject secondObject, bool connectedToObjectIsATrigger)
        {
            Debug.Log($"RPC_InformServerOnRopeAttach {objectToConnectTo.name} -- {secondObject.name} {connectedToObjectIsATrigger}");
            Local_ConnectTwoObjects(objectToConnectTo,secondObject, connectedToObjectIsATrigger);
            RPC_BroadcastRopeConnectToClients(objectToConnectTo, secondObject, connectedToObjectIsATrigger);
        }
        
        [ObserversRpc(ExcludeOwner = true)]
        private void RPC_BroadcastRopeConnectToClients(NetworkObject objectToConnectTo, NetworkObject secondObject, bool connectedToObjectIsATrigger)
        {
            Local_ConnectTwoObjects(objectToConnectTo,secondObject, connectedToObjectIsATrigger);
        }

        private void Client_DestroyRope()
        {
            Destroy(currentRope.gameObject); 
        }
        
        private static void ClearTriggerReactionEvents(ITriggerItem trigger, IReactionItem reaction)
        {
            if (trigger != null)
            {
                var connectedReaction = ConnectionDictionary.GetConnectedReactions(trigger);
                if (connectedReaction != null)
                {
                    trigger.OnTriggerActivated -= connectedReaction.OnTrigger;
                }
                ConnectionDictionary.RemoveConnections(trigger);
            }

            if (reaction != null) {
                var connectedTriggers = ConnectionDictionary.GetConnectedTriggers(reaction);
                if (connectedTriggers != null)
                {
                    foreach (var connectedTrigger in connectedTriggers)
                    {
                        connectedTrigger.OnTriggerActivated -= reaction.OnTrigger;
                        ConnectionDictionary.RemoveConnections(trigger);
                    }
                }
            }
        }
        
        private static void MakeConnection(ITriggerItem trigger, IReactionItem reaction)
        {
            if (trigger != null && reaction != null)
            {
                ClearTriggerReactionEvents(trigger, reaction);
                ConnectionDictionary.AddConnections(trigger, reaction);
                trigger.OnTriggerActivated += reaction.OnTrigger;
            }
        }
    }
}

