using FishNet.Object;
using Player.Data;
using UnityEngine;

namespace Player.Items
{
    public abstract class StationaryObjectBase : NetworkBehaviour
    {
        [SerializeField] private StationaryEffect supportedEffects;
        
        [Server]
        public void ApplyStationaryEffect(StationaryEffect effects)
        {
            if ((supportedEffects & effects) != 0)
            {
                Debug.Log($"StationaryObjectBase:: Applying stationary effect: {effects}");
                OnServerActivateStationaryObject();
                RPC_ActivateStationaryObjectOnClients();
            }
        }

        [ObserversRpc]
        private void RPC_ActivateStationaryObjectOnClients()
        {
            OnClientActivateStationaryObject();
        }
        
        /// <summary>
        /// Do game state logic here, this is will run at the server side
        /// </summary>
        protected abstract void OnServerActivateStationaryObject();
        /// <summary>
        /// Any Audio, or visuals should be done here. this will be triggered in all clients,
        /// so implementing SFX and VFX here is sufficient
        /// </summary>
        protected abstract void OnClientActivateStationaryObject();
    }
}