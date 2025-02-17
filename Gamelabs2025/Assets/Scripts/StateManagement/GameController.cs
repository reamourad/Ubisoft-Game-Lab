using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using NUnit.Framework;
using UnityEngine;

namespace StateManagement
{
    public class GameController : NetworkBehaviour
    {
        [SerializeField] NetworkObject playerPrefab;

        private List<NetworkObject> players;
        
        private void Start()
        {
            if (IsServerStarted)
            {
                SpawnPlayers();
            }
        }

        private void SpawnPlayers()
        {
            //TODO: Make it spawn all players
            var networkManager = InstanceFinder.NetworkManager;
            players = new List<NetworkObject>();
            foreach (var connection in InstanceFinder.NetworkManager.ClientManager.Clients)
            {
                NetworkObject nob = networkManager.GetPooledInstantiated(playerPrefab,Vector3.zero, Quaternion.identity, true);
                networkManager.ServerManager.Spawn(nob, connection.Value);
                players.Add(nob);
            }
        }
    }
}