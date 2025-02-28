using System;
using System.Collections.Generic;
using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items.Interfaces;
using Player;
using StateManagement;
using UnityEngine;

namespace Items
{
    public class Vacuum : NetworkBehaviour, IUsableItem
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
        
        [SerializeField] private SphereCollider triggerVolume;
        [SerializeField] private Transform suctionPoint;
        [SerializeField] private float suctionCompleteDetectionRadius=0.25f;
        
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float vacuumSuctionRegular = 10;
        [SerializeField] private float vacuumSuctionPlayer = 10;
        
        [SerializeField] private ParticleSystem particles;
        
        private Dictionary<Collider, VacuumCacheStruct> vacuumCache = new Dictionary<Collider, VacuumCacheStruct>();
        
        private bool localUsingFlag = false;
        private readonly SyncVar<bool> VacuumActive = new SyncVar<bool>();
        
        private Rigidbody parentRigidbody;
        private bool suckedPlayer = false;
        
        private void Start()
        {
            if(!IsServerStarted)
                triggerVolume.gameObject.SetActive(false);
        }
        
        public override void OnStartClient()
        {
            base.OnStartNetwork();
            
            //move this to on-grab
            InitialiseVacuum();
        }

        private void InitialiseVacuum()
        {
            parentRigidbody = GetComponentInParent<Rigidbody>();
            var parentNo = parentRigidbody.GetComponent<NetworkObject>();
            if(!parentNo)
                return;
            
            //if already attached to an owner
            if(parentNo.IsOwner)
                GiveOwnership(parentNo.Owner);
        }
        
        public void UseItem(bool isUsing)
        {
            Debug.Log($"Vacuum {isUsing} (IsOwner = {IsOwner})");
            //don't use if not owner
            if(!IsOwner)
                return;
            
            if(localUsingFlag == isUsing) return;
            localUsingFlag = isUsing;
            RPC_SendActivationRequestToServer(isUsing);
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
            if(use)
                particles.Play();
            else
            {
                particles.Stop();
            }
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
                if(!playerRole.IsOwner)
                    return;
                if(playerRole.Role != PlayerRole.RoleType.Hider)
                    return;
                
                Debug.Log($"<color=cyan>VACUUM SUCKK!!: Player:{playerRole.Role}, Owner: {playerRole.IsOwner}, Server:{IsServerStarted}</color>");
                if(!suckedPlayer) 
                    GameController.Instance.ServerHiderCaptured();
                suckedPlayer = true;
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
                Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
                var scale = triggerVolume.transform.localScale;
                Gizmos.DrawSphere(triggerVolume.center + triggerVolume.transform.position, triggerVolume.radius * Mathf.Max(scale.x, scale.y, scale.z));
            }
        }
    }
}