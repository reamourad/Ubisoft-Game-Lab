using System;
using System.Collections;
using FishNet.Managing;
using Networking;
using Player.Audio;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace StateManagement
{
    public class MainMenuController : MonoBehaviour
    {
        public enum MainMenuState
        {
            None=0,
            MenuStart,
            MainMenu,
            SettingsMenu,
            QuitMenu
        }
        
        [SerializeField] private MainMenuState currentMainMenuState = MainMenuState.None;
        [SerializeField] private GameObject pressAnyKeyObj;
        [SerializeField] private GameObject mainMenuObj;
        [SerializeField] private GameObject settingsObj;
        [SerializeField] private GameObject quitConfirmObj;
        [SerializeField] private GameObject waitGuiObj;

        [Header("Connection")]
        [SerializeField] private GameObject fetchUI;
        [SerializeField] private GameObject fetchFailUI;
        [SerializeField] private TMPro.TMP_Text versionText;
        
        [Header("MainMenu References")] [SerializeField]
        private GameObject mainMenuCam;
        
        [SerializeField] private Toggle[] mainMenuToggles;
        
        public AudioClip menuMusic;
        public AudioClip navigationSound;
        public AudioClip selectSound;
        
        private int currSelIndex = 0;
        private bool started = false;

        private void OnEnable()
        {
            InputReader.Instance.OnMainMenuStart += OnMenuStart;
            InputReader.Instance.OnMainMenuNavigationUpDown += OnMenuMainMenuNavigate;
            
            if(started)
                InputReader.Instance.OnMainMenuAccept += OnSelectItem;
            
            InputReader.Instance.OnMainMenuBack += OnQuit;
        }

        private void OnDisable()
        {
            InputReader.Instance.OnMainMenuStart -= OnMenuStart;
            InputReader.Instance.OnMainMenuNavigationUpDown -= OnMenuMainMenuNavigate;
            InputReader.Instance.OnMainMenuAccept -= OnSelectItem;
            InputReader.Instance.OnMainMenuBack -= OnQuit;
        }

        private IEnumerator Start()
        {
            if (Keyboard.current != null)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }

            yield return StartCoroutine(Initialise());
            AudioManager.Instance.PlayBG(menuMusic);
            SwitchMenu(MainMenuState.MenuStart);
            UpdateSelection();
            
            Application.targetFrameRate = 60;
        }

        IEnumerator Initialise()
        {
#if !UNITY_SERVER
            fetchUI.gameObject.SetActive(true);
            var res = NetworkConnectionHelper.PullConnectionInfo();
            yield return new WaitUntil(() => res.IsCompleted);
            fetchUI.gameObject.SetActive(false);
            if (!NetworkConnectionHelper.ConnectionInfoAvailable)
            {
                fetchFailUI.gameObject.SetActive(true);
                yield break;
            }
            
            
            var clientVersion = Application.version;
            var serverVersion = NetworkConnectionHelper.RemoteServerVersion;
            versionText.text = $"Remote server version: {serverVersion}\n" +
                               $"Client version: {clientVersion}";

            if (clientVersion != serverVersion)
            {
                Debug.LogWarning("ClientConnectionUI::Remote Server and Client version mismatch, Features/Systems may break due to a different version.");
            }
            
            InputReader.Instance.SetToUIInputs();
#else
            yield return null;
#endif
        }
        
        private void OnQuit()
        {
            if(currentMainMenuState != MainMenuState.MainMenu)
                return;
            
            SwitchMenu(MainMenuState.QuitMenu);
            AudioManager.Instance.PlaySFX(selectSound);
        }

        private void OnSelectItem()
        {
            if(currentMainMenuState != MainMenuState.MainMenu)
                return;
            
            switch (currSelIndex)
            {
                case 0: StartOnlinePlay();
                    break;
                case 1: SwitchMenu(MainMenuState.SettingsMenu);
                    break;
                case 2: SwitchMenu(MainMenuState.QuitMenu);
                    break;
            }
            AudioManager.Instance.PlaySFX(selectSound);
        }

        private void StartOnlinePlay()
        {
            if(!started)
                return;
            
            if(currentMainMenuState != MainMenuState.MainMenu)
                return;
            
            InputReader.Instance.SetToGameplayInputs();
            NetworkConnectionHelper.ConnectToRemoteServer();
            ShowWaitGui();
            Debug.Log("WTF");
        }

        private void ShowWaitGui()
        {
            waitGuiObj.gameObject.SetActive(true);
        }

        private void OnMenuStart()
        {
            if(currentMainMenuState != MainMenuState.MenuStart)
                return;
            SwitchMenu(MainMenuState.MainMenu);
            if (!started)
            {
                mainMenuCam.SetActive(true);
                StartCoroutine(WaitAndEnableMenuSelection());
                started = true;
            }
        }

        IEnumerator WaitAndEnableMenuSelection()
        {
            yield return new WaitForEndOfFrame();
            InputReader.Instance.OnMainMenuAccept += OnSelectItem;
        }

        private void OnMenuMainMenuNavigate(float value)
        {
            if (value > 0)
            {
                currSelIndex -= 1;
                if(currSelIndex < 0)
                    currSelIndex = mainMenuToggles.Length-1;
            }
            else if (value < 0)
            {
                currSelIndex = (currSelIndex + 1) % mainMenuToggles.Length;
            }

            UpdateSelection();
        }

        private void UpdateSelection()
        {
            mainMenuToggles[currSelIndex].isOn = !mainMenuToggles[currSelIndex].isOn;
            AudioManager.Instance.PlaySFX(navigationSound);
        }
        
        public void SwitchMenu(MainMenuState newMainMenuState)
        {
            currentMainMenuState = newMainMenuState;
            UpdateMenuState();
        }

        private void UpdateMenuState()
        {
            pressAnyKeyObj.gameObject.SetActive(currentMainMenuState == MainMenuState.MenuStart);
            mainMenuObj.gameObject.SetActive(currentMainMenuState == MainMenuState.MainMenu);
            settingsObj.gameObject.SetActive(currentMainMenuState == MainMenuState.SettingsMenu);
            quitConfirmObj.gameObject.SetActive(currentMainMenuState == MainMenuState.QuitMenu);
        }
        
        
    }
}