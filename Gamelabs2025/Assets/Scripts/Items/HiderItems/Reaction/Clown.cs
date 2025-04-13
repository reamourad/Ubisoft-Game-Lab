using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FishNet.Object;
using GogoGaga.OptimizedRopesAndCables;
using Player.Items;
using UnityEngine;

namespace Items.HiderItems.Reaction
{
    public class Clown : DetectableObject, IReactionItem, IHiderGrabableItem
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
            triggered = true;
            NoiseManager.Instance.GenerateNoise(transform.position, 0.55f,1);
            RPC_OnClientTrigger();
        }

        private void Update()
        {
            PlayHandleAnimation();
        }

        [ObserversRpc]
        private void RPC_OnClientTrigger()
        {
            if(triggered) return;
            triggered = true;
            PlayParticle();
            PlayTriggerAnimation();
            PlaySound();
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

        private TweenerCore<Quaternion, Vector3, QuaternionOptions> topTween;
        private TweenerCore<Vector3, Vector3, VectorOptions> clownTween;
        private void PlayTriggerAnimation()
        {
            topTween= door.DORotate(new Vector3(-90, 0, 0), 0.5f);
            topTween.OnComplete(() =>
            {
                clownTween = clown.DOMove(transform.position + Vector3.up * .3f, .25f).OnComplete(ServerDespawn);
            });
        }

        [Server]
        private void ServerDespawn()
        {
            StartCoroutine(DelayedDespawn());
        }
        
        [Server]
        private IEnumerator DelayedDespawn()
        {
            yield return new WaitForSeconds(0.3f);
            Despawn();
        }
        
        private void OnDisable()
        {
            DisableRope();
            KillTweens();
        }

        private void OnDestroy()
        {
            DisableRope();
            KillTweens();
        }

        private void DisableRope()
        {
            if (rope != null)
            {
                rope.gameObject.SetActive(false);
            }
        }

        private void KillTweens()
        {
            DOTween.Kill(topTween);
            if(clownTween == null) return;
            DOTween.Kill(clownTween);
        }

        private void PlaySound()
        {
            if(!clip) return;
            audioSource.PlayOneShot(clip);
        }
    }
}
