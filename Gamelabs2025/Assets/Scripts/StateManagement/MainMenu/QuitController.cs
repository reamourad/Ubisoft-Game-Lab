using System;
using Player.Audio;
using UnityEngine;

namespace StateManagement
{
    public class QuitController : MonoBehaviour
    {
        [SerializeField]
        MainMenuController mainMenuController;
        
        private void OnEnable()
        {
            InputReader.Instance.OnMainMenuAccept += QuitGame;
            InputReader.Instance.OnMainMenuBack += BackToMain;
        }

        private void BackToMain()
        {
            AudioManager.Instance.PlaySFX(mainMenuController.selectSound);
            mainMenuController.SwitchMenu(MainMenuController.MainMenuState.MainMenu);
        }

        private void QuitGame()
        {
            AudioManager.Instance.PlaySFX(mainMenuController.selectSound);
            Application.Quit();
        }

        private void OnDisable()
        {
            InputReader.Instance.OnMainMenuAccept -= QuitGame;
            InputReader.Instance.OnMainMenuBack -= BackToMain;
        }
        
    }
}