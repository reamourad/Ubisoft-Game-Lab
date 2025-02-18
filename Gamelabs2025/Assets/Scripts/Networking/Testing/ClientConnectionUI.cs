using System;
using FishNet;
using UnityEngine;

namespace Networking.Testing
{
    public class ClientConnectionUI : MonoBehaviour
    {
        [SerializeField]
        private bool serverStarted = false;
        [SerializeField]
        private bool clientStarted = false;
        
        [SerializeField] TMPro.TMP_InputField portInputField;
        [SerializeField] TMPro.TMP_Text versionText;
        
        public void OnClickConnectToRemoteServer()
        {
            NetworkConnectionHelper.ConnectToRemoteServer();
        }

        public void OnClickConnectToLocalServer()
        {
            NetworkConnectionHelper.ConnectToLocalServer();
        }

        public void OnClickStartServer()
        {
            NetworkConnectionHelper.StartServer();
        }

        public void OnClickCustomConnectToServer()
        {
            NetworkConnectionHelper.ConnectToServer("127.0.0.1", ushort.Parse(portInputField.text));
        }

        void Start()
        {
            var clientVersion = Application.version;
            var serverVersion = NetworkConnectionHelper.RemoteServerVersion;
            versionText.text = $"Remote server version: {serverVersion}\n" +
                               $"Client version: {clientVersion}";

            if (clientVersion != serverVersion)
            {
                Debug.LogWarning("ClientConnectionUI::Remote Server and Client version mismatch, Features/Systems may break due to a different version.");
            }
            
            InstanceFinder.NetworkManager.ServerManager.StopConnection(true);
            NetworkConnectionHelper.StartServer();
        }
        
        private void Update()
        {
            serverStarted = InstanceFinder.IsServerStarted;
            clientStarted = InstanceFinder.IsClientStarted;
        }
    }
}