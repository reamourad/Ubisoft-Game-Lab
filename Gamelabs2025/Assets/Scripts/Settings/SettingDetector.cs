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
        }

        [SerializeField] private TMPro.TextMeshProUGUI settingValue;
        [SerializeField] SettingType settingType = SettingType.None;

        private void Start()
        {
            ReadData();
            GetComponentInChildren<Slider>()?.onValueChanged.AddListener(UpdateSliderValue);
            GetComponentInChildren<Toggle>()?.onValueChanged.AddListener(UpdateToggleValue);
        }

        private void ReadData()
        {
            switch (settingType)
            {
                case SettingType.Bloom:
                    GetComponentInChildren<Toggle>().isOn = GameSettings.Settings.BloomEnabled;
                    break;
                case SettingType.Fog:
                    GetComponentInChildren<Toggle>().isOn = GameSettings.Settings.VolumetricFog;
                    break;
                case SettingType.DepthOfField:
                    GetComponentInChildren<Toggle>().isOn = GameSettings.Settings.DepthOfFieldEnabled;
                    break;
                case SettingType.MasterVolume:
                    GetComponentInChildren<Slider>().value = GameSettings.Settings.MasterVolume*100;
                    break;
                case SettingType.MusicVolume:
                    GetComponentInChildren<Slider>().value = GameSettings.Settings.MusicVolume*100;
                    break;
                case SettingType.SfxVolume:
                    GetComponentInChildren<Slider>().value = GameSettings.Settings.SfxVolume*100;
                    break;
                case SettingType.AmbienceVolume:
                    GetComponentInChildren<Slider>().value = GameSettings.Settings.AmbianceVolume*100;
                    break;
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
            }

            settingValue.text = ((int)value * 100).ToString();
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