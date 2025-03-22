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
        public bool isInConnectionMode = false;
        private bool lookingAtObjectIsATrigger = false;
        private bool connectedToObjectIsATrigger = false;
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
        
        
        public void OnConnectButtonPressed()
        {
            
            Debug.Log("Pressed Connect Button");
            //this is the first object you connect to, connect the rope from the object to you 
            if (!isInConnectionMode)
            {
                if (lookingAtObject == null) { return; }
                isInConnectionMode = true;
                //we want to create a rope from the object to the ghost 
                //TODO: we can make this a bool and check if it created a rope
                currentRope = Rope.CreateRope(ropePrefab, lookingAtObject.transform, this.wireGrab);
                connectedToObjectIsATrigger = lookingAtObjectIsATrigger; 
                Debug.Log("You are now in connection mode");
            }
            else
            {
                Debug.Log("Currently in Connection Mode");
                //not looking at a relevant object 
                if (!lookingAtObject)
                {
                    isInConnectionMode = false;
                    Destroy(currentRope.gameObject); 
                    Debug.Log("You are now in normal mode");
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
                    if ((connectedToObjectIsATrigger && !lookingAtObjectIsATrigger) || (!connectedToObjectIsATrigger && lookingAtObjectIsATrigger))
                    {
                        InScreenUI.Instance.SetToolTipText("Press " + 
                                                           InputReader.GetCurrentBindingText(InputReader.Instance.inputMap.Gameplay.ConnectItems) 
                                                           + " to connect"); 
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
    }

}

