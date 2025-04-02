using System.Collections;
using System.Threading;
using FishNet.Object;
using Player;
using StateManagement;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Networking
{
    public class PlayerInitialiser : NetworkBehaviour
    {
        [Header("Seeker"), SerializeField]
        private SeekerGraphicsManager fpsGraphicsManager;

        [SerializeField]
        private GameObject playerGraphics;
        
        [Header("Hider"), SerializeField]
        
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
            GetComponentInChildren<Player.PlayerInputController>().enabled = false;
        }
        
        private void NonOwnerHiderInitialisation()
        {
            GetComponentInChildren<Player.PlayerInputController>().enabled = false;
        }
        
        private void OwnerIntialisation(PlayerRole.RoleType playerRole)
        {
            GameLookupMemory.LocalPlayer = this.gameObject;
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
            if(playerGraphics != null)
                playerGraphics.transform.localRotation = Quaternion.identity;
            StartCoroutine(Delayed(() =>
            {
                var fpsCam = GetComponentInChildren<CinemachineCamera>();
                fpsCam.enabled = true;
                var camTrf = Camera.main.transform;
                camTrf.parent = fpsCam.transform;
                camTrf.localPosition = Vector3.zero;
                camTrf.localRotation = Quaternion.identity;
                
                GetComponentInChildren<NetworkPlayerController>().SetCameraTransform(camTrf);
                if (fpsGraphicsManager != null)
                    fpsGraphicsManager.SetRendererEnabled(false);
            }));
            
        }

        private void OwnerHiderInitialisation()
        {
            StartCoroutine(Delayed(() =>
            {
                if(hiderCameraPrefab == null || hiderCameraTargetTransform == null)
                    return ;
            
                Debug.Log("Loading TPS Camera!!");
                hiderPlayerCamera = Instantiate(hiderCameraPrefab, hiderCameraTargetTransform.position, Quaternion.identity);
                hiderPlayerCamera.GetComponent<CameraObstructionHandler>().player = this.transform;
                var cineCam = hiderPlayerCamera.GetComponent<CinemachineCamera>();
                GetComponentInChildren<NetworkPlayerController>().SetCameraTransform(cineCam.transform);
                cineCam.Target.TrackingTarget = hiderCameraTargetTransform;
                cineCam.Target.LookAtTarget = hiderCameraTargetTransform;
                Debug.Log("Loading TPS Camera!! Done");
            }));
        }

        IEnumerator Delayed(System.Action callback)
        {
            yield return new WaitUntil(()=>IsClientInitialized);
            yield return new WaitWhile(() => Camera.main == null);
            callback?.Invoke();
        }
    }
}