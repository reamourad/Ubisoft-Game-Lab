using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items.Interfaces;
using UnityEngine;

namespace Items
{
    public class Vacuum : NetworkBehaviour, IUsableItem
    {
        [SerializeField] private Collider triggerVolume;
        [SerializeField] private Transform suctionPoint;
        [SerializeField] private float suctionCompleteDetectionRadius=0.25f;
        
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float vacuumSuctionForce = 10;
        
        [SerializeField] private ParticleSystem particles;
        List<Rigidbody> serverRigidBodiesInRange = new List<Rigidbody>();
        
        private bool localUsingFlag = false;
        private readonly SyncVar<bool> VacuumActive = new SyncVar<bool>();

        private void Start()
        {
            if(!IsServerStarted)
                triggerVolume.gameObject.SetActive(false);
        }

        public void UseItem(bool isUsing)
        {
            //don't use if not owner
            if(!IsOwner)
                return;
            
            if(localUsingFlag == isUsing) return;
            localUsingFlag = isUsing;
            RPC_SendActivationRequestAtServer(isUsing);
        }

        [ObserversRpc(ExcludeOwner = false)]
        void RPC_SendActivationRequestAtServer(bool use)
        {
            if (IsServerStarted)
                ServerVacummActivation(use);
            else
                ClientVacuumActivation(use);
        }

        void ServerVacummActivation(bool use)
        {
            VacuumActive.Value = use;
            triggerVolume.gameObject.SetActive(use);
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
            if(!IsServerStarted)
                return;
            var copyRbs = new List<Rigidbody>(serverRigidBodiesInRange);
            foreach (var rb in copyRbs)
            {
                var dir = (suctionPoint.position - rb.position).normalized;
                rb.AddForce(dir * vacuumSuctionForce, ForceMode.Impulse);

                if (Vector3.Distance(rb.position, suctionPoint.position) <= suctionCompleteDetectionRadius)
                {
                    ServerVacuumedItem(rb);
                }
            }
            copyRbs.Clear();
        }

        private void ServerVacuumedItem(Rigidbody rb)
        {
            Debug.Log("Item Vacuumed " + rb.name);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if(!IsServerStarted)
                return;
            
            //only add valid colliders
            if(other.gameObject.layer != layerMask)
                return;

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

            if (other.TryGetComponent<Rigidbody>(out var rb))
            {
                if(serverRigidBodiesInRange.Contains(rb))
                    serverRigidBodiesInRange.Remove(rb);
            }
        }

        private void OnDrawGizmos()
        {
            if(!suctionPoint)
                return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(suctionPoint.position, suctionCompleteDetectionRadius);
        }
    }
}