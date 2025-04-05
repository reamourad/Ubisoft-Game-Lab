using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Transporting;
using Networking;
using UnityEngine;
using Utils;

namespace StateManagement
{
    public class GameStateController : SingletonBehaviour<GameStateController>
    {
        [SerializeField] private string MainMenuSceneName = "MainMenu";
        [SerializeField] private string CutsceneSceneName = "Cutscene";
        [SerializeField] private string GameSceneName = "GameOver";
        [SerializeField] private string GameOverSceneName = "Gameplay";
        
        
        [SerializeField]
        private GameStates currentState;

        [SerializeField]
        private int minPlayers = 1;


        private List<int> connectedPlayers;
        
        private void Start()
        {
            connectedPlayers = new List<int>();
            InstanceFinder.NetworkManager.ServerManager.OnRemoteConnectionState += Server_OnRemoteConnectionStateChanged;
            InstanceFinder.NetworkManager.ClientManager.OnClientConnectionState += Client_OnClientConnectionStateChanged;
        }

        public void ClientDisconnectFromServer()
        {
            NetworkConnectionHelper.ClientDisconnectFromServer();
        }
        
        public void ServerChangeState(GameStates newState)
        {
            if(!NetworkUtility.IsServer)
                return;
            
            if(newState == currentState)
                return;
            
            Debug.Log($"GameStateController::<color=green> Switching state {currentState} --> {newState} </color>");
            switch (newState)
            {
                case GameStates.MainMenu:
                    NetworkUtility.Server_LoadScene(MainMenuSceneName);
                    break;
                case GameStates.CutScene:
                    NetworkUtility.Server_LoadScene(CutsceneSceneName);
                    break;
                case GameStates.Game:
                    NetworkUtility.Server_LoadScene(GameSceneName);
                    break;
                case GameStates.GameOver:
                    NetworkUtility.Server_LoadScene(GameOverSceneName);
                    break;
            }
            currentState = newState;
        }
        
        private void Server_OnRemoteConnectionStateChanged(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            var serverManager = InstanceFinder.NetworkManager.ServerManager;
            Debug.Log($"GameStateController::<color=green>Remote Connection (Server) changed for {args.ConnectionId} : {args.ConnectionState}</color>");
            if (args.ConnectionState == RemoteConnectionState.Started)
            {
                connectedPlayers.Add(args.ConnectionId);
                if (connectedPlayers.Count == minPlayers)
                {
                    ServerChangeState(GameStates.CutScene);
                }
            }
            else if (args.ConnectionState == RemoteConnectionState.Stopped)
            {
                connectedPlayers.Remove(args.ConnectionId);
                if (connectedPlayers.Count < minPlayers)
                {
                    Debug.Log("GameStateController::Minimum Players not available, game session cancelled.");
                    CleanupAndReturnMainMenu();
                }
            }
        }
        
        private void Client_OnClientConnectionStateChanged(ClientConnectionStateArgs args)
        {
            Debug.Log($"GameStateController::<color=green>Remote Connection changed (Client) {args.ConnectionState}</color>");
            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                CleanupAndReturnMainMenu();
            }
        }
        
        private void CleanupAndReturnMainMenu()
        {
            NetworkConnectionHelper.ResetConnections();
            Destroy(InstanceFinder.NetworkManager.gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene(MainMenuSceneName);
            Destroy(this.gameObject); //delete the state-manager
        }
    }
}