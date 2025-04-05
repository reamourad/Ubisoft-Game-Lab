using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GogoGaga.OptimizedRopesAndCables;
using Player.Data;
using UnityEngine;

namespace Player.Items.HiderItems.Reaction
{
    public class SmokeBomb : NetworkBehaviour, IReactionItem, IHiderGrabableItem
    {
        [SerializeField] private LayerMask stationaryLayerMask;
        [SerializeField] private StationaryEffect effects;
        [SerializeField] private float range;
        [SerializeField] private GameObject SmokeFx;
        [SerializeField] private float fxKillTime = 5;
        [SerializeField] private Transform losSrcTrf;
        
        [Header("Tests")]
        [SerializeField] private bool showBounds;
        [SerializeField] private Transform targetAlignTest;
        
        public Rope rope { get; set; }
        public void OnTrigger(ITriggerItem triggerItem)
        {
            Debug.Log("Smoke Triggered");
            OnServerExplode();
        }
        
        [Server]
        private void OnServerExplode()
        {
            ApplySmokeEffect();
            RPC_OnClientExplode();
            StartCoroutine(DelayedDespawn());
        }

        [Server]
        private void ApplySmokeEffect()
        {
            var colliders = Physics.OverlapSphere(transform.position, range, stationaryLayerMask);
            foreach (var col in colliders)
            {
                if (ValidCollider(col, out StationaryObjectBase stationaryObject))
                {
                    stationaryObject.ApplyStationaryEffect(effects);
                }
            }
        }

        private bool ValidCollider(Collider collider, out StationaryObjectBase stationaryObject)
        {
            //Check if above the object
            stationaryObject = null;
            var stationary = collider.GetComponentInParent<StationaryObjectBase>();
            if (stationary == null)
                return false;
            
            var dir = (stationary.transform.position - transform.position).normalized;
            var up = Vector3.up;
            var aligned = Vector3.Dot(dir, up) > 0; //everything above ground
            if (!aligned)
                return false;
            
            //if not in line of sight (ie; assuming a wall is in the )
            if (Physics.Raycast(losSrcTrf.position, dir, out RaycastHit hit, range))
            {
                if (hit.collider != collider)
                {
                    return false;
                }
            }

            stationaryObject = stationary;
            return true;
        }
        
        [Server]
        IEnumerator DelayedDespawn()
        {
            yield return new WaitForSeconds(0.15f);
            Despawn();
        }
        
        [ObserversRpc]
        private void RPC_OnClientExplode()
        {
            Debug.Log("TACTICAL SMOKE");
            var go = Instantiate(SmokeFx, transform.position, Quaternion.identity);
            Destroy(go,fxKillTime);
        }

        private void OnDisable()
        {
            if (rope != null)
            {
                rope.gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            if(!showBounds)
                return;
            
            var color = Color.white;
            color.a = 0.25f;
            Gizmos.color = color;
            Gizmos.DrawSphere(transform.position, range);

            if(targetAlignTest == null)
                return;
            
            Vector3 worldUp = Vector3.up;
            Vector3 dir = (targetAlignTest.position - transform.position).normalized;
            
            float dot = Vector3.Dot(worldUp, dir);
            color =  dot > 0 ? Color.green : Color.red;
            Debug.DrawRay(transform.position, dir * range, color);
        }
    }
}