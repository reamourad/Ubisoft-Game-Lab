using System;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items.Interfaces;
using Player.IK;
using Player.Inventory;
using Unity.VisualScripting;
using UnityEngine;

namespace Items
{
    public class Tablet : NetworkBehaviour, IUsableItem, ISeekerAttachable
    {
        [Header("Attachment")]
        [SerializeField] private Transform graphicsAttachment;
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

            Transform attachmentTarget = null;
            if (IsOwner)
            {
                attachmentTarget = Camera.main.transform;
            }
            else
            {
                attachmentTarget = GetComponentInParent<SeekerLocators>().SeekerHeadNonOwner;
            }
            
            //Attach the graphic to player
            var temp = attachmentTarget.localRotation;
            attachmentTarget.localRotation = Quaternion.identity;
            graphicsAttachment.parent = attachmentTarget;
            attachmentTarget.localRotation = temp;
        }

        public void OnDetach(Transform parentTrf, bool spawnWorldDummy)
        {
            Debug.Log("Tablet:::OnDetach");
            var dummySpawnLoc = parentTrf.position + new Vector3(0,1,0) + parentTrf.forward * 2f;
            RPC_ServerRequestDespawn(dummySpawnLoc, spawnWorldDummy);
        }

        public string GetUsePromptText()
        {
            return "Use Cameras";
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

        private void OnDestroy()
        {
            if(graphicsAttachment != null)
                Destroy(graphicsAttachment.gameObject);
        }
    }
}