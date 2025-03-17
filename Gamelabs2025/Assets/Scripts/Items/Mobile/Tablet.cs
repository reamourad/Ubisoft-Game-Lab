using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items.Interfaces;
using Player.Inventory;
using Unity.VisualScripting;
using UnityEngine;

namespace Items
{
    public class Tablet : NetworkBehaviour, IUsableItem, ISeekerAttachable
    {
        [SerializeField] private NetworkObject worldDummyRef;
        private CameraPreviewer cameraPreviewer;
        
        public void UseItem(bool isUsing)
        {
            if (isUsing && cameraPreviewer == null)
            {
                cameraPreviewer = Instantiate(Resources.Load("Camera/CameraViewer")).GetComponent<CameraPreviewer>();
                cameraPreviewer.Open();
            }
        }

        public void OnAttach(Transform parentTrf)
        {
            transform.SetParent(parentTrf);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            Debug.Log("Tablet:::OnAttach");
        }

        public void OnDetach(Transform parentTrf, bool spawnWorldDummy)
        {
            Debug.Log("Tablet:::OnDetach");
            var dummySpawnLoc = parentTrf.position + parentTrf.forward * 2f;
            RPC_ServerRequestDespawn(dummySpawnLoc, spawnWorldDummy);
        }

        [ServerRpc]
        private void RPC_ServerRequestDespawn(Vector3 spawnLoc, bool spawnWorldDummy)
        {
            if (spawnWorldDummy)
            {
                var netManager = InstanceFinder.NetworkManager;
                var nob = netManager.GetPooledInstantiated(worldDummyRef, spawnLoc, Quaternion.identity, true);
                netManager.ServerManager.Spawn(nob);
            }

            Despawn();
        }
    }
}