using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Player.Settings
{
    public class PostProcessingSettingsUpdater : MonoBehaviour
    {
        Volume volume;
        private void Start()
        {
            volume = GetComponent<Volume>();
            GameSettings.OnUpdated  += OnSettingsUpdated;
            OnSettingsUpdated();
        }

        private void OnSettingsUpdated()
        {
            bool bloom = GameSettings.Settings.BloomEnabled;
            bool dof = GameSettings.Settings.DepthOfFieldEnabled;
            bool vf = GameSettings.Settings.VolumetricFog;
            
            var bloomComp = volume.profile.components.Find(a=> a is Bloom);
            var dofComp = volume.profile.components.Find(a=> a is DepthOfField);
            var volComp = volume.profile.components.Find(a=> a is VolumetricFogVolumeComponent);

            if (bloomComp != null)
                bloomComp.active = bloom;
            if(dofComp != null)
                dofComp.active = dof;
            if(volComp != null)
                volComp.active = vf;
        }
    }
}