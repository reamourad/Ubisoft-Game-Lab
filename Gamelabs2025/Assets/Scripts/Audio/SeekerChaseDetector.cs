using System.Collections;
using System.Linq;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using StateManagement;
using UnityEngine;

namespace Player.Audio
{
    public class SeekerChaseDetector : NetworkBehaviour
    {
       
        
        private bool previous = false;
        private bool chaseStarted = false;
        private GameObject hider;
        private PlayerRole[] roles;
        
        [SerializeField] LayerMask layerMask;
        [SerializeField] private Transform eye;
        [SerializeField] private float threshDot = 0.75f;
        [SerializeField] private float threshDist = 3f;
        [SerializeField] private float chaseEndPeriod = 3;

        private float cooldownSeconds;
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            StartCoroutine(CheckRoutine());
        }

        IEnumerator CheckRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                bool current = InChase();
                if (current == previous)
                {
                    if (current)
                    {
                        cooldownSeconds = chaseEndPeriod;
                        if (!chaseStarted)
                        {
                            GameController.Instance.ServerInitiateChase();
                            chaseStarted = true;
                        }
                    }
                    else
                    {
                        if(chaseStarted) cooldownSeconds -= 1;
                        if (cooldownSeconds <= 0 && chaseStarted)
                        {
                            cooldownSeconds = 0;
                            chaseStarted = false;
                            GameController.Instance.ServerStopChase();
                        }
                    }
                }
                previous = current;
            }
        }
        
        bool InChase()
        {
            if (roles == null)
                roles = FindObjectsByType<PlayerRole>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            if (hider == null)
                hider = roles.SingleOrDefault(a => a.Role == PlayerRole.RoleType.Hider)?.gameObject;
            
            var dir = (hider.transform.position - transform.position).normalized;
            var dot = Vector3.Dot(dir, transform.forward);

            if (dot >= threshDot)
            {
                if (Vector3.Distance(hider.transform.position, transform.position) > threshDist)
                {
                    return false;
                }

                if (Physics.Raycast(eye.position, dir, out RaycastHit hit, threshDist, layerMask))
                {
                    Debug.DrawRay(eye.position, dir * threshDist, Color.yellow);
                    Debug.Log($"HIT: {hit.collider.gameObject.name}");
                    var playerRole = hit.collider.GetComponentInParent<PlayerRole>();
                    return playerRole && playerRole.Role == PlayerRole.RoleType.Hider;
                }
            }
            
            return false;
        }
    }
}