using System;
using System.Collections;
using FishNet.Object;
using UnityEngine;

namespace Player.Items.StationaryObjects.SmokeDetector
{
    public class SmokeDetector : StationaryObjectBase
    {
        [SerializeField] private float audioTime;
        [SerializeField] private float audioDelay;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private AudioSource audioSrc;

        [SerializeField] private GameObject lightEffect;
        [SerializeField] private float regularBlinkDelay=1;

        private float lightDelay = 1;
        Coroutine coroutine;

        public override void OnStartClient()
        {
            base.OnStartClient();
            StartCoroutine(LightBlinker());
        }

        IEnumerator LightBlinker()
        {
            lightDelay = regularBlinkDelay;
            while (true)
            {
                lightEffect.SetActive(true);
                yield return new WaitForSeconds(0.25f);
                lightEffect.SetActive(false);
                
                
                yield return new WaitForSeconds(lightDelay);
            }
        }
        
        protected override void OnServerActivateStationaryObject()
        {
            //we don't care about the dissipation system
            Debug.Log($"SMOKE DETECTOR:: SMOKE SMOKE SMOKE!!!");
            NoiseManager.Instance.GenerateNoise(transform.position, 1,1);
        }

        protected override void OnClientActivateStationaryObject()
        {
            Debug.Log($"SMOKE DETECTOR:: SMOKE SMOKE SMOKE!!!");
            PlaySmokeDetectorAudio();
        }

        [Client]
        private void PlaySmokeDetectorAudio()
        {
            if(coroutine != null)
                StopCoroutine(coroutine);
            coroutine = StartCoroutine(SmokeDetectorAudio());
        }

        IEnumerator SmokeDetectorAudio()
        {
            float time = Time.time;
            float endTime = Time.time + audioTime;
            
            lightDelay = audioDelay;
            while (time < endTime)
            {
                audioSrc.PlayOneShot(audioClip);
                yield return new WaitForSeconds(audioDelay);
                time = Time.time;
            }
            lightDelay = regularBlinkDelay;
        }
    }
}