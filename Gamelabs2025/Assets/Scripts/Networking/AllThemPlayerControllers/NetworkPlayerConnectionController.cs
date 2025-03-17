using System;
using FishNet.Object;
using Items.Interfaces;
using UnityEngine;

namespace Networking
{
    public class NetworkPlayerConnectionController : NetworkBehaviour
    {
        private GameObject lookingAtObject = null;
        private HiderLookManager hiderLookManager;
        private bool isInConnectionMode = false;
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
                Debug.Log("You are now in connection mode");
            }
            
        }

        public void Update()
        { 
            if (hiderLookManager.GetCurrentLookTarget()?.GetComponent<IConnectable>() != null)
            {
                lookingAtObject = hiderLookManager.GetCurrentLookTarget(); 
            }
        }
    }

}

