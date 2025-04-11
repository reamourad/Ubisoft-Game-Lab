using System;
using System.Collections.Generic;
using Items.Interfaces;
using Player;
using Player.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace Items
{
    public class COMP476Vacuum_NoNetwork : MonoBehaviour, IUsableItem, ISeekerAttachable
    {
        private struct VacuumCacheStruct
        {
            public readonly Rigidbody RigidBody;
            public readonly PlayerRole Role;

            public VacuumCacheStruct(Rigidbody rigidBody, PlayerRole role)
            {
                RigidBody = rigidBody;
                Role = role;
            }
        }

        [Header("Power Params")]
        [SerializeField] private float maxPower = 100f;
        [SerializeField] private float rechargeRatePerSec = 1.5f;
        [SerializeField] private float useRatePerSec = 2.5f;

        [Header("References and Other Params")]
        [SerializeField] private GameObject worldDummyRef;
        [SerializeField] private SphereCollider triggerVolume;
        [SerializeField] private Transform suctionPoint;
        [SerializeField] private float suctionCompleteDetectionRadius = 0.25f;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float vacuumSuctionRegular = 10;
        [SerializeField] private float vacuumSuctionPlayer = 10;
        [SerializeField] private ParticleSystem particles;

        private Dictionary<Collider, VacuumCacheStruct> vacuumCache = new();

        private bool localUsingFlag = false;
        private bool vacuumActive = false;

        private Rigidbody parentRigidbody;
        private bool suckedPlayer = false;

        private VacuumGui vacuumGui;

        private void Start()
        {
            triggerVolume.gameObject.SetActive(false);
            InitialiseVacuum();
        }

        private void OnDestroy()
        {
            if (vacuumGui != null)
                Destroy(vacuumGui.gameObject);
        }

        private void InitialiseVacuum()
        {
            parentRigidbody = GetComponentInParent<Rigidbody>();
            vacuumGui = Instantiate(Resources.Load<GameObject>("VacuumCanvas")).GetComponent<VacuumGui>();
            VacuumPowerManager.Instance.Initialise(maxPower, useRatePerSec, rechargeRatePerSec);
            VacuumPowerManager.Instance.OnPowerDepleted += () => UseItem(false);
        }

        public void UseItem(bool isUsing)
        {
            if (localUsingFlag == isUsing) return;
            if (isUsing && !VacuumPowerManager.Instance.HasPower)
                return;

            localUsingFlag = isUsing;
            SetVacuumState(isUsing);
            VacuumPowerManager.Instance.SetVacuumActiveStatus(isUsing);
        }

        private void SetVacuumState(bool use)
        {
            vacuumActive = use;
            vacuumCache.Clear();
            triggerVolume.gameObject.SetActive(use);

            if (use) particles.Play();
            else particles.Stop();
        }

        private void Update()
        {
            if (vacuumGui == null) return;

            vacuumGui.SetPowerPercentage(VacuumPowerManager.Instance.PowerPercentage);
        }

        private void FixedUpdate()
        {
            if (!vacuumActive) return;

            ApplySuction();
        }

        private void ApplySuction()
        {
            var colliders = Physics.OverlapSphere(triggerVolume.transform.position, triggerVolume.radius, layerMask);
            foreach (var col in colliders)
            {
                var vacuumCache = GetCacheStruct(col);
                if (vacuumCache.Equals(default)) continue;

                var rb = vacuumCache.RigidBody;
                if (rb == null || rb == parentRigidbody) continue;
                if (!InLineOfSight(rb)) continue;

                var force = vacuumCache.Role == null ? vacuumSuctionRegular : vacuumSuctionPlayer;
                var rbPos = rb.centerOfMass + rb.position;
                var dir = (suctionPoint.position - rbPos).normalized;
                rb.AddForce(dir * force, ForceMode.Force);

                var dist = Vector3.Distance(rbPos, suctionPoint.position);
                if (dist <= suctionCompleteDetectionRadius)
                    Capture(rb);
            }
        }

        private void Capture(Rigidbody rb)
        {
            var playerRole = rb.GetComponent<PlayerRole>();

            if (playerRole != null)
            {
                if (playerRole.Role != PlayerRole.RoleType.Hider) return;
                if (!suckedPlayer)
                {
                    Debug.Log("Hider captured");
                    
                    suckedPlayer = true;
                }
                return;
            }

            var item = rb.GetComponent<IVacuumDestroyable>();
            item?.OnVacuumed();

            if(FindFirstObjectByType<UIManager>() != null) FindFirstObjectByType<UIManager>().Score += 10;
            Destroy(rb.gameObject);
        }

        private bool InLineOfSight(Rigidbody toCheck)
        {
            var direction = ((toCheck.position + toCheck.centerOfMass) - suctionPoint.position).normalized;
            var ray = new Ray(suctionPoint.position, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, 50.0f, layerMask, QueryTriggerInteraction.Ignore))
            {
                var cache = GetCacheStruct(hit.collider);
                return toCheck == cache.RigidBody;
            }
            return false;
        }

        private VacuumCacheStruct GetCacheStruct(Collider collider)
        {
            if (vacuumCache.TryGetValue(collider, out var cacheStruct))
                return cacheStruct;

            var rb = collider.GetComponentInParent<Rigidbody>();
            var role = collider.GetComponentInParent<PlayerRole>();
            var cacheEntry = new VacuumCacheStruct(rb, role);
            vacuumCache.Add(collider, cacheEntry);
            return cacheEntry;
        }

        private void OnDrawGizmos()
        {
            if (!suctionPoint) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(suctionPoint.position, suctionCompleteDetectionRadius);
            if (triggerVolume.gameObject.activeInHierarchy)
            {
                Gizmos.color = new Color(0f, 0f, 1f, 0.25f);
                Gizmos.DrawSphere(triggerVolume.center + triggerVolume.transform.position, triggerVolume.radius);
            }
        }

        public void OnAttach(Transform parentTrf)
        {
            transform.SetParent(parentTrf);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            InitialiseVacuum();
        }

        public void OnDetach(Transform parentTrf, bool spawnWorldDummy)
        {
            if (spawnWorldDummy && worldDummyRef != null)
            {
                var dummy = Instantiate(worldDummyRef, parentTrf.position + Vector3.up + parentTrf.forward * 2f, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}