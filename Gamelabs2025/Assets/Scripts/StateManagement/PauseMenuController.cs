using Player.Audio;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StateManagement
{
    namespace StateManagement
{
    public class PauseMenuController : MonoBehaviour
    {
        public enum PauseMenuState
        {
            None=0,
            PauseMenu=1,
            SettingsMenu=2,
            QuitToMainMenu=3,
            QuitGameMenu=4,
        }
        
        [FormerlySerializedAs("currentMainMenuState")] [SerializeField] private PauseMenuState currentPauseMenuState = PauseMenuState.None;
        [FormerlySerializedAs("mainMenuObj")] [SerializeField] private GameObject pauseMenuObj;
        [SerializeField] private GameObject settingsObj;
        [SerializeField] private GameObject quitToMainConfirmObj;
        [SerializeField] private GameObject quitAppConfirmObj;
        
        [SerializeField] private Toggle[] mainMenuToggles;
        
        public AudioClip navigationSound;
        public AudioClip selectSound;
        
        private int currSelIndex = 0;
        private bool started = false;

        private static GameObject pauseMenuObjRef;
        private static PauseMenuController instance;
        
        public static bool IsShowing { get;private set; }

        public static void ShowPauseMenu(bool show)
        {
            if(IsShowing == show)
                return;
            IsShowing = show;

            if (IsShowing)
            {
                if (pauseMenuObjRef == null)
                {
                    pauseMenuObjRef = Resources.Load<GameObject>("PauseMenuGui");
                    
                }
                instance = Instantiate(pauseMenuObjRef).GetComponent<PauseMenuController>();
            }
            else
            {
                Destroy(instance.gameObject);
            }
        }
        
        public static void ReturnToPauseMenu()
        {
            if(instance != null)
                instance.SwitchMenu(PauseMenuState.PauseMenu);
        }
        
        private void Start()
        {
            InputReader.Instance.SetToUIInputs();
            SwitchMenu(PauseMenuState.PauseMenu);
            UpdateSelection();
        }

        private void OnDestroy()
        {
            InputReader.Instance.OnMainMenuNavigationUpDown -= OnMenuMainMenuNavigate;
            InputReader.Instance.OnMainMenuAccept -= OnSelectItem;
            InputReader.Instance.OnMainMenuBack -= ResumeGame;
            InputReader.Instance.SetToGameplayInputs();
        }
        
        private void OnSelectItem()
        {
            if(currentPauseMenuState != PauseMenuState.PauseMenu)
                return;
            
            switch (currSelIndex)
            {
                case 0:
                    ResumeGame();
                    break;
                case 1: SwitchMenu(PauseMenuState.SettingsMenu);
                    break;
                case 2: SwitchMenu(PauseMenuState.QuitToMainMenu);
                    break;
                case 3: SwitchMenu(PauseMenuState.QuitGameMenu);
                    break;
            }
            AudioManager.Instance.PlaySFX(selectSound);
        }

        private void ResumeGame()
        {
            if(currentPauseMenuState != PauseMenuState.PauseMenu)
                return;
            
            ShowPauseMenu(false);
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
        
        public void SwitchMenu(PauseMenuState newPauseMenuState)
        {
            currentPauseMenuState = newPauseMenuState;
            UpdateMenuState();
        }

        private void UpdateMenuState()
        {
            pauseMenuObj.gameObject.SetActive(currentPauseMenuState == PauseMenuState.PauseMenu);
            settingsObj.gameObject.SetActive(currentPauseMenuState == PauseMenuState.SettingsMenu);
            quitToMainConfirmObj.gameObject.SetActive(currentPauseMenuState == PauseMenuState.QuitToMainMenu);
            quitAppConfirmObj.gameObject.SetActive(currentPauseMenuState == PauseMenuState.QuitGameMenu);

            if (currentPauseMenuState == PauseMenuState.PauseMenu)
            {
                InputReader.Instance.OnMainMenuNavigationUpDown += OnMenuMainMenuNavigate;
                InputReader.Instance.OnMainMenuAccept += OnSelectItem;
                InputReader.Instance.OnMainMenuBack += ResumeGame;
            }
            else
            {
                InputReader.Instance.OnMainMenuNavigationUpDown -= OnMenuMainMenuNavigate;
                InputReader.Instance.OnMainMenuAccept -= OnSelectItem;
                InputReader.Instance.OnMainMenuBack -= ResumeGame;
            }
        }
    }
}
}