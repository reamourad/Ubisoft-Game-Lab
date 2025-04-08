using System;
using System.Linq;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items.Interfaces;
using Player.Audio;
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
        
        [SerializeField] private float dotThreshold = 0.65f;
        
        [SerializeField] private NetworkObject worldDummyRef;
        [SerializeField] private GameObject displayGui;
        [SerializeField] private TMPro.TMP_Text readingText;
        
        [SerializeField] private AudioClip readSound;
        [SerializeField] private Transform checkPoint;
        
        [SerializeField] private float nextUseDelay = 0.15f;

        private ThermometerGui gui;
        private PlayerRole hiderRole;
        
        private bool isUsing = false;
        private float nextTime = 0;
        
        public void UseItem(bool isUsing)
        {
            this.isUsing = isUsing;
            AudioManager.Instance.PlaySFX(readSound);
            displayGui.SetActive(isUsing);
        }

        private void LateUpdate()
        {
            if(!isUsing)
                return;
            
            if(Time.time > nextTime)
                nextTime = Time.time + nextUseDelay;
            
            var reading = ReadTemperature();
            switch (reading.temp)
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
                gui.SetTemperatureText(readingText.text, reading.distance);
        }

        private (TempType temp, float distance) ReadTemperature()
        {
            hiderRole = FindObjectsByType<PlayerRole>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .SingleOrDefault(a => a.Role == PlayerRole.RoleType.Hider);

            if (hiderRole == null)
                return (TempType.Normal, Mathf.Infinity);
            
            var dir = (hiderRole.GetCentrofMassPosition() - transform.position).normalized;
            var dot = Vector3.Dot(checkPoint.forward, dir);
            if (dot >= dotThreshold)
            {
                return (TempType.Low, Vector3.Distance(hiderRole.GetCentrofMassPosition(), checkPoint.position));
            }
            
            return (TempType.Normal, Mathf.Infinity);
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
                transform.localPosition += new Vector3(0, 0.015f, 0);
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

        public string GetUsePromptText()
        {
            return "Take reading";
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