using FishNet.Transporting;
using UnityEngine;

namespace Networking
{
    public class HeadlessServerStarter : MonoBehaviour
    {
#if UNITY_SERVER
        void Start()
        {
            if(Application.isEditor)
                return;
            Debug.Log("HeadlessServerStarter:: Starting server");
            NetworkConnectionHelper.StartServer();
        }
#endif
    }
}