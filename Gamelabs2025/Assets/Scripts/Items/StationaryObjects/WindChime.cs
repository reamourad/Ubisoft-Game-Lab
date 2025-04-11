using System.Collections.Generic;
using DG.Tweening;
using Player.Items;
using UnityEngine;

namespace Items.StationaryObjects
{
    public class WindChime : StationaryObjectBase
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Transform fxSpawnPoint;
        [SerializeField] private GameObject windChimeFx;
        [SerializeField] private List<Transform> pivots;
        [SerializeField] private AudioClip clip;

        private bool isTriggered;
        protected override void OnServerActivateStationaryObject()
        {
            if (isTriggered) return;
            NoiseManager.Instance?.GenerateNoise(transform.position, 1f, 0.5f);
            isTriggered = true;
        }

        protected override void OnClientActivateStationaryObject()
        {
            ShakeCylinders();
            PlayParticle();
            PlaySound();
        }

        private void ShakeCylinders()
        {
            foreach (var pivot in pivots)
            {
                pivot.DOShakeRotation(3, Vector3.one * 2 ,5)
                    .OnComplete(() => { pivot.DORotate(Vector3.zero, 0.5f); });
            }
        }

        private void PlayParticle()
        {
            var fx = Instantiate(windChimeFx, fxSpawnPoint.position, Quaternion.identity);
            Destroy(fx,4f);
        }

        private void PlaySound()
        {
            audioSource.PlayOneShot(clip);
        }
    }
}