using System;
using System.Collections;
using FishNet.Object;
using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

namespace Player.Items.HiderItems
{
    public class PressurePlates : NetworkObject, ITriggerItem, IHiderGrabableItem
    {
        
        [SerializeField] Animator animator;
        [SerializeField] float activationDelay=0.25f;
        public Rope rope { get; set; }
        public event Action<ITriggerItem> OnTriggerActivated;
        
        private Coroutine activationRoutine;
        
        [Server]
        private void OnTriggerEnter(Collider other)
        {
            var role = other.GetComponentInParent<PlayerRole>();
            if(role == null)
                return;
            if (role.Role == PlayerRole.RoleType.Seeker)
                activationRoutine = StartCoroutine(DelayedActivation());
        }

        [Server]
        private void OnTriggerExit(Collider other)
        {
            var role = other.GetComponentInParent<PlayerRole>();
            if(role == null)
                return;
            if (role.Role == PlayerRole.RoleType.Seeker)
                StopCoroutine(activationRoutine);
        }

        IEnumerator DelayedActivation()
        {
           yield return new WaitForSeconds(activationDelay);
           OnTriggerActivated?.Invoke(this);
        }
        
    }
}