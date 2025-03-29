using System;
using System.Collections;
using FishNet.Object;
using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

namespace Player.Items.HiderItems
{
    public class PressurePlates : NetworkBehaviour, ITriggerItem, IHiderGrabableItem
    {
        
        [SerializeField] private Animator animator;
        [SerializeField] private float activationDelay=0.25f;
        public Rope rope { get; set; }
        public event Action<ITriggerItem> OnTriggerActivated;
        
        private Coroutine activationRoutine;
        
        [Server]
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"PressurePlate:: {other.name} entered");
            var role = other.GetComponentInParent<PlayerRole>();
            if(role == null)
                return;
            if (role.Role == PlayerRole.RoleType.Seeker)
            {
                activationRoutine = StartCoroutine(DelayedActivation());
                Debug.Log("PressurePlate::PRESSED");
            }
            animator.SetBool("pressed", true);
        }

        [Server]
        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"PressurePlate:: {other.name} exited");
            var role = other.GetComponentInParent<PlayerRole>();
            if(role == null)
                return;
            if (role.Role == PlayerRole.RoleType.Seeker)
            {
                StopCoroutine(activationRoutine);
                Debug.Log("PressurePlate::RELEASED");
            }
            animator.SetBool("pressed", false);
        }

        IEnumerator DelayedActivation()
        {
           yield return new WaitForSeconds(activationDelay);
           OnTriggerActivated?.Invoke(this);
           Debug.Log($"PressurePlate::ACTIVATED CONNECTION {OnTriggerActivated != null}");
        }
        
    }
}