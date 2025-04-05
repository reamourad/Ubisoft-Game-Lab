using System;
using System.Collections;
using Networking;
using Player;
using UnityEngine;
using UnityEngine.Playables;

namespace StateManagement
{
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private PlayableDirector seekerWin;
        [SerializeField] private PlayableDirector hiderWin;

        [SerializeField] private bool test;
        [SerializeField] PlayerRole.RoleType testWinner;
        
        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            InputReader.Instance.OnMainMenuAccept += OnContinue;
            var winner = test ? testWinner : GameLookupMemory.Winner;
            yield return new WaitForEndOfFrame();

            PlayableDirector director=null;
            if (winner == PlayerRole.RoleType.Hider)
            {
                director = hiderWin;
                hiderWin.gameObject.SetActive(true);
                yield return new WaitForEndOfFrame();
                hiderWin.Play();
            }
            else if (winner == PlayerRole.RoleType.Seeker)
            {
                director = seekerWin;
                seekerWin.gameObject.SetActive(true);
                yield return new WaitForEndOfFrame();
                seekerWin.Play();
            }

            if (director != null)
            {
                yield return new WaitForSeconds((float)director.playableAsset.duration);
            }
            
            InputReader.Instance.SetToUIInputs();
        }

        private void OnContinue()
        {
            GameStateController.Instance?.ClientDisconnectFromServer();
        }
    }
}