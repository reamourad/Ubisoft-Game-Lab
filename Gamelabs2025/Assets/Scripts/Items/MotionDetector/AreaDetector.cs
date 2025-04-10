using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GogoGaga.OptimizedRopesAndCables;
using StateManagement;
using UnityEngine;

namespace Player.Items.MotionDetector
{
    public class AreaDetector : DetectableObject, ITriggerItem, IHiderGrabableItem
    {
        [SerializeField] private Transform rayCastPoint;
        [SerializeField] private float activationDelay=0.25f;
        [SerializeField] private AudioClip triggeredClip;
        
        [SerializeField] private GameObject rangeDetector;
        [SerializeField] private AudioSource localSFXSource;
        [SerializeField] private MeshRenderer meshRenderer;
        public Rope rope { get; set; }
        public event Action<ITriggerItem> OnTriggerActivated;
        
        private Coroutine activationRoutine;
        private Coroutine localActivationRoutine;

        public override void OnStartClient()
        {
            base.OnStartClient();
            StartCoroutine(Initialize());
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            rangeDetector.gameObject.SetActive(true);
            rangeDetector.GetComponent<MeshRenderer>().enabled = false;
        }

        private void Update()
        {
            if (IsClientStarted 
                && GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.Hider)
            {
                rangeDetector.gameObject.SetActive(true);
            }
        }

        IEnumerator Initialize()
        {
            yield return new WaitWhile(() => GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.None);
            if(GameLookupMemory.MyLocalPlayerRole != PlayerRole.RoleType.Hider)
                yield break;
            
            rangeDetector.gameObject.SetActive(true);
            Destroy(rangeDetector.GetComponentInChildren<Rigidbody>());
        }
        
        
        [Server]
        private void OnTriggerEnter(Collider other)
        {
            if(activationRoutine != null)
                return;
            
            var role = other.GetComponentInParent<PlayerRole>();
            if(role == null)
                return;
            if (role.Role == PlayerRole.RoleType.Seeker)
            {
                var direction = (other.transform.position - rayCastPoint.position).normalized;
                var dist = Vector3.Distance(rayCastPoint.position, other.transform.position);
                //somehow the player is below you... ignore
                if (Physics.Raycast(rayCastPoint.position, direction, out RaycastHit hit))
                {
                    //Check for line of sight
                    if (hit.collider != other)
                    {
                        return;
                    }
                }
                
                activationRoutine = StartCoroutine(DelayedActivation());
                RPC_OnPlayerEntered();
            }

        }

        [ObserversRpc(ExcludeOwner = false)]
        private void RPC_OnPlayerEntered()
        {
            if(localActivationRoutine != null)
                StopCoroutine(localActivationRoutine);
            
            localActivationRoutine =  StartCoroutine(ClientActivationRoutine());
        }

        IEnumerator ClientActivationRoutine()
        {
            var end = Time.time + 3;
            while (Time.time < end)
            {
                localSFXSource.PlayOneShot(triggeredClip);
                meshRenderer.material.EnableKeyword("_EMISSION");
                yield return new WaitForSeconds(0.125f);
                meshRenderer.material.DisableKeyword("_EMISSION");
                yield return new WaitForSeconds(0.125f);
            }
            meshRenderer.material.EnableKeyword("_EMISSION");
        }
        
        [Server]
        private void OnTriggerExit(Collider other)
        {
            var role = other.GetComponentInParent<PlayerRole>();
            if(role == null)
                return;
            
            if (role.Role == PlayerRole.RoleType.Seeker)
            {
                if(activationRoutine != null)
                    StopCoroutine(activationRoutine);
                
                activationRoutine = null;
            }
        }

        IEnumerator DelayedActivation()
        { 
            yield return new WaitForSeconds(activationDelay);
            OnTriggerActivated?.Invoke(this);
            yield return new WaitForSeconds(1);
            activationRoutine = null;
        }
    }
}