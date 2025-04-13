using System;
using System.Collections;
using Player.Audio;
using StateManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Credits
{
    public class CreditsRoller : MonoBehaviour
    {
        [SerializeField] ScrollRect scrollView;
        [SerializeField] AudioClip clip;
        [SerializeField] AudioClip menuClip;

        [SerializeField] MainMenuController mainMenuController;
        
        Coroutine routine;
        
        private void OnEnable()
        {
            InputReader.Instance.OnMainMenuBack += CloseCredits;
            routine = StartCoroutine(Credits(CloseCredits));
        }

        private void OnDisable()
        {
            InputReader.Instance.OnMainMenuBack -= CloseCredits;
            if(routine != null)
                StopCoroutine(routine);
            
            AudioManager.Instance.PlayBG(menuClip);
        }

        private void CloseCredits()
        {
            mainMenuController.SwitchMenu(MainMenuController.MainMenuState.MainMenu);
        }
        
        private IEnumerator Credits(Action onComplete = null)
        {
            AudioManager.Instance.PlayBG(clip);
            float time = clip.length / 2;
            float timeStep = 0;
            while (timeStep <= 1)
            {
                timeStep += Time.deltaTime / time;
                scrollView.normalizedPosition = Vector2.Lerp(Vector2.up,Vector2.zero, timeStep);
                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForSeconds(clip.length / 2);
            routine = null;
            onComplete?.Invoke();
        }
    }
}