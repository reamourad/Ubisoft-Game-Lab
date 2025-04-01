using System;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items.Interfaces;
using Player.IK;
using Player.Inventory;
using UnityEngine;

namespace Player.Items.Thermometer
{
    public enum TempType
    {
        None=0,
        Normal,
        Low
    }
    public class Thermometer : NetworkBehaviour , IUsableItem, ISeekerAttachable
    {
        private const string NORMAL_READING = "NORM";
        private const string LOW_READING = "LOW";
        private const string NONE_READING = "--";
        
        [Header("Attachment")]
        [SerializeField] private Transform graphicsAttachment;
        
        [SerializeField] private float boxCastRange = 10;
        
        [SerializeField] private NetworkObject worldDummyRef;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private BoxCollider detector;
        [SerializeField] private TMPro.TMP_Text readingText;

        private ThermometerGui gui;
        
        public void UseItem(bool isUsing)
        {
            var temp = ReadTemperature();
            switch (temp)
            {
                case TempType.Normal:
                    readingText.text = NORMAL_READING;
                    break;
                case TempType.Low:
                    readingText.text = LOW_READING;
                    break;
                default:
                    readingText.text = NONE_READING;
                    break;
            }
            
            if(gui != null)
                gui.SetTemperatureText(readingText.text);
        }

        private TempType ReadTemperature()
        {
            if (Physics.BoxCast(detector.bounds.center, detector.bounds.size, detector.transform.forward,out RaycastHit hit,
                    detector.transform.rotation,
                    boxCastRange,layerMask))
            {
                if (hit.collider == null)
                    return TempType.Normal;
                
                var role = hit.collider.GetComponentInParent<PlayerRole>();
                if (role != null && role.Role == PlayerRole.RoleType.Hider)
                {
                    return TempType.Low;
                }
            }
            
            return TempType.Normal;
        }

        public void OnAttach(Transform parent)
        {
            Debug.Log("Thermometer:::OnAttach");
            transform.parent = parent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            
            if(IsOwner)
                gui = Instantiate(Resources.Load<GameObject>("ThermometerCanvas")).GetComponent<ThermometerGui>();
            
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
            Debug.Log("Thermometer:::OnDetach");
            var dummySpawnLoc = parentTrf.position + new Vector3(0,1,0) + parentTrf.forward * 2f;
            RPC_ServerRequestDespawn(dummySpawnLoc, spawnWorldDummy);
        }

        private void OnDestroy()
        {
            if(gui != null)
                Destroy(gui.gameObject);
            
            if(graphicsAttachment != null)
                Destroy(graphicsAttachment.gameObject);
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