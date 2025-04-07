using System;
using System.Collections;
using System.Collections.Generic;
using Player.Settings;
using UnityEngine;
using UnityEngine.Audio;
using Utils;

namespace Player.Audio
{
    public class AudioManager : SingletonBehaviour<AudioManager>
    {
        [SerializeField] private AudioSource bgSource;
        [SerializeField] private AudioSource ambianceSource;
        [SerializeField] private AudioSource sfSourceReference;
        [SerializeField] private AudioSource monSfSourceReference;
        
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private List<AudioSource> worldSources = new List<AudioSource>();
        

        private AudioClip currentBGClip;
        private float masterVolume=1;
        
        private void Awake()
        {
            if(Instance != null && Instance != this)
                Destroy(gameObject);
            
            OnSettingsUpdated();
        }

        private void Start()
        {
            GameSettings.OnUpdated += OnSettingsUpdated;
            DontDestroyOnLoad(this);
        }

        private void OnDestroy()
        {
            GameSettings.OnUpdated -= OnSettingsUpdated;
        }

        private void OnSettingsUpdated()
        {
            masterVolume = GameSettings.Settings.MasterVolume;
            bgSource.volume = masterVolume * GameSettings.Settings.MusicVolume;
            ambianceSource.volume = masterVolume * GameSettings.Settings.AmbianceVolume;
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
            var go = Instantiate(sfSourceReference.gameObject, this.transform);
            go.gameObject.SetActive(true);
            var sfx = go.GetComponent<AudioSource>();
            sfx.volume = masterVolume * GameSettings.Settings.SfxVolume;
            sfx.PlayOneShot(audioClip);
            Destroy(sfx.gameObject, audioClip.length + 1);
        }
        
        public void PlayMonsterSFX(AudioClip audioClip)
        {
            var go = Instantiate(monSfSourceReference.gameObject, this.transform);
            go.gameObject.SetActive(true);
            var sfx = go.GetComponent<AudioSource>();
            sfx.volume = masterVolume * GameSettings.Settings.SfxVolume;
            sfx.PlayOneShot(audioClip);
            Destroy(sfx.gameObject, audioClip.length + 1);
        }

        public void PlayAmbience(AudioClip audioClip)
        {
            ambianceSource.clip = audioClip;
            ambianceSource.Play();
        }
    }
}