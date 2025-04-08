using System;
using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using Items.Interfaces;
using Unity.Cinemachine;
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

        public enum CameraType
        {
            None = 0,
            FirstPerson,
            ThirdPerson,
        }
        
        [SerializeField] Transform cameraTransform;
        [SerializeField] private float speed=1f;
        [SerializeField] private float maxLookAngle=80f;

        [Tooltip("FPS - Uses mouse (X,Y) to update view, TPS - Uses movement direction to handle rotation.")]
        [SerializeField] private CameraType cameraType=CameraType.None;
        [SerializeField] private float tpsLookSpeed=100;
        
        [SerializeField] private IUsableItem usableItem;
        
        private PlayerInputData playerInputData;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void SetCameraTransform(Transform cameraTransform)
        {
            this.cameraTransform = cameraTransform;
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
            usableItem = GetComponentInChildren<IUsableItem>();
            usableItem?.UseItem(isUsing);
        }
        
        private void Update()
        {
            if(!IsOwner)
                return;

            //Update camera after
            UpdateFirstPersonView(playerInputData, Time.deltaTime);
            
        }

        private void FixedUpdate()
        {
            if(!IsOwner)
                return;
            
            UpdatePlayerMovement(playerInputData, Time.fixedDeltaTime);
        }

        private void UpdatePlayerMovement(PlayerInputData inputData, float deltaTime)
        {
            if (cameraType == CameraType.FirstPerson)
                FPSMove(inputData, deltaTime);
            else if (cameraType == CameraType.ThirdPerson)
                TPSMove(inputData, deltaTime);
        }

        private void FPSMove(PlayerInputData inputData, float deltaTime)
        {
            var rotation = transform.rotation;
            var forward = rotation * Vector3.forward;
            var right = rotation * Vector3.right;
            var dir = (forward * inputData.MoveInputVector.y + right * inputData.MoveInputVector.x).normalized;
            var move = dir * (speed / 10f);
            rb.MovePosition(rb.position + move * deltaTime);
        }

        private void TPSMove(PlayerInputData inputData, float deltaTime)
        {
            if(cameraTransform == null)
                return;
            
            var forward = cameraTransform.transform.forward;
            var right = cameraTransform.transform.right;
            forward.y = 0;
            right.y = 0;
            var currPos = rb.position;
            var dir = (forward * inputData.MoveInputVector.y + right * inputData.MoveInputVector.x).normalized;
            var move = dir * (speed / 10f);
            rb.MovePosition(currPos + move * deltaTime);
            var newPos = rb.position;
            var newMoveDir = (newPos - currPos).normalized;
            newMoveDir.y = 0;
            if (newMoveDir != Vector3.zero)
            {
                transform.forward = Vector3.Lerp(transform.forward, newMoveDir, Time.deltaTime * tpsLookSpeed);
            }
        }
        
        private void UpdateFirstPersonView(PlayerInputData inputData, float deltaTime)
        {
            if(cameraType == CameraType.None || cameraType == CameraType.ThirdPerson)
                return;
            
            if(cameraTransform == null)
                return;
            
            var cameraRot = cameraTransform.eulerAngles;
            var newRot = new Vector3(transform.eulerAngles.x, cameraRot.y, transform.eulerAngles.z);
            transform.rotation = Quaternion.Euler(newRot);
        }

        public Transform GetCamera()
        {
            return cameraTransform;
        }

        public Vector3 GetCameraForwardZeroedYNormalised()
        {
            return GetCameraForwardZeroedY().normalized;
        }
        
        public Vector3 GetCameraRightZeroedYNormalised()
        {
            return GetCameraRightZeroedY().normalized;
        }
        
        private Vector3 GetCameraForwardZeroedY()
        {
            return new Vector3(cameraTransform.transform.forward.x, 0, cameraTransform.transform.forward.z);
        }
        
        private Vector3 GetCameraRightZeroedY()
        {
            return new Vector3(cameraTransform.transform.right.x, 0, cameraTransform.transform.right.z);
        }
    }
}