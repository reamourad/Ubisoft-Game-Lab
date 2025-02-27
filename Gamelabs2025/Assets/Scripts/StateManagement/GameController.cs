using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Component.Animating;
using FishNet.Object;
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
        
        private List<NetworkObject> players;
        
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
            
            SwitchGameStage(GameStage.Preparing);
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
            Networking.TimeManager.Instance.Initialize(gameTimeSeconds, () =>
            {
                SwitchGameStage(GameStage.Game);
            });
        }

        private void OpenDoor()
        {
            doorAnimator?.SetTrigger("Open");
        }
        
        private void ServerGamePostStage()
        { 
            if(!IsServerStarted)
                return;
            
            GameStateController.Instance.Server_ChangeState(GameStates.GameOver);
        }
        
        public void ServerHiderCaptured()
        {
            if(!IsServerStarted)
                return;
            
            SwitchGameStage(GameStage.Postgame);
        }
    }
}