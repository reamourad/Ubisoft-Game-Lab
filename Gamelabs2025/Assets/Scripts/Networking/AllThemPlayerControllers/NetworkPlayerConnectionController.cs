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
        private HiderLookManager hiderLookManager;
        private bool isInConnectionMode = false;
        private bool isATrigger = false;
        [SerializeField] private GameObject ropePrefab;
        [SerializeField] private Transform wireGrab; 
        
        //when looking at a trigger or reaction object it should say "Press A to grab a wire"
        //Once you pressed A, the rope should follow you around 
        //when you look at the contrary item, it should say "Press A to connect" 
        //you can press A anytime to cancel the connection 

        void Start()
        {
            hiderLookManager = GetComponent<HiderLookManager>();
        }
        
        
        public void OnConnectButtonPressed()
        {
            if (lookingAtObject == null) { return; }
            
            //this is the first object you connect to, connect the rope from the object to you 
            if (!isInConnectionMode)
            {
                isInConnectionMode = true;
                //we want to create a rope from the object to the ghost 
                Rope.CreateRope(ropePrefab, lookingAtObject.transform, this.wireGrab);
                Debug.Log("You are now in connection mode");
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
                    isATrigger = true;
                }
                else
                {
                    isATrigger = false;
                }
            }
            else
            {
                lookingAtObject = null;
            }
        }
    }

}

