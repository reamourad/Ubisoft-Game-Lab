using System;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Settings
{
    public class SettingDetector : MonoBehaviour
    {
        public enum SettingType
        {
            None,
            Bloom,
            Fog,
            DepthOfField,
            MasterVolume,
            MusicVolume,
            SfxVolume,
            AmbienceVolume,
            //Gameplay
            SeekerLook,
            HiderLook
        }

        [SerializeField] private TMPro.TextMeshProUGUI settingValue;
        [SerializeField] SettingType settingType = SettingType.None;
        public float sliderChange = 1;
        
        private Slider slider;
        private Toggle toggle;
        
        private void Start()
        {
            slider = GetComponentInChildren<Slider>();
            toggle = GetComponentInChildren<Toggle>();
            ReadData();
            slider?.onValueChanged.AddListener(UpdateSliderValue);
            toggle?.onValueChanged.AddListener(UpdateToggleValue);
        }

        private void ReadData()
        {
            switch (settingType)
            {
                case SettingType.Bloom:
                    toggle.isOn = GameSettings.Settings.BloomEnabled;
                    break;
                case SettingType.Fog:
                    toggle.isOn = GameSettings.Settings.VolumetricFog;
                    break;
                case SettingType.DepthOfField:
                    toggle.isOn = GameSettings.Settings.DepthOfFieldEnabled;
                    break;
                case SettingType.MasterVolume:
                    slider.value = GameSettings.Settings.MasterVolume*100;
                    break;
                case SettingType.MusicVolume:
                    slider.value = GameSettings.Settings.MusicVolume*100;
                    break;
                case SettingType.SfxVolume:
                    slider.value = GameSettings.Settings.SfxVolume*100;
                    break;
                case SettingType.AmbienceVolume:
                    slider.value = GameSettings.Settings.AmbianceVolume*100;
                    break;
                case SettingType.SeekerLook:
                    slider.value = GameSettings.Settings.SeekerLook;
                    break;
                case SettingType.HiderLook:
                    slider.value = GameSettings.Settings.HiderLook;
                    break;
            }

            if (slider != null)
            {
                if (settingType == SettingType.SeekerLook || settingType == SettingType.HiderLook)
                {
                    settingValue.text = slider.value.ToString("0.00");
                }
                else
                    settingValue.text = $"{slider.value}";
            }
                
        }

        void UpdateSliderValue(float value)
        {
            switch (settingType)
            {
                case SettingType.MasterVolume:
                    GameSettings.Settings.MasterVolume = value/100;
                    break;
                case SettingType.MusicVolume:
                    GameSettings.Settings.MusicVolume = value/100;
                    break;
                case SettingType.SfxVolume:
                    GameSettings.Settings.SfxVolume = value/100;
                    break;
                case SettingType.AmbienceVolume:
                    GameSettings.Settings.AmbianceVolume = value/100;
                    break;
                case SettingType.SeekerLook:
                    GameSettings.Settings.SeekerLook = value;
                    break;
                case SettingType.HiderLook:
                    GameSettings.Settings.HiderLook = value;
                    break;
            }
            
            if (slider != null)
            {
                if (settingType == SettingType.SeekerLook || settingType == SettingType.HiderLook)
                {
                    settingValue.text = slider.value.ToString("0.00");
                }
                else
                    settingValue.text = $"{slider.value}";
            }
            GameSettings.OnUpdated?.Invoke();
        }

        void UpdateToggleValue(bool value)
        {
            switch (settingType)
            {
                case SettingType.Bloom:
                    GameSettings.Settings.BloomEnabled = value;
                    break;
                case SettingType.Fog:
                    GameSettings.Settings.VolumetricFog = value;
                    break;
                case SettingType.DepthOfField:
                    GameSettings.Settings.DepthOfFieldEnabled = value;
                    break;
            }
            GameSettings.OnUpdated?.Invoke();
        }
    }
}