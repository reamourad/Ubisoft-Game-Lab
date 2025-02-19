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
        [SerializeField] private List<Transform> spawnPoints;
        
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
            int indx = 0;
            foreach (var connection in InstanceFinder.NetworkManager.ClientManager.Clients)
            {
                var point = spawnPoints[indx];
                
                NetworkObject nob = networkManager.GetPooledInstantiated(playerPrefab,point.position, point.rotation, true);
                networkManager.ServerManager.Spawn(nob, connection.Value);
                players.Add(nob);
                
                indx = (indx + 1) % spawnPoints.Count;
            }
        }
    }
}