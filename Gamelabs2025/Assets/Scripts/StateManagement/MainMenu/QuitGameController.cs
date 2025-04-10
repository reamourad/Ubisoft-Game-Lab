using System;
using Player.Audio;
using StateManagement.MainMenu.UI;
using StateManagement.StateManagement;
using UnityEngine;

namespace StateManagement
{
    public class QuitGameController : MonoBehaviour
    {
        [SerializeField]
        MainMenuController mainMenuController;

        [SerializeField] private bool IsInsidePause;
        
        private void OnEnable()
        {
            InputReader.Instance.OnMainMenuAccept += QuitGame;
            InputReader.Instance.OnMainMenuBack += BackToMain;
        }

        private void BackToMain()
        {
            AudioManager.Instance.PlaySFX(UINavigationSounds.Instance.selectSound);
            if (IsInsidePause)
                PauseMenuController.ReturnToPauseMenu();
            else
                mainMenuController.SwitchMenu(MainMenuController.MainMenuState.MainMenu);
        }

        private void QuitGame()
        {
            AudioManager.Instance.PlaySFX(UINavigationSounds.Instance.selectSound);
            Application.Quit();
        }

        private void OnDisable()
        {
            InputReader.Instance.OnMainMenuAccept -= QuitGame;
            InputReader.Instance.OnMainMenuBack -= BackToMain;
        }
        
    }
}