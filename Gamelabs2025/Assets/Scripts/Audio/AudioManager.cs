using System;
using System.Collections;
using UnityEngine;
using Utils;

namespace Player.Audio
{
    public class AudioManager : SingletonBehaviour<AudioManager>
    {
        [SerializeField] private AudioSource bgSource;
        [SerializeField] private AudioSource ambianceSource;
        [SerializeField] private AudioSource sfSourceReference;

        private AudioClip currentBGClip;
        
        private void Awake()
        {
            if(Instance != null && Instance != this)
                Destroy(gameObject);
        }

        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        public void PlayBG(AudioClip audioClip, float fadeDur = 0.25f)
        {
            if(currentBGClip == audioClip)
                return;
            
            currentBGClip = audioClip;
            StartCoroutine(BGTransition(audioClip, fadeDur));
        }

        IEnumerator BGTransition(AudioClip audioClip, float fadeDur = 0.25f)
        {
            var old = bgSource;
            bgSource = Instantiate(old, this.transform);
            Destroy(old.gameObject, fadeDur);
            
            bgSource.clip = audioClip;
            bgSource.Play();
            
            float timeStep = 0;
            while (timeStep <= 1)
            {
                timeStep += Time.deltaTime/ fadeDur;
                bgSource.volume = Mathf.Lerp(0, 1, timeStep);
                if(old)
                    old.volume = Mathf.Lerp(1, 0, timeStep);
                yield return new WaitForEndOfFrame();
            }
        }
        
        public void PlaySFX(AudioClip audioClip)
        {
            var go = Instantiate(sfSourceReference.gameObject, sfSourceReference.transform);
            var sfx = go.GetComponent<AudioSource>();
            sfx.gameObject.SetActive(true);
            Destroy(sfx.gameObject, audioClip.length);
            sfx.PlayOneShot(audioClip);
        }

        public void PlayAmbience(AudioClip audioClip)
        {
            ambianceSource.clip = audioClip;
            ambianceSource.Play();
        }
    }
}