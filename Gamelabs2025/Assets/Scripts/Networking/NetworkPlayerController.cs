using System;
using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using Items.Interfaces;
using UnityEngine;

namespace Networking
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(NetworkTransform))]
    public class NetworkPlayerController : NetworkBehaviour
    {
        [Serializable]
        public struct PlayerInputData
        {
            public float PitchSensitivity;
            public float YawSensitivity;
            public Vector2 MoveInputVector;
            public Vector2 LookInputVector;

            public PlayerInputData(float pitchSensitivity, float yawSensitivity, Vector2 moveInputVector, Vector2 lookInputVector)
            {
                PitchSensitivity = pitchSensitivity;
                YawSensitivity = yawSensitivity;
                MoveInputVector = moveInputVector;
                LookInputVector = lookInputVector;
            }
        }
        
        [SerializeField] Transform cameraTransform;
        [SerializeField] private float speed=1f;
        [SerializeField] private float maxLookAngle=80f;

        [SerializeField] private IUsableItem usableItem;
        
        private PlayerInputData playerInputData;
        private Rigidbody rb;
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            name = $"Player [{(IsOwner ? "LOCAL_PLAYER" : "REMOTE_PLAYER")}]";
        }

        public void UpdatePlayerInputs(PlayerInputData inputData)
        {
            this.playerInputData = inputData;
        }

        public void UseItem(bool isUsing)
        {
            if (usableItem == null)
            {
                usableItem = GetComponentInChildren<IUsableItem>();
            }

            usableItem?.UseItem(isUsing);
        }
        
        private void Update()
        {
            if(!IsOwner)
                return;
            
            UpdateView(playerInputData, Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if(!IsOwner)
                return;
            
            UpdatePlayerMovement(playerInputData, Time.fixedDeltaTime);
        }
        
        private void UpdatePlayerMovement(PlayerInputData inputData, float deltaTime)
        {
            var rotation = transform.rotation;
            var forward = rotation * Vector3.forward;
            var right = rotation * Vector3.right;
            
            Vector3 move = (forward * inputData.MoveInputVector.y + right * inputData.MoveInputVector.x) * speed;
            rb.MovePosition(transform.position + move * deltaTime);
            
        }

        private void UpdateView(PlayerInputData inputData, float deltaTime)
        {
            float pitchInput = inputData.LookInputVector.y;
            float yawInput = inputData.LookInputVector.x;
            
            Vector3 localCameraEuler = cameraTransform.localEulerAngles;
            float pitch = localCameraEuler.x > 180 ? localCameraEuler.x - 360.0f : localCameraEuler.x;
            pitch = Mathf.Clamp(pitch - pitchInput * inputData.PitchSensitivity * deltaTime, -maxLookAngle, maxLookAngle);
            localCameraEuler.x = pitch < 0.0f? pitch + 360.0f : pitch;
            
            cameraTransform.localEulerAngles = localCameraEuler;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(Vector3.up * yawInput * inputData.YawSensitivity * deltaTime));
        }
        
    }
}