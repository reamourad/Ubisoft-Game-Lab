using System;
using Player;
using Player.Settings;
using Unity.Cinemachine;
using UnityEngine;

namespace Utils
{
    public class LookSettingsRespecter : MonoBehaviour
    {
        [SerializeField] CinemachineInputAxisController axisController;
        [SerializeField] PlayerRole.RoleType role;

        private float startGainX;
        private float startGainY;
        
        private void Start()
        {
            axisController = GetComponent<CinemachineInputAxisController>();
            startGainX = axisController.Controllers[0].Input.Gain;
            startGainY = axisController.Controllers[1].Input.Gain;

            GameSettings.OnUpdated += OnUpdatedSettings;
            
            OnUpdatedSettings();
        }

        private void OnDestroy()
        {
            GameSettings.OnUpdated -= OnUpdatedSettings;
        }

        private void OnUpdatedSettings()
        {
            if(!axisController)
                return;
            
            if (role == PlayerRole.RoleType.Seeker)
            {
                axisController.Controllers[0].Input.Gain = startGainX * GameSettings.Settings.SeekerLook;
                axisController.Controllers[1].Input.Gain = startGainY * GameSettings.Settings.SeekerLook;
            }
            else if(role == PlayerRole.RoleType.Hider)
            {
                axisController.Controllers[0].Input.Gain = startGainX * GameSettings.Settings.HiderLook;
                axisController.Controllers[1].Input.Gain = startGainY * GameSettings.Settings.HiderLook;
            }
        }
        
    }
}