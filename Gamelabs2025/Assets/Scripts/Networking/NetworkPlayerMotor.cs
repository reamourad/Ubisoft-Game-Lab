using System;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

namespace Networking
{
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkPlayerMotor : NetworkBehaviour
    {
        [Serializable]
        public struct PlayerMoveData
        {
            public Vector2 MoveInputVector;
            public Vector2 LookInputVector;

            public PlayerMoveData(Vector2 moveInputVector, Vector2 lookInputVector)
            {
                MoveInputVector = moveInputVector;
                LookInputVector = lookInputVector;
            }
        }

        [SerializeField] private float speed=1f;
        [SerializeField] private float pitchSensitivity=10f;
        [SerializeField] private float yawSensitivity = 100f;
        [SerializeField] private float maxLookAngle=80f;
        [SerializeField] Transform cameraTransform;
        
        private PlayerMoveData playerMoveData;
        private Rigidbody rb;
        private float cameraPitch;
        
        private void Awake()
        {
           rb = GetComponent<Rigidbody>();
        }
        
        public override void OnStartNetwork()
        {
            TimeManager.OnTick += TimeManagerOnTick;
            TimeManager.OnPostTick += TimeManagerOnOnPostTick;
        }
        
        public override void OnStopNetwork()
        {
            TimeManager.OnTick -= TimeManagerOnTick;
            TimeManager.OnPostTick -= TimeManagerOnOnPostTick;
        }

        public void UpdatePlayerInputs(PlayerMoveData moveData)
        {
            this.playerMoveData = moveData;
        }
        
        private void TimeManagerOnTick()
        {
            if (IsOwner)
                PerformReplicate(new ReplicateData(playerMoveData));
            else
                PerformReplicate(default);
        }
        
        [Replicate]
        private void PerformReplicate(ReplicateData replicateData,ReplicateState replicateState= ReplicateState.Invalid, Channel channel=Channel.Unreliable)
        {
            UpdatePlayerMovement(replicateData.PlayerMoveData);
        }

        private void UpdatePlayerMovement(PlayerMoveData moveData)
        {
            Vector3 move = cameraTransform.forward * moveData.MoveInputVector.y + 
                           cameraTransform.right * moveData.MoveInputVector.x;
            move.y = 0f;
            rb.AddForce(move.normalized * speed);
            
            float mouseX = moveData.LookInputVector.x;
            float mouseY = moveData.LookInputVector.y;

            cameraPitch -= mouseY * pitchSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);

            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
            rb.MoveRotation(rb.rotation * Quaternion.Euler(Vector3.up * (mouseX * yawSensitivity)));
        }
        
        private void TimeManagerOnOnPostTick()
        {
            if(!IsServerStarted)
                return;
            
            CreateReconcile();
        }

        public override void CreateReconcile()
        {
            var data = new ReconcileData(rb.position, rb.rotation, rb.linearVelocity, rb.angularVelocity);
            PerformReconcile(data);
        }
        
        [Reconcile]
        private void PerformReconcile(ReconcileData reconcileData, Channel channel = Channel.Unreliable)
        {
            rb.Move(reconcileData.Position, reconcileData.Rotation);
            rb.linearVelocity = reconcileData.Velocity;
            rb.angularVelocity = reconcileData.AngularVelocity;
        }
        
        private struct ReplicateData : IReplicateData
        {
            private uint tick;
            public readonly PlayerMoveData PlayerMoveData;

            public ReplicateData(PlayerMoveData playerMoveData) : this()
            {
                tick = 0;
                PlayerMoveData = playerMoveData;
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

        private struct ReconcileData : IReconcileData
        {
            private uint tick;
            public readonly Vector3 Position;
            public readonly Quaternion Rotation;
            public readonly Vector3 Velocity;
            public readonly Vector3 AngularVelocity;

            public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity) : this()
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