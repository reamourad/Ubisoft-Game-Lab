using System;
using System.Collections;
using FishNet.Object;
using GogoGaga.OptimizedRopesAndCables;
using Player.Data;
using UnityEngine;

namespace Player.Items.HiderItems.Reaction
{
    public class Fan : NetworkBehaviour, IReactionItem, IHiderGrabableItem
    {
        [SerializeField] private StationaryEffect effects;
        [SerializeField] private Transform spinTransform;
        [SerializeField] private float rotationSpeed = 5;
        [SerializeField] private GameObject windFx;
        [SerializeField] private float radius;
        [SerializeField] private float range;

        private bool isSpinning;
        public Rope rope { get; set; }

        public void OnTrigger(ITriggerItem triggerItem)
        {
            OnServerBlow();
        }

        [Server]
        private void OnServerBlow()
        {
            RPC_OnClientBlow();
            ApplyWindEffect();
            // StartCoroutine(DelayedDespawn());
        }

        [Server]
        private void ApplyWindEffect()
        {
            var colliders = Physics.OverlapCapsule(transform.position, transform.position + transform.up * range,
                radius);
            foreach (var collider in colliders)
            {
                if (ValidCollider(collider, out var stationaryObject))
                {
                    stationaryObject.ApplyStationaryEffect(effects);
                }
            }
        }
        
        [ObserversRpc]
        private void RPC_OnClientBlow()
        {
            var effect = Instantiate(windFx, transform.position, Quaternion.identity);
            effect.transform.SetParent(transform);
            effect.transform.forward = Vector3.up;
            isSpinning = true;
            Destroy(effect,3);
        }

        private void Update()
        {
            if(!isSpinning) return;
            SpinFan();
        }

        [Server]
        IEnumerator DelayedDespawn()
        {
            yield return new WaitForSeconds(0.15f);
            Despawn();
        }

        private void SpinFan()
        {
            spinTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        private bool ValidCollider(Collider collider, out StationaryObjectBase stationaryObject)
        {
            //Check if above the object
            stationaryObject = null;
            stationaryObject = collider.GetComponentInParent<StationaryObjectBase>();
            if (stationaryObject == null)
                return false;
            return true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            DrawWireCapsule(transform.position, transform.position + transform.up * range);
        }

        private void DrawWireCapsule(Vector3 p1, Vector3 p2)
        {
            Gizmos.DrawWireSphere(p1, radius);
            Gizmos.DrawWireSphere(p2, radius);

            var up = Vector3.up * radius;
            var forward = Vector3.forward * radius;
            var right = Vector3.right * radius;

            Gizmos.DrawLine(p1 + up, p2 + up);
            Gizmos.DrawLine(p1 - up, p2 - up);
            Gizmos.DrawLine(p1 + forward, p2 + forward);
            Gizmos.DrawLine(p1 - forward, p2 - forward);
            Gizmos.DrawLine(p1 + right, p2 + right);
            Gizmos.DrawLine(p1 - right, p2 - right);
        }
    }
}