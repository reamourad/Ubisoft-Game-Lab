using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using StateManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerRole : NetworkBehaviour
    {
        public enum RoleType
        {
            None = 0,
            Seeker,
            Hider
        }
        
        [SerializeField] private RoleType roleType;
        public RoleType Role => roleType;

        [SerializeField] private GameObject seekerGraphicHack;

        private Rigidbody rb;
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                GameLookupMemory.MyLocalPlayerRole = roleType;
            }
        }

        private void LateUpdate()
        {
            Hack();
        }

        void Hack()
        {
            if(seekerGraphicHack != null && seekerGraphicHack.transform.localRotation != Quaternion.identity)
                seekerGraphicHack.transform.localRotation = Quaternion.identity;
        }

        public Vector3 GetCentrofMassPosition()
        {
            if(rb == null)
                rb = GetComponent<Rigidbody>();
            
            return transform.position + rb.centerOfMass; 
        }
    }
    
}