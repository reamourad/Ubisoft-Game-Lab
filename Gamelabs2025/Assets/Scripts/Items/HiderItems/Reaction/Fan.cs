using DG.Tweening;
using FishNet.Object;
using GogoGaga.OptimizedRopesAndCables;
using Player.Data;
using Player.Items;
using UnityEngine;

namespace Items.HiderItems.Reaction
{
    public class Fan : NetworkBehaviour, IReactionItem, IHiderGrabableItem
    {
        [SerializeField] private StationaryEffect effects;
        [SerializeField] private Transform spinTransform;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float rotationSpeed = 5;
        [SerializeField] private GameObject smokeFx;
        [SerializeField] private GameObject windFx;
        [SerializeField] private AudioClip clip;
        [SerializeField] private float radius;
        [SerializeField] private float range;

        private bool isTriggered;
        private bool isSpinning;
        public Rope rope { get; set; }

        public void OnTrigger(ITriggerItem triggerItem)
        {
            OnServerBlow();
        }

        [Server]
        private void OnServerBlow()
        {
            //if (isTriggered) return;
            RPC_OnClientBlow();
            ApplyWindEffect();
            isTriggered = true;
        }

        [Server]
        private void ApplyWindEffect()
        {
            var colliders = Physics.OverlapCapsule(transform.position, transform.position + transform.up * range,
                radius);
            foreach (var collider in colliders)
            {
                if (ValidCollider(collider, out var stationaryObject))
                {
                    DOVirtual.DelayedCall(1, () => { stationaryObject.ApplyStationaryEffect(effects); });
                }
            }
        }

        [ObserversRpc]
        private void RPC_OnClientBlow()
        {
            var effect = Instantiate(windFx, transform.position, Quaternion.identity);
            effect.transform.SetParent(transform);
            effect.transform.forward = Vector3.up;
            isSpinning = true;
            PlaySound();
            Destroy(effect, 3);
            DOVirtual.DelayedCall(3.5f, () =>
            {
                isSpinning = false;
                /*var smoke = Instantiate(smokeFx, transform.position, Quaternion.identity);
                smoke.transform.SetParent(transform);
                smoke.transform.forward = Vector3.up;
                Destroy(smoke, 3);*/
            });
        }

        private void PlaySound()
        {
            audioSource.PlayOneShot(clip);
        }

        private void Update()
        {
            if (!isSpinning) return;
            SpinFan();
        }

        private void SpinFan()
        {
            spinTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        private bool ValidCollider(Collider collider, out StationaryObjectBase stationaryObject)
        {
            stationaryObject = null;
            stationaryObject = collider.GetComponentInParent<StationaryObjectBase>();
            return stationaryObject != null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            DrawWireCapsule(transform.position, transform.position + transform.up * range);
        }

        private void DrawWireCapsule(Vector3 p1, Vector3 p2)
        {
            Gizmos.DrawWireSphere(p1, radius);
            Gizmos.DrawWireSphere(p2, radius);

            var up = Vector3.up * radius;
            var forward = Vector3.forward * radius;
            var right = Vector3.right * radius;

            Gizmos.DrawLine(p1 + up, p2 + up);
            Gizmos.DrawLine(p1 - up, p2 - up);
            Gizmos.DrawLine(p1 + forward, p2 + forward);
            Gizmos.DrawLine(p1 - forward, p2 - forward);
            Gizmos.DrawLine(p1 + right, p2 + right);
            Gizmos.DrawLine(p1 - right, p2 - right);
        }
    }
}