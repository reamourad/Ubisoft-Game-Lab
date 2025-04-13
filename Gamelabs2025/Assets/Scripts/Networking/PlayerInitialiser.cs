using System;
using System.Collections;
using System.Threading;
using FishNet.Object;
using Player;
using StateManagement;
using Tutorial;
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

#if UNITY_EDITOR
        [SerializeField] private bool enableDebugButton = true;
#endif

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
                StartCoroutine(CheckFallingBelowY(-10f)); // Adjust the threshold as needed
                var fpsCam = GetComponentInChildren<CinemachineCamera>();
                fpsCam.enabled = true;
                var camTrf = Camera.main.transform;
                camTrf.parent = fpsCam.transform;
                camTrf.localPosition = Vector3.zero;
                camTrf.localRotation = Quaternion.identity;
                
                GetComponentInChildren<NetworkPlayerController>().SetCameraTransform(camTrf);
                if (fpsGraphicsManager != null)
                    fpsGraphicsManager.SetRendererEnabled(false);

                TutorialManager.Instance.SpawnTutorialUI(PlayerRole.RoleType.Seeker);
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
                StartCoroutine(CheckFallingBelowY(-10f)); // Adjust the threshold as needed
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
                
                TutorialManager.Instance.SpawnTutorialUI(PlayerRole.RoleType.Hider);
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

        /*private void OnTriggerEnter(Collider other)
        {
            if(!IsOwner) return;
            
            if(other.CompareTag("Respawn"))
                transform.position = startingPosition;
        }*/

        private IEnumerator CheckFallingBelowY(float thresholdY)
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                if (transform.position.y < thresholdY)
                {
                    Debug.Log("Player fell below threshold. Respawning...");
                    //transform.position = startingPosition;
                    GetComponent<Rigidbody>()?.MovePosition(startingPosition);
                }
                else
                {
                    Debug.Log("Player not below threshold.");
                }
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!enableDebugButton || !IsOwner) return;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 18;
            buttonStyle.normal.textColor = Color.white;

            if (GUI.Button(new Rect(20, 20, 220, 40), "Teleport to Z = 1000", buttonStyle))
            {
                var currentPos = transform.position;
                //transform.position = new Vector3(currentPos.x, currentPos.y, 1000f);
                GetComponent<Rigidbody>()?.MovePosition(startingPosition);

                if(GetComponent<Rigidbody>() == null)
                {
                    Debug.LogWarning("No rb");
                }
                Debug.Log("Teleported player to Z = 1000 for debugging.");
            }
        }
#endif

    }
}