using System;
using UnityEngine;

namespace Player.Settings
{
    public class WorldSfxAudioSettingUpdater : MonoBehaviour
    {
        
        AudioSource audioSource;
        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            GameSettings.OnUpdated += Settings_OnUpdated;
            Settings_OnUpdated();
        }

        private void OnDestroy()
        {
            GameSettings.OnUpdated -= Settings_OnUpdated;
        }

        private void Settings_OnUpdated()
        {
            audioSource.volume = GameSettings.Settings.MasterVolume * GameSettings.Settings.SfxVolume;
        }
    }
}