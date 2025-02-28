using System;
using System.Collections;
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
        [SerializeField] GameObject fetchUI;
        [SerializeField] GameObject fetchFailUI;
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

        IEnumerator Start()
        {
#if !UNITY_SERVER
            fetchUI.gameObject.SetActive(true);
            var res = NetworkConnectionHelper.PullConnectionInfo();
            yield return new WaitUntil(() => res.IsCompleted);
            fetchUI.gameObject.SetActive(false);
            if (!NetworkConnectionHelper.ConnectionInfoAvailable)
            {
                fetchFailUI.gameObject.SetActive(true);
                yield break;
            }
            
            
            var clientVersion = Application.version;
            var serverVersion = NetworkConnectionHelper.RemoteServerVersion;
            versionText.text = $"Remote server version: {serverVersion}\n" +
                               $"Client version: {clientVersion}";

            if (clientVersion != serverVersion)
            {
                Debug.LogWarning("ClientConnectionUI::Remote Server and Client version mismatch, Features/Systems may break due to a different version.");
            }
#endif
        }
        
        private void Update()
        {
            serverStarted = InstanceFinder.IsServerStarted;
            clientStarted = InstanceFinder.IsClientStarted;
        }
    }
}