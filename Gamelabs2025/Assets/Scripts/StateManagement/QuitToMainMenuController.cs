using Player.Audio;
using StateManagement.MainMenu.UI;
using StateManagement.StateManagement;
using UnityEngine;

namespace StateManagement
{
    public class QuitToMainMenuController : MonoBehaviour
    {
        private void OnEnable()
        {
            InputReader.Instance.OnMainMenuAccept += QuitGame;
            InputReader.Instance.OnMainMenuBack += BackToMain;
        }

        private void BackToMain()
        {
            AudioManager.Instance.PlaySFX(UINavigationSounds.Instance.selectSound);
            PauseMenuController.ReturnToPauseMenu();
        }

        private void QuitGame()
        {
            AudioManager.Instance.PlaySFX(UINavigationSounds.Instance.selectSound);
            GameStateController.Instance.ClientDisconnectFromServer();
        }

        private void OnDisable()
        {
            InputReader.Instance.OnMainMenuAccept -= QuitGame;
            InputReader.Instance.OnMainMenuBack -= BackToMain;
        }
        
    }
}