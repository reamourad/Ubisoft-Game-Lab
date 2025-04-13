using System;
using System.Collections;
using FishNet;
using FishNet.Object;
using Networking;
using Player;
using Player.Audio;
using UnityEngine;
using UnityEngine.Playables;

namespace StateManagement
{
    public class GameOverController : NetworkBehaviour
    {
        [SerializeField] private PlayableDirector seekerWin;
        [SerializeField] private PlayableDirector hiderWin;

        [SerializeField] private bool test;
        [SerializeField] PlayerRole.RoleType testWinner;
        
        [SerializeField] private GameObject replayNotifObj;
        [SerializeField] private AudioClip replayNotif;
        
        private bool disconnected = false;
        private bool replayRequested = false;
        private int replayRequestCount=0;


        public override void OnStartClient()
        {
            base.OnStartClient();
            StartCoroutine(StartOverLogic());
        }

        private IEnumerator StartOverLogic()
        {
            GameController.IsReplayingGame = false;
            yield return new WaitForEndOfFrame();
            InputReader.Instance.OnMainMenuAccept += ReplayGame;
            InputReader.Instance.OnMainMenuBack += ToMainMenu;
            
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

        private void ToMainMenu()
        {
            InputReader.Instance.OnMainMenuBack -= ToMainMenu;
            InputReader.Instance.OnMainMenuAccept -= ReplayGame;
            
            if (GameStateController.Instance != null)
                GameStateController.Instance.ClientDisconnectFromServer();
        }
        
        private void ReplayGame()
        {
            if (replayRequested)
                return;
            
            replayRequested = true;
            InputReader.Instance.OnMainMenuAccept -= ReplayGame;
            RPC_RequestServerToReplay();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RPC_RequestServerToReplay()
        {
            replayRequestCount+=1;

            if (replayRequestCount < InstanceFinder.ClientManager.Clients.Count)
            {
                Debug.Log("Game Replay Requested waiting for 1 more.");
                RPC_ServerRecievedRequest();
                return;
            }

            if (GameStateController.Instance != null)
            {
                GameController.IsReplayingGame = true;
                GameStateController.Instance.ServerChangeState(GameStates.Game);
            }
        }

        [ObserversRpc(ExcludeOwner = false)]
        private void RPC_ServerRecievedRequest()
        {
            AudioManager.Instance.PlaySFX(replayNotif);
            replayNotifObj.SetActive(true);
        }

        private void OnDestroy()
        {
            InputReader.Instance.OnMainMenuAccept -= ReplayGame;
            InputReader.Instance.OnMainMenuBack -= ToMainMenu;
        }
    }
}