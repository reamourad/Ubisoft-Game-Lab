using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items.Interfaces;
using Player;
using Player.IK;
using Player.Inventory;
using StateManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace Items
{
    public class Vacuum : NetworkBehaviour, IUsableItem, ISeekerAttachable
    {
        private struct VacuumCacheStruct
        {
            public readonly NetworkObject NObj;
            public readonly Rigidbody RigidBody;
            public readonly PlayerRole Role;

            public VacuumCacheStruct(NetworkObject nObj, Rigidbody rigidBody, PlayerRole role)
            {
                NObj = nObj;
                RigidBody = rigidBody;
                Role = role;
            }
        }

        [FormerlySerializedAs("vacummHead")]
        [Header("Basic Attachment")] 
        [SerializeField] private Transform vacuumHead;
        [SerializeField] private Transform vacuumBody;
        
        [Header("Power Params")]
        [SerializeField] private float maxPower = 100f;
        [FormerlySerializedAs("rechargeRate")] [SerializeField] private float rechargeRatePerSec = 1.5f;
        [FormerlySerializedAs("useRate")] [SerializeField] private float useRatePerSec = 2.5f;
        
        [Header("References and Other Params")]
        [SerializeField] private NetworkObject worldDummyRef;
        [SerializeField] private SphereCollider triggerVolume;
        [SerializeField] private Transform suctionPoint;
        [SerializeField] private float suctionCompleteDetectionRadius=0.25f;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float vacuumSuctionRegular = 10;
        [SerializeField] private float vacuumSuctionPlayer = 10;
        [SerializeField] private ParticleSystem particles;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip startClip;
        [SerializeField] private AudioClip midClip;
        [SerializeField] private AudioClip endClip;
        
        private Dictionary<Collider, VacuumCacheStruct> vacuumCache = new Dictionary<Collider, VacuumCacheStruct>();
        
        private bool localUsingFlag = false;
        private readonly SyncVar<bool> VacuumActive = new SyncVar<bool>();
        
        private Rigidbody parentRigidbody;
        private bool suckedPlayer = false;
        
        private VacuumGui vacuumGui;
        private Coroutine vacuumStartSFXCoroutine;
        
        private void Start()
        {
            if(!IsServerStarted)
                triggerVolume.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if(vacuumGui != null)
                Destroy(vacuumGui.gameObject);
            
            if(vacuumHead != null)
                Destroy(vacuumHead.gameObject);
            
            if(vacuumBody != null)
                Destroy(vacuumBody.gameObject);
        }

        public override void OnStartClient()
        {
            base.OnStartNetwork();
        }

        private void InitialiseVacuum()
        {
            parentRigidbody = GetComponentInParent<Rigidbody>();
            var parentNo = parentRigidbody.GetComponent<NetworkObject>();
            if(!parentNo)
                return;
            
            //if already attached to an owner
            if (parentNo.IsOwner)
            {
                GiveOwnership(parentNo.Owner);
                vacuumGui = Instantiate(Resources.Load<GameObject>("VacuumCanvas")).GetComponent<VacuumGui>();
                VacuumPowerManager.Instance.Initialise(maxPower, useRatePerSec, rechargeRatePerSec);
                VacuumPowerManager.Instance.OnPowerDepleted += () =>
                {
                    UseItem(false);
                };
                
                //dirty hack to attach the head correctly.
                var camera = Camera.main.transform;
                var temp = camera.transform.localRotation;
                camera.transform.localRotation = Quaternion.identity;
                vacuumHead.parent = Camera.main.transform;
                // lets move the head locally slightly
                vacuumHead.transform.localPosition += new Vector3(0.05f, 0.1f, 0f);
                vacuumBody.gameObject.SetActive(false);
                camera.transform.localRotation = temp;
            }
        }
        
        //Only called by player controller, which will only happen locally
        public void UseItem(bool isUsing)
        {
            Debug.Log($"Vacuum {isUsing} (IsOwner = {IsOwner})");
            if(localUsingFlag == isUsing) return;
            if(isUsing && !VacuumPowerManager.Instance.HasPower)
                return;
            localUsingFlag = isUsing;
            RPC_SendActivationRequestToServer(isUsing);
            VacuumPowerManager.Instance.SetVacuumActiveStatus(isUsing);
            Debug.Log($"Vacuum Activation Request Sent!!");
        }

        [ServerRpc]
        void RPC_SendActivationRequestToServer(bool isUsing)
        {
            if (IsServerStarted)
            {
                ServerVacuumActivation(isUsing);
            }
        }
        
        void ServerVacuumActivation(bool use)
        {
            Debug.Log($"Vacuum [Server] ACTIVATED {use}");
            VacuumActive.Value = use;
            //clear cache
            vacuumCache.Clear();
            triggerVolume.gameObject.SetActive(use);
            RPC_BroadcastActivationToClients(use);
        }

        [ObserversRpc]
        void RPC_BroadcastActivationToClients(bool use)
        {
            //clear cache
            vacuumCache.Clear();
            ClientVacuumActivation(use);
        }
        
        void ClientVacuumActivation(bool use)
        {
            //Animate Visuals only here.
            if(vacuumStartSFXCoroutine != null)
                StopCoroutine(vacuumStartSFXCoroutine);

            if (use)
            {
                particles.Play();
                vacuumStartSFXCoroutine = StartCoroutine(AudioSFXStart());
            }
            else
            {
                particles.Stop();
                audioSource.clip = endClip;
                audioSource.loop = false;
                audioSource.Play();
            }
        }

        IEnumerator AudioSFXStart()
        {
            audioSource.PlayOneShot(startClip);
            yield return new WaitForSeconds(startClip.length);
            audioSource.clip = midClip;
            audioSource.loop = true;
            audioSource.Play();
            
        }
        
        private void Update()
        {
            if(vacuumGui == null) return;
            
            vacuumGui.SetPowerPercentage(VacuumPowerManager.Instance.PowerPercentage);
        }

        void FixedUpdate()
        {
            if(!VacuumActive.Value)
                return;

            ApplySuction();
        }

        private void ApplySuction()
        {
            var colliders = Physics.OverlapSphere(triggerVolume.transform.position, triggerVolume.radius, layerMask);
            foreach (var col in colliders)
            {
                //multiple get-components, are very slow especially when inside an x-update loop.
                //so cache them on their first fetch, so we don't need to fetch them in the next frame.
                var vacuumCache = GetCacheStruct(col);
                if (vacuumCache.Equals(default(VacuumCacheStruct)))
                {
                    continue;
                }

                var rb = vacuumCache.RigidBody;
                if(rb == null)
                    continue;
                
                // don't apply on self.
                if (rb == parentRigidbody)
                {
                    continue;
                }
                
                // check if in line of sight, we don't want things in the other room to be sucked in.
                if(!InLineOfSight(rb))
                    return;
                
                // if this collider does not have authority here, ignore affecting it.
                var nob = vacuumCache.NObj;
                if (IsClientStarted && nob && !nob.IsOwner)
                {
                    continue;
                }

                // if player, use appropriate force value - for they are heavier!!!.
                var force = vacuumCache.Role == null ? vacuumSuctionRegular : vacuumSuctionPlayer;
                var rbPos = (rb.centerOfMass + rb.position);
                // apply suction force.
                var dir = (suctionPoint.position - rbPos).normalized;
                rb.AddForce(dir * force, ForceMode.Force);
                
                // distance check to if they can be considered as vacuumed.
                var dist = Vector3.Distance(rbPos, suctionPoint.position);
                Debug.Log($"Vacumm Suction::Distance To {rb.name} = {dist}m");
                if (dist <= suctionCompleteDetectionRadius)
                {
                    Capture(rb);
                }
            }
        }
        
        private void Capture(Rigidbody rb)
        {
            //detect for player captured
            var playerRole = rb.GetComponent<PlayerRole>();
            var nob = rb.GetComponent<NetworkObject>();
            
            //check if player is sucked in
            if (playerRole != null)
            {
                //Run only on the Authority holding client.
                if(!playerRole.IsOwner)
                    return;
                
                //Ignore if not player
                if(playerRole.Role != PlayerRole.RoleType.Hider)
                    return;
                
                Debug.Log($"<color=cyan>VACUUM SUCKK!!: Player:{playerRole.Role}, Owner: {playerRole.IsOwner}, Server:{IsServerStarted}</color>");
                if (!suckedPlayer)
                {
                    GameController.Instance.ServerHiderCaptured();
                    suckedPlayer = true;
                }
                return;
            }
            
            //check if its a normal item
            if (IsServerStarted)
            {   
                var item = rb.GetComponent<IVacuumDestroyable>();
                item?.OnVacuumed();

                nob.Despawn();   
            }
        }
        
        private bool InLineOfSight(Rigidbody toCheck)
        {
            RaycastHit hit;
            var direction = ((toCheck.position + toCheck.centerOfMass) - suctionPoint.position).normalized;
            var ray = new Ray( suctionPoint.position, direction);
            if (Physics.Raycast(ray, out hit, 50.0f, layerMask, QueryTriggerInteraction.Ignore))
            {
                var hitCollider = hit.collider;
                Debug.DrawLine(suctionPoint.position, hit.point, Color.red);
                var cache = GetCacheStruct(hitCollider);
                if (cache.Equals(default(VacuumCacheStruct)))
                    return false;
                
                return toCheck == cache.RigidBody;
            }
            
            return false;
        }
        
        private VacuumCacheStruct GetCacheStruct(Collider collider)
        {
            if(vacuumCache.TryGetValue(collider, out var cacheStruct))
                return cacheStruct;
            
            var nob = collider.GetComponentInParent<NetworkObject>();
            if(!IsServerStarted && nob && !nob.IsOwner)
                return default(VacuumCacheStruct);
                
            var rb = collider.GetComponentInParent<Rigidbody>();
            var role = collider.GetComponentInParent<PlayerRole>();
            var cacheEntry = new VacuumCacheStruct(nob, rb, role);
            vacuumCache.Add(collider,cacheEntry);
            return cacheEntry;
        }
        
        private void OnDrawGizmos()
        {
            if(!suctionPoint)
                return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(suctionPoint.position, suctionCompleteDetectionRadius);
            if (triggerVolume.gameObject.activeInHierarchy)
            {
                Gizmos.color = new Color(0f, 0f, 1f, 0.25f);
                var scale = triggerVolume.transform.localScale;
                Gizmos.DrawSphere(triggerVolume.center + triggerVolume.transform.position, triggerVolume.radius);
            }
        }

        public void OnAttach(Transform parentTrf)
        {
            transform.SetParent(parentTrf);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            
            
            Debug.Log("Vacuum:::OnAttach");
            if(IsOwner)
                InitialiseVacuum();
            else
            {
                var target = GetComponentInParent<SeekerLocators>();
                
                //hack to reliably attach to seeker's head
                //Todo: Fix this Issue where the vacuum head does not maintain consistant rotation
                
                var temp = target.SeekerHeadNonOwner.localRotation;
                target.SeekerHeadNonOwner.localRotation = Quaternion.identity;
                vacuumHead.parent = target.SeekerHeadNonOwner;
                target.SeekerHeadNonOwner.localRotation = temp;
                
                vacuumBody.parent = target.SeekerBodyNonOwner;
            }
        }

        public void OnDetach(Transform parentTrf, bool spawnWorldDummy)
        {
            Debug.Log("Vacuum:::OnDetach");
            var dummySpawnLoc = parentTrf.position + new Vector3(0,1,0) + parentTrf.forward * 2f;
            RPC_ServerRequestDespawn(dummySpawnLoc, spawnWorldDummy);
        }

        public string GetUsePromptText()
        {
            return "Use vacuum";
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