using FishNet.Object;
using Unity.Cinemachine;

namespace Networking
{
    public class PlayerOwnerManager : NetworkBehaviour
    {
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!IsOwner)
            {
                GetComponentInChildren<CinemachineCamera>().enabled = false;
                GetComponentInChildren<TestPlayerController>().enabled = false;
            }
        }
    }
}