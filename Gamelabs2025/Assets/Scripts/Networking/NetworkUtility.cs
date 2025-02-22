using FishNet;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using UnityEngine;

namespace Networking
{
    public static class NetworkUtility
    {
        public static bool IsServer => InstanceFinder.IsServerStarted;
        public static bool IsClient => InstanceFinder.IsClientOnlyStarted;

        public static void Server_LoadScene(string sceneName)
        {
            if(!IsServer)
                return;

            Debug.Log("Loading scene " + sceneName);
            var data = new SceneLoadData(sceneName)
            {
                ReplaceScenes = ReplaceOption.All
            };
            InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(data);
        }
    }
}
