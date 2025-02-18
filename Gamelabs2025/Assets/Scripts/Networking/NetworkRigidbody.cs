using System;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

namespace Networking
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkRigidbody : NetworkBehaviour
    {
        private Rigidbody rb;
        private Vector3 lastVelocity;
        private Vector3 lastAngularVelocity;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            lastVelocity = rb.linearVelocity;
            lastAngularVelocity = rb.angularVelocity;
        }
        
        public override void OnStartNetwork()
        {
            TimeManager.OnTick += TimeManagerOnOnTick; 
            TimeManager.OnPostTick += TimeManagerOnOnPostTick;
            
        }
        public override void OnStopNetwork()
        {
            TimeManager.OnTick -= TimeManagerOnOnTick; 
            TimeManager.OnPostTick -= TimeManagerOnOnPostTick;
        }
        
        private void TimeManagerOnOnTick()
        {
            /*if (IsOwner)
            {
                var deltaVelocity = rb.linearVelocity - lastVelocity;
                var deltaAngularVelocity = rb.angularVelocity - lastAngularVelocity;
                lastVelocity = rb.linearVelocity;
                lastAngularVelocity = rb.angularVelocity;
                var data = new ReplicateData(deltaVelocity, deltaAngularVelocity);
                PerformReplicate(data);
            }
            else
            {
                PerformReplicate(default);
            }*/
        }
        
        private void TimeManagerOnOnPostTick()
        {
            if(!IsServerStarted)
                return;

            CreateReconcile();
        }
        
        public override void CreateReconcile()
        {
            ReconcilationData data = new ReconcilationData(rb.position,rb.rotation, rb.linearVelocity, rb.angularVelocity);
            PerformReconcile(data);
        }

        [Replicate]
        private void PerformReplicate(ReplicateData data, ReplicateState state = ReplicateState.Invalid,
            Channel channel = Channel.Unreliable)
        {
            /*rb.linearVelocity += data.DelataLinearVelocity;
            rb.angularVelocity += data.DeltaAngularVelocity;*/
        }
        
        [Reconcile]
        void PerformReconcile(ReconcilationData data, Channel channel = Channel.Unreliable)
        {
            rb.Move(data.Position, data.Rotation);
            rb.angularVelocity = data.AngularVelocity;
            rb.linearVelocity = data.Velocity;
        }
        
        
        private struct ReplicateData : IReplicateData
        {
            private uint tick;
            public readonly Vector3 DelataLinearVelocity;
            public readonly Vector3 DeltaAngularVelocity;
            
            public ReplicateData(Vector3 delataLinearVelocity, Vector3 deltaAngularVelocity) : this()
            {
                tick = 0;

                DelataLinearVelocity = delataLinearVelocity;
                DeltaAngularVelocity = deltaAngularVelocity;
            }

            public uint GetTick()
            {
                return tick;
            }

            public void SetTick(uint value)
            {
                tick = value;
            }

            public void Dispose()
            {
               
            }
        }
        private struct ReconcilationData : IReconcileData
        {
            private uint tick;
            
            public readonly Vector3 Position;
            public readonly Quaternion Rotation;
            public readonly Vector3 Velocity;
            public readonly Vector3 AngularVelocity;

            public ReconcilationData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity) : this()
            {
                tick = 0;
                Position = position;
                Rotation = rotation;
                Velocity = velocity;
                AngularVelocity = angularVelocity;
            }

            public uint GetTick()
            {
                return tick;
            }

            public void SetTick(uint value)
            {
                tick = value;
            }

            public void Dispose()
            {
                
            }
        }
    }
}