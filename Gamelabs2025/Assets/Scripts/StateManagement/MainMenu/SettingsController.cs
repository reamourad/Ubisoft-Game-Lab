using System;
using System.Collections;
using Player.Audio;
using Player.Settings;
using StateManagement.MainMenu.UI;
using StateManagement.StateManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace StateManagement
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private AudioClip tabSwitchSFX;
        [SerializeField] private GameObject settingsCam;
        [SerializeField] private MainMenuController mainMenuController;
        [SerializeField] private GameObject[] tabs;
        
        [SerializeField] private bool IsInsidePauseMenu = false;
        
        private int currSelIndx = 0;
        private Coroutine sliderUpdateRoutine;
        
        GameObject currentTab;
        
        private void OnEnable()
        {
            if(settingsCam != null)
                settingsCam.SetActive(true);
            
            InputReader.Instance.OnSettingsTabSwitchEvent += OnNavigate;
            InputReader.Instance.OnMainMenuBack += OnCancel;
        }
        private void OnDisable()
        {
            if(settingsCam != null)
                settingsCam.SetActive(false);
            
            InputReader.Instance.OnSettingsTabSwitchEvent -= OnNavigate;
            InputReader.Instance.OnMainMenuBack -= OnCancel;
            GameSettings.SaveData();
        }

        private void OnNavigate(float obj)
        {
            var indx = currSelIndx;
            if (obj > 0)
                indx += 1;
            else if(obj < 0)
                indx -= 1;
            
            indx = Mathf.Clamp(indx, 0, tabs.Length - 1);
            if(indx == currSelIndx)
                return;
            
            currSelIndx = indx;
            if(currentTab != null)
                currentTab.GetComponent<Toggle>().isOn = false;
            
            currentTab = tabs[currSelIndx];
            currentTab.GetComponent<Toggle>().isOn = true;
            AudioManager.Instance.PlaySFX(UINavigationSounds.Instance.tabSwitchSFX);
        }

        private void OnCancel()
        {
            AudioManager.Instance.PlaySFX(UINavigationSounds.Instance.selectSound);
            if (IsInsidePauseMenu)
                PauseMenuController.ReturnToPauseMenu();
            else
                mainMenuController.SwitchMenu(MainMenuController.MainMenuState.MainMenu);
        }
        
    }
}