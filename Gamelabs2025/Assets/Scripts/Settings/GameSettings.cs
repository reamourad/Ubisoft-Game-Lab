using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Player.Settings
{
    public static class GameSettings
    {
        [System.Serializable]
        public class SettingsData
        {
            public const string PREF_KEY = "Game_Settings";
            //Post-Processing
            public bool BloomEnabled=true;
            public bool VolumetricFog=true;
            public bool DepthOfFieldEnabled=true;
            
            //Audio
            public float MasterVolume=1;
            public float MusicVolume=1;
            public float SfxVolume=1;
            public float AmbianceVolume=1;
            
            //Gameplay
            public float SeekerLook = 1;
            public float HiderLook = 1;
        }
        
        public static Action OnUpdated;
        
        private static SettingsData settingsData=null;
        public static SettingsData Settings
        {
            get
            {
                if (settingsData == default)
                {
                    settingsData = LoadSettings();
                }
                return settingsData;
            }
        }
        
        private static SettingsData LoadSettings()
        {
            var data = PlayerPrefs.GetString(SettingsData.PREF_KEY, "");
            if (string.IsNullOrEmpty(data))
            {
                return new SettingsData();
            }
            return JsonConvert.DeserializeObject<SettingsData>(data);
        }

        public static void SaveData()
        {
            var json = JsonConvert.SerializeObject(settingsData);
            PlayerPrefs.SetString(SettingsData.PREF_KEY, json);
            PlayerPrefs.Save();
        }
    }
}