using System.Collections;
using System.Collections.Generic;
using Player.Audio;
using Player.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace StateManagement
{
    public class SettingsTabController : MonoBehaviour
    {
        [SerializeField] private GameObject settingsCam;
        [SerializeField] private GameObject[] settingsPanelOptions;
        
        [SerializeField] private AudioClip navigateSound;
        [SerializeField] private AudioClip acceptSound;
        
        private int currSelIndx = 0;
        private GameObject currentGO;
        private Coroutine sliderUpdateRoutine;
        
        private void OnEnable()
        {
            InputReader.Instance.OnMainMenuNavigationUpDown += OnNavigateY;
            InputReader.Instance.OnMainMenuNavigationLeftRight += OnNavigateX;
            InputReader.Instance.OnMainMenuAccept += OnAccept;
        }
        private void OnDisable()
        {
            InputReader.Instance.OnMainMenuNavigationUpDown -= OnNavigateY;
            InputReader.Instance.OnMainMenuNavigationLeftRight -= OnNavigateX;
            InputReader.Instance.OnMainMenuAccept -= OnAccept;
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
            AudioManager.Instance.PlaySFX(navigateSound);
        }
        
        private void OnNavigateX(float xVal, bool heldDown)
        {
            var go = settingsPanelOptions[currSelIndx];
            if (heldDown)
            {
                if (sliderUpdateRoutine != null)
                    return;
                
                var goSettingComp = go.GetComponent<SettingDetector>();
                if(goSettingComp == null)
                    return;
                sliderUpdateRoutine = StartCoroutine(SliderUpdater(go, goSettingComp.sliderChange * xVal));
            }
            else
            {
                if(sliderUpdateRoutine != null)
                    StopCoroutine(sliderUpdateRoutine);
                sliderUpdateRoutine = null;
            }
        }

        IEnumerator SliderUpdater(GameObject go, float delta)
        {
            var slider = go.GetComponentInChildren<Slider>();
            if(slider == null)
                yield break;
            
            while (true)
            {
                slider.value += delta;
                AudioManager.Instance.PlaySFX(navigateSound);
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void OnAccept()
        {
            var go = settingsPanelOptions[currSelIndx];
            var toggle = go.GetComponentInChildren<Toggle>();
            if (toggle != null)
            {
                toggle.isOn = !toggle.isOn;
                AudioManager.Instance.PlaySFX(acceptSound);
            }
        }
    }
}