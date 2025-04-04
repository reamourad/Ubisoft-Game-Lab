using System;
using System.Collections;
using FishNet.Object;
using GogoGaga.OptimizedRopesAndCables;
using Player.Audio;
using UnityEngine;

namespace Player.Items.HiderItems
{
    public class PressurePlates : NetworkBehaviour, ITriggerItem, IHiderGrabableItem
    {
        
        [SerializeField] private Animator animator;
        [SerializeField] private float activationDelay=0.25f;
        [SerializeField] private AudioClip pressSFX;
        [SerializeField] private AudioSource localSFXSource;
        public Rope rope { get; set; }
        public event Action<ITriggerItem> OnTriggerActivated;
        
        private Coroutine activationRoutine;
        
        [Server]
        private void OnTriggerEnter(Collider other)
        {
            if(activationRoutine != null)
                return;
            
            Debug.Log($"PressurePlate:: {other.name} entered");
            var role = other.GetComponentInParent<PlayerRole>();
            if(role == null)
                return;
            if (role.Role == PlayerRole.RoleType.Seeker)
            {
                activationRoutine = StartCoroutine(DelayedActivation());
                RPC_OnPlayerEntered();
                Debug.Log("PressurePlate::PRESSED");
            }
            animator.SetBool("pressed", true);
        }

        [ObserversRpc(ExcludeOwner = false)]
        private void RPC_OnPlayerEntered()
        {
            localSFXSource.PlayOneShot(pressSFX);
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
                if(activationRoutine != null)
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
           activationRoutine = null;
        }
        
    }
}