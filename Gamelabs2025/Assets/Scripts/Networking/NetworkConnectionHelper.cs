using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using Newtonsoft.Json;
using UnityEngine;

namespace Networking
{
    public static class NetworkConnectionHelper
    {
        [System.Serializable]
        private class NetworkConnectionInfo
        {
            public string remoteServerVersion;
            public string address;
            public int port;
        }
        
        private const string SERVER_IPADDR = "127.0.0.1";
        private const ushort SERVER_PORT = 7770;

        private static Dictionary<string, NetworkConnectionInfo> connectionInfo;
        private static LocalConnectionState connectionState;
        
        public static bool ConnectionInfoAvailable => GetConnectionInfo() != null;

        public static string RemoteServerVersion
        {
            get
            {
                var connectionInfo = GetConnectionInfo();
                if (connectionInfo == null || !connectionInfo.ContainsKey(Application.version))
                    return "--";

                return connectionInfo[Application.version].remoteServerVersion;
            }
        }
        
        private static Dictionary<string, NetworkConnectionInfo> GetConnectionInfo()
        {
            return connectionInfo;
            
            /*var txtAsset = Resources.Load<TextAsset>("NetworkConf/ConnectionInfo");
            if (txtAsset == null)
            {
                Debug.LogError("<color=red>Network Connection Info is missing from Resources</color>");
                return null;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<NetworkConnectionInfo>(txtAsset.text);*/
        }
        
        public static void StartServer(System.Action<ServerConnectionStateArgs> serverConnectionStateCallback=null)
        {
            InstanceFinder.ServerManager.OnServerConnectionState -= OnServerConnectionStateChanged;
            InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionStateChanged;
            if(serverConnectionStateCallback != null)
                InstanceFinder.ServerManager.OnServerConnectionState += serverConnectionStateCallback;
            
            var tugBoat = InstanceFinder.NetworkManager.GetComponent<Tugboat>();
            //set addresses
            tugBoat.SetServerBindAddress(SERVER_IPADDR, IPAddressType.IPv4);
            tugBoat.SetPort(SERVER_PORT);
            
            Debug.Log("<color=cyan>NetworkConnectionHelper::Starting Server...</color>");
            //start server.
            tugBoat.StartConnection(true);
        }
        private static void OnServerConnectionStateChanged(ServerConnectionStateArgs args)
        {
            Debug.Log($"<color=cyan>NetworkConnectionHelper::Server connection state changed: {args.ConnectionState}</color>");
            connectionState = args.ConnectionState;
        }
        
        public static void ConnectToServer(string address, ushort port, System.Action<ClientConnectionStateArgs> clientConnectionStateCallback=null)
        {
            InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionStateChanged;
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionStateChanged;
            if(clientConnectionStateCallback != null)
                InstanceFinder.ClientManager.OnClientConnectionState += clientConnectionStateCallback;

            var tugBoat = InstanceFinder.NetworkManager.GetComponent<Tugboat>();
            Debug.Log("<color=cyan>NetworkConnectionHelper::Starting Client...</color>");
            //set addresses
            tugBoat.SetClientAddress(address);
            tugBoat.SetPort(port);
            
            //start server.
            tugBoat.StartConnection(false);
        }

        public static void ClientDisconnectFromServer()
        {
            var tugBoat = InstanceFinder.NetworkManager.GetComponent<Tugboat>();
            tugBoat.StopConnection(false);
        }
        
        private static void OnClientConnectionStateChanged(ClientConnectionStateArgs args)
        {
            Debug.Log($"<color=cyan>NetworkConnectionHelper::Client connection state changed: {args.ConnectionState}</color>");
            connectionState = args.ConnectionState;
        }
        
        public static void ConnectToRemoteServer(System.Action<ClientConnectionStateArgs> clientConnectionStateCallback = null)
        {
            var connections = GetConnectionInfo();
            if (connections == null || !connections.ContainsKey(Application.version))
            {
                Debug.Log("Server Version for this client is not available.");
                return;
            }

            var connectionInfo = GetConnectionInfo()[Application.version];
            ConnectToServer(connectionInfo.address, (ushort)connectionInfo.port, clientConnectionStateCallback);
        }

        public static void ConnectToLocalServer(System.Action<ClientConnectionStateArgs> clientConnectionStateCallback = null)
        {
            ConnectToServer(SERVER_IPADDR, SERVER_PORT, clientConnectionStateCallback);
        }

        public static void ResetConnections()
        {
            var tugboat = InstanceFinder.NetworkManager.GetComponent<Tugboat>();
            if (NetworkUtility.IsClient)
            {
                tugboat.StopConnection(false);
            }
            if (NetworkUtility.IsServer)
            {
                tugboat.StopConnection(true);
            }
        }

        /// <summary>
        /// Pull the server connection config from a cloud storage solution via a simple GET request
        /// This allows our server config to be updated independent of the client build.
        /// So if any errors happen during demo-day. we can re-deploy our server and update the config to continue
        /// playing.
        /// </summary>
        public static async Task PullConnectionInfo()
        {
            var url =
                "https://objectstorage.ca-montreal-1.oraclecloud.com/n/axadirgfhxca/b/Gamelabs/o/ConnectionInfo.json";
            HttpClient client = new HttpClient();
            var res = await client.GetAsync(url);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                connectionInfo = JsonConvert.DeserializeObject<Dictionary<string, NetworkConnectionInfo>>(json);
            }
        } 
    }
}