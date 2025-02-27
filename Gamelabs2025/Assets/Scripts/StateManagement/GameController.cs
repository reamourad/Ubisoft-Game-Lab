using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using FishNet;
using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Networking;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StateManagement
{
    public class GameController : NetworkBehaviour
    {
        public enum GameStage
        {
            None = 0,
            Preparing,
            Game,
            Postgame
        }
        
        [SerializeField] private NetworkObject seekerPrefab;
        [SerializeField] private Transform seekerSpawn;
        
        [SerializeField] private NetworkObject hiderPrefab;
        [SerializeField] private Transform hiderSpawn;

        [Header("Game Variables")] 
        [SerializeField] private int prepTimeSeconds = 30;
        [SerializeField] private int gameTimeSeconds = 480;
        [SerializeField] private NetworkAnimator doorAnimator;
        
        private GameStage currentStage = GameStage.None;
        private List<NetworkObject> players;
        private readonly SyncVar<PlayerRole.RoleType> winner = new SyncVar<PlayerRole.RoleType>(PlayerRole.RoleType.None);

        public Action<GameStage> OnStageChanged;
        
        // singleton pattern
        private static GameController instance;
        public static GameController Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<GameController>();
                
                return instance;
            }
        }
        
        private void Start()
        {
            if (!IsServerStarted)
            {
                return;
            }

            winner.Value = PlayerRole.RoleType.None;
            winner.OnChange += WinnerOnOnChange;
            SwitchGameStage(GameStage.Preparing);
        }

        private void OnDestroy()
        {
            winner.OnChange -= WinnerOnOnChange;
        }

        private void WinnerOnOnChange(PlayerRole.RoleType prev, PlayerRole.RoleType next, bool asserver)
        {
            GameLookupMemory.Winner = next;
        }

        private void SpawnPlayers()
        {
            var networkManager = InstanceFinder.NetworkManager;
            players = new List<NetworkObject>();
            int seekerCount = 0;
            foreach (var connection in InstanceFinder.NetworkManager.ClientManager.Clients)
            {
                var randSel = SelectRandomSide();
                NetworkObject nob = networkManager.GetPooledInstantiated(randSel.prefab,randSel.spawnPoint.position, randSel.spawnPoint.rotation, true);
                networkManager.ServerManager.Spawn(nob, connection.Value);
                players.Add(nob);
            }
        }
        
        //Lets not look here, its quite shitty :p
        private (NetworkObject prefab, Transform spawnPoint) SelectRandomSide()
        {
            NetworkObject prefab;
            Transform spawnPoint;
            if (players.Count == 0)
            {
                if(Random.Range(0, 100) % 2 == 0)
                {
                    prefab = seekerPrefab;
                    spawnPoint = seekerSpawn;
                }
                else
                {
                    prefab = hiderPrefab;
                    spawnPoint = hiderSpawn;
                }
            }
            else
            {
                var playerRole = players[0].GetComponent<PlayerRole>().Role;
                if(playerRole == PlayerRole.RoleType.Hider)
                {
                    prefab = seekerPrefab;
                    spawnPoint = seekerSpawn;
                }
                else
                {
                    prefab = hiderPrefab;
                    spawnPoint = hiderSpawn;
                }
            }
            
            return (prefab, spawnPoint);
        }
        
        private void SwitchGameStage(GameStage stage)
        {
            if(!IsServerStarted)
                return;
            
            switch (stage)
            {
                case GameStage.Preparing:
                    ServerGamePrepareStage();
                    break;
                
                case GameStage.Game:
                    ServerGamePlayStage();
                    break;
                case GameStage.Postgame:
                    ServerGamePostStage();
                    break;
            }
            currentStage = stage;
            OnStageChanged?.Invoke(currentStage);
            RPC_InformClientsOfGameStageChange(currentStage);
        }

        [ObserversRpc]
        void RPC_InformClientsOfGameStageChange(GameStage stage)
        {
            currentStage = stage;
            OnStageChanged?.Invoke(currentStage);
        }
        
        private void ServerGamePrepareStage()
        {
            SpawnPlayers();
            Networking.TimeManager.Instance.Initialize(prepTimeSeconds, () =>
            {
                SwitchGameStage(GameStage.Game);
            });
        }
        
        private void ServerGamePlayStage()
        {
            OpenDoor();
            Networking.TimeManager.Instance.Initialize(gameTimeSeconds, ServerGameTimeRanOut);
        }
        
        private void OpenDoor()
        {
            doorAnimator?.SetTrigger("Open");
        }
        
        private void ServerGamePostStage()
        { 
            if(!IsServerStarted)
                return;
            
            GameStateController.Instance.ServerChangeState(GameStates.GameOver);
        }
        
        /// <summary>
        /// Considers the game to be won by the Hider, Changes state to post game after 2 seconds.
        /// </summary>
        private void ServerGameTimeRanOut()
        {
            if(!IsServerStarted)
                return;
            GameLookupMemory.Winner = PlayerRole.RoleType.Hider;
            StartCoroutine(DelayedInvoke(() =>
            {
                SwitchGameStage(GameStage.Postgame);
            }, 2f));
        }
        
        /// <summary>
        /// Considers the game to be won by the seeker, Changes state to post game after 3 seconds.
        /// </summary>
        public void ServerHiderCaptured()
        {
            if (!IsServerStarted)
            {
                Debug.Log("GameController::Invoking On Server!!");
                RPC_InvokeServerHiderCapturedOnServer();
                return;
            }
                
            if(currentStage != GameStage.Game)
                return;
            
            var hider = players.Find(a => a.GetComponent<PlayerRole>().Role == PlayerRole.RoleType.Hider);
            if(hider != null)
                hider.Despawn();
            
            Networking.TimeManager.Instance.StopActiveTimer();
            GameLookupMemory.Winner = PlayerRole.RoleType.Seeker;
            StartCoroutine(DelayedInvoke(() =>
            {
                SwitchGameStage(GameStage.Postgame);
            }, 3f));
        }

        [ServerRpc(RequireOwnership = false)]
        private void RPC_InvokeServerHiderCapturedOnServer()
        {
            ServerHiderCaptured();
        }
        
        private IEnumerator DelayedInvoke(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}