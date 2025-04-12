using System;
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
        private CinemachineBrain cineBrain;
        
        private Vector3 startingPosition;
        private GameObject spawnedSeekerTutorial;
        private GameObject spawnedHiderTutorial;

        public override void OnStartClient()
        {
            base.OnStartClient();
            StartCoroutine(Initialise());
        }

        private IEnumerator Initialise()
        {
            Debug.Log($"Initialising PlayerInitialiser");
            yield return new WaitForEndOfFrame();
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
            Debug.Log($"Initialising Owner");
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
                Debug.Log(transform.position);
                Debug.Log(GameController.Instance);
                Debug.Log(GameController.Instance.HiderSpawn);
                
                transform.position = GameController.Instance.SeekerSpawn.position;
                startingPosition = transform.position;
                var fpsCam = GetComponentInChildren<CinemachineCamera>();
                fpsCam.enabled = true;
                var camTrf = Camera.main.transform;
                camTrf.parent = fpsCam.transform;
                camTrf.localPosition = Vector3.zero;
                camTrf.localRotation = Quaternion.identity;
                
                GetComponentInChildren<NetworkPlayerController>().SetCameraTransform(camTrf);
                if (fpsGraphicsManager != null)
                    fpsGraphicsManager.SetRendererEnabled(false);

                GameObject prefab = Resources.Load<GameObject>("SeekerTutorial");

                spawnedSeekerTutorial = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                spawnedSeekerTutorial.transform.SetParent(GameObject.Find("Canvas")?.transform, false);

                
            }));
            
        }

        private void OwnerHiderInitialisation()
        {
            Debug.Log($"Initialising Owner Hider");
            StartCoroutine(Delayed(() =>
            {
                if(hiderCameraPrefab == null || hiderCameraTargetTransform == null)
                    return ;
                
                Debug.Log(transform.position);
                Debug.Log(GameController.Instance);
                Debug.Log(GameController.Instance.HiderSpawn);
                transform.position = GameController.Instance.HiderSpawn.position;
                startingPosition = transform.position;
                
                Debug.Log("Loading TPS Camera!!");
                hiderPlayerCamera = Instantiate(hiderCameraPrefab, hiderCameraTargetTransform.position, Quaternion.identity);
                hiderPlayerCamera.GetComponent<CameraObstructionHandler>().player = this.transform;
                var cineCam = hiderPlayerCamera.GetComponent<CinemachineCamera>();
                GetComponentInChildren<NetworkPlayerController>().SetCameraTransform(cineCam.transform);
                cineCam.Target.TrackingTarget = hiderCameraTargetTransform;
                cineCam.Target.LookAtTarget = hiderCameraTargetTransform;
                Debug.Log("Loading TPS Camera!! Done");
                Instantiate(Resources.Load<GameObject>("HiderCanvas"), hiderCameraTargetTransform);
                
                //Set to manual update.
                cineBrain = CinemachineBrain.GetActiveBrain(0);
                if (cineBrain != null)
                {
                    cineBrain.UpdateMethod = CinemachineBrain.UpdateMethods.ManualUpdate;
                }

                GameObject prefab = Resources.Load<GameObject>("HiderTutorial");

                spawnedHiderTutorial = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                spawnedHiderTutorial.transform.SetParent(GameObject.Find("Canvas")?.transform, false);

            }));
        }

        private void Update()
        {
            if(!IsOwner)
                return;
            
            if(cineBrain != null)
                cineBrain.ManualUpdate();
        }

        IEnumerator Delayed(System.Action callback)
        {
            yield return new WaitUntil(()=>  IsClientInitialized);
            yield return new WaitWhile(() => Camera.main == null);
            callback?.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if(!IsOwner) return;
            
            if(other.CompareTag("Respawn"))
                transform.position = startingPosition;
        }
    }
}