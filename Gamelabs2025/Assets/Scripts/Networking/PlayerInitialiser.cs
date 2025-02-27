using FishNet.Object;
using Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Networking
{
    public class PlayerInitialiser : NetworkBehaviour
    {
        [SerializeField]
        private GameObject hiderCameraPrefab;
        [FormerlySerializedAs("hiderCameraTransform")] [SerializeField]
        private Transform hiderCameraTargetTransform;
        
        private GameObject hiderPlayerCamera;
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            var playerRole = GetComponent<PlayerRole>().Role;
            if (!IsOwner)
                NonOwnerIntialisation(playerRole);
            else
                OwnerIntialisation(playerRole);
        }

        private void NonOwnerIntialisation(PlayerRole.RoleType playerRole)
        {
            
            switch (playerRole)
            {
                case PlayerRole.RoleType.Seeker:
                    NonOwnerSeekerInitialisation();
                    break;
                case PlayerRole.RoleType.Hider:
                    NonOwnerHiderInitialisation();
                    break;
            }
        }

        private void NonOwnerSeekerInitialisation()
        {
            GetComponentInChildren<TestPlayerInputController>().enabled = false;
        }
        
        private void NonOwnerHiderInitialisation()
        {
            GetComponentInChildren<TestPlayerInputController>().enabled = false;
        }
        
        private void OwnerIntialisation(PlayerRole.RoleType playerRole)
        {
            switch (playerRole)
            {
                case PlayerRole.RoleType.Seeker:
                    OwnerSeekerInitialisation();
                    break;
                case PlayerRole.RoleType.Hider:
                    OwnerHiderInitialisation();
                    break;
            }
        }

        private void OwnerSeekerInitialisation()
        {
            //nothing as of now.
            GetComponentInChildren<CinemachineCamera>().enabled = true;
        }

        private void OwnerHiderInitialisation()
        {
            if(hiderCameraPrefab == null || hiderCameraTargetTransform == null)
               return;
            
            hiderPlayerCamera = Instantiate(hiderCameraPrefab, hiderCameraTargetTransform.position, Quaternion.identity);
            hiderPlayerCamera.GetComponent<CameraObstructionHandler>().player = this.transform;
            var cineCam = hiderPlayerCamera.GetComponent<CinemachineCamera>();
            GetComponentInChildren<NetworkPlayerController>().SetCameraTransform(cineCam.transform);
            cineCam.Target.TrackingTarget = hiderCameraTargetTransform;
            cineCam.Target.LookAtTarget = hiderCameraTargetTransform;
        }
    }
}