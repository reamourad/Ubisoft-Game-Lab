using System.Collections;
using System.Threading;
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
        [SerializeField]
        private GameObject seekerRightHandTrf;
        
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
            StartCoroutine(Delayed(() =>
            {
                var fpsCam = GetComponentInChildren<CinemachineCamera>();
                fpsCam.enabled = true;
                var camTrf = Camera.main.transform;
                camTrf.parent = fpsCam.transform;
                camTrf.localPosition = Vector3.zero;
                camTrf.localRotation = Quaternion.identity;
                
                //Move the hand under camera
                seekerRightHandTrf.transform.parent = camTrf;
                seekerRightHandTrf.transform.localPosition = Vector3.zero;
                seekerRightHandTrf.transform.localRotation = Quaternion.identity;
                
                GetComponentInChildren<NetworkPlayerController>().SetCameraTransform(camTrf);
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