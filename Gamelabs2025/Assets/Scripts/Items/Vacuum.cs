using System;
using System.Collections.Generic;
using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items.Interfaces;
using UnityEngine;

namespace Items
{
    public class Vacuum : NetworkBehaviour, IUsableItem
    {
        [SerializeField] private SphereCollider triggerVolume;
        [SerializeField] private Transform suctionPoint;
        [SerializeField] private float suctionCompleteDetectionRadius=0.25f;
        
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float vacuumSuctionForce = 10;
        
        [SerializeField] private ParticleSystem particles;
        
        private bool localUsingFlag = false;
        private readonly SyncVar<bool> VacuumActive = new SyncVar<bool>();

        private void Start()
        {
            if(!IsServerStarted)
                triggerVolume.gameObject.SetActive(false);
        }
        
        public override void OnStartClient()
        {
            base.OnStartNetwork();
            var parentNo = GetComponent<NetworkTransform>();
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
            //ClientVacuumActivation(isUsing);
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
            triggerVolume.gameObject.SetActive(use);
            RPC_BroadcastActivationToClients(use);
        }

        [ObserversRpc]
        void RPC_BroadcastActivationToClients(bool use)
        {
            ClientVacuumActivation(use);
        }
        
        void ClientVacuumActivation(bool use)
        {
            //Animate Visuals only here.
            Debug.Log($"Vacuum [Client] ACTIVATED {use}");
            if(use)
                particles.Play();
            else
            {
                particles.Stop();
            }
        }

        void FixedUpdate()
        {
            if(!IsServerStarted)
                return;
            
            if(!VacuumActive.Value)
                return;
            
            var colliders = Physics.OverlapSphere(triggerVolume.transform.position, triggerVolume.radius, layerMask);
            Debug.Log($"Vacuum [FixedUpdate] SUCKABLE: {colliders.Length}");
            foreach (var col in colliders)
            {
                var rb = col.GetComponentInParent<Rigidbody>();
                var dir = (suctionPoint.position - rb.position).normalized;
                rb.AddForce(dir * vacuumSuctionForce, ForceMode.Force);

                if (Vector3.Distance(rb.position, suctionPoint.position) <= suctionCompleteDetectionRadius)
                {
                    ServerVacuumedItem(rb);
                }
            }
        }

        private void ServerVacuumedItem(Rigidbody rb)
        {
            Debug.Log("Item Vacuumed " + rb.name);
            if (rb.TryGetComponent<NetworkObject>(out var nob))
            {
                nob.Despawn();
            }
        }
        
        /*private void OnTriggerEnter(Collider other)
        { 
            Debug.Log("TRIGGER ENTER");
            if(!IsServerStarted)
                return;
            
            if(!VacuumActive.Value)
                return;
            
            //only add valid colliders
            if (other.gameObject.layer != layerMask)
            {
                Debug.Log("Invalid Layer mask");
                return;
            }

            //check if physics capable
            if (other.TryGetComponent<Rigidbody>(out var rb))
            {
                if(!serverRigidBodiesInRange.Contains(rb))
                    serverRigidBodiesInRange.Add(rb);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(!IsServerStarted)
                return;

            if(!VacuumActive.Value)
                return;
            
            if (other.TryGetComponent<Rigidbody>(out var rb))
            {
                if(serverRigidBodiesInRange.Contains(rb))
                    serverRigidBodiesInRange.Remove(rb);
            }
        }*/

        private void OnDrawGizmos()
        {
            if(!suctionPoint)
                return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(suctionPoint.position, suctionCompleteDetectionRadius);
        }
    }
}