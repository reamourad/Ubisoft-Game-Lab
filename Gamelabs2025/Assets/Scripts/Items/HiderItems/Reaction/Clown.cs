using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FishNet.Object;
using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

namespace Items.HiderItems.Reaction
{
    public class Clown : NetworkBehaviour, IReactionItem, IHiderGrabableItem
    {
        [SerializeField] private Transform clown, door, handle;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private GameObject confettiFx;
        [SerializeField] private AudioClip clip;

        private bool triggered;
        public Rope rope { get; set; }
        public void OnTrigger(ITriggerItem triggerItem)
        {
            OnServerTrigger();
        }
        
        [Server]
        private void OnServerTrigger()
        {
            if (triggered) return;
            RPC_OnClientTrigger();
            triggered = true;
        }

        private void Update()
        {
            PlayHandleAnimation();
        }

        [ObserversRpc]
        private void RPC_OnClientTrigger()
        {
            if(triggered) return;
            PlayParticle();
            PlayTriggerAnimation();
            PlaySound();
            triggered = true;
        }

        private void PlayParticle()
        {
            var effect = Instantiate(confettiFx, transform.position, Quaternion.identity);
            Destroy(effect, 3);
        }

        private void PlayHandleAnimation()
        {
            if(triggered) return;
            handle.Rotate(100 * Time.deltaTime, 0, 0);
        }

        private void PlayTriggerAnimation()
        {
            door.DORotate(new Vector3(-90,0,0), 0.5f).OnComplete(() =>
            {
                // clown.DOPunchPosition(Vector3.up , 1f, 3, 10, true);
                clown.DOMove(transform.position + Vector3.up * .3f, .25f);
            });
        }

        private void PlaySound()
        {
            if(!clip) return;
            audioSource.PlayOneShot(clip);
        }
    }
}
