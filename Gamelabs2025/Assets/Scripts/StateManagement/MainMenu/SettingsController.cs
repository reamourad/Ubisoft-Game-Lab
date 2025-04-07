using System;
using System.Collections;
using Player.Audio;
using Player.Settings;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace StateManagement
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private GameObject settingsCam;
        [SerializeField] private GameObject[] settingsPanelOptions;
        [SerializeField] private MainMenuController mainMenuController;
        private int currSelIndx = 0;
        private GameObject currentGO;
        private Coroutine sliderUpdateRoutine;
        
        private void OnEnable()
        {
            settingsCam.SetActive(true);
            InputReader.Instance.OnMainMenuNavigationUpDown += OnNavigateY;
            InputReader.Instance.OnMainMenuNavigationLeftRight += OnNavigateX;
            InputReader.Instance.OnMainMenuAccept += OnAccept;
            InputReader.Instance.OnMainMenuBack += OnCancel;
        }
        private void OnDisable()
        {
            settingsCam.SetActive(false);
            InputReader.Instance.OnMainMenuNavigationUpDown -= OnNavigateY;
            InputReader.Instance.OnMainMenuNavigationLeftRight -= OnNavigateX;
            InputReader.Instance.OnMainMenuAccept -= OnAccept;
            InputReader.Instance.OnMainMenuBack -= OnCancel;
            GameSettings.SaveData();
        }

        private void OnNavigateY(float yVal)
        {
            if (yVal > 0)
            {
                currSelIndx -= 1;
                if(currSelIndx < 0)
                    currSelIndx = settingsPanelOptions.Length-1;
            }
            else if (yVal < 0)
            {
                currSelIndx = (currSelIndx + 1) % settingsPanelOptions.Length;
            }

            UpdateSelection();
        }

        private void UpdateSelection()
        {
            if(currentGO != null)
                currentGO.GetComponent<Image>().enabled = false;
            
            currentGO = settingsPanelOptions[currSelIndx];
            currentGO.GetComponent<Image>().enabled = true;
            AudioManager.Instance.PlaySFX(mainMenuController.navigationSound);
        }
        
        private void OnNavigateX(float xVal, bool heldDown)
        {
            var go = settingsPanelOptions[currSelIndx];
            if (heldDown)
            {
                if (sliderUpdateRoutine != null)
                    return;
                
                sliderUpdateRoutine = StartCoroutine(SliderUpdater(go, xVal));
            }
            else
            {
                if(sliderUpdateRoutine != null)
                    StopCoroutine(sliderUpdateRoutine);
                sliderUpdateRoutine = null;
            }
        }

        IEnumerator SliderUpdater(GameObject go, float value)
        {
            var slider = go.GetComponentInChildren<Slider>();
            while (true)
            {
                slider.value += value;
                AudioManager.Instance.PlaySFX(mainMenuController.navigationSound);
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private void OnCancel()
        {
           mainMenuController.SwitchMenu(MainMenuController.MainMenuState.MainMenu);
           AudioManager.Instance.PlaySFX(mainMenuController.selectSound);
        }

        private void OnAccept()
        {
            var go = settingsPanelOptions[currSelIndx];
            var toggle = go.GetComponentInChildren<Toggle>();
            if (toggle != null)
            {
                toggle.isOn = !toggle.isOn;
                AudioManager.Instance.PlaySFX(mainMenuController.selectSound);
            }
        }
        
    }
}