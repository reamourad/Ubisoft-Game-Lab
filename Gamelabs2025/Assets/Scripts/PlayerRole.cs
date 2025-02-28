using System;
using FishNet.Object;
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

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                GameLookupMemory.MyLocalPlayerRole = roleType;
            }
        }
    }
    
}