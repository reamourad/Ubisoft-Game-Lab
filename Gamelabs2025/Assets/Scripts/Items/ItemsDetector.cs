using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Player.Audio;
using Player.WorldMarker;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Items
{
    public class ItemsDetector : NetworkBehaviour
    {
        [SerializeField] private float coolDownTime = 2f;
        
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private Transform originPoint;
        [SerializeField] private float range;
        [SerializeField] private MeshRenderer detectionGraphic;
        
        [SerializeField] private Sprite triggerIcon;
        [SerializeField] private Sprite reactionIcon;
        [SerializeField] private Sprite stationaryIcon;
        
        [SerializeField] private AudioClip scanSFX;
        [SerializeField] private AudioClip hiderScanSFX;
        
        [SerializeField] private AnimationCurve curve;
        private Coroutine routine;
        
        private List<string> createdItems;
        private int maxMarkers = 5;
        Collider[] colliders;

        private float nextAllowDetectionTime = 0f;
        

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                createdItems = new List<string>();
                InputReader.Instance.OnHiderItemScanEvent += Detect;
            }
        }

        private void OnDestroy()
        {
            InputReader.Instance.OnHiderItemScanEvent -= Detect;
        }

        public void Detect()
        {
            if(Time.time < nextAllowDetectionTime)
                return;
            
            nextAllowDetectionTime = Time.time + coolDownTime;
            
            if(routine != null)
                StopCoroutine(routine);
            
            
            //clone the reference
            var detectionObjClone = Instantiate(detectionGraphic.gameObject, originPoint.position, Quaternion.identity, transform);
            detectionObjClone.SetActive(true);
            var mesh = detectionObjClone.GetComponent<MeshRenderer>();
            
            colliders = Physics.OverlapSphere(originPoint.position, range, layerMask);
            routine = StartCoroutine(DetectionThingy(mesh));
        }

        IEnumerator DetectionThingy(MeshRenderer mesh)
        {
            AudioManager.Instance.PlaySFX(scanSFX);
            // Use sharedMaterial if you don't want to create a new instance
            // Otherwise, mesh.material is fine if you want a unique instance per call
            Material material = mesh.material; 
            Color baseColor = material.color;
            var startAlpha = material.color.a;

            mesh.transform.localScale = Vector3.zero;

            Vector3 maxScale = Vector3.one * range;
            float timeElapsed = 0f;
            float duration = 1f;

            if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
            {
                Gamepad.current.SetMotorSpeeds(0.15f, 0.25f);
            }
            
            while (timeElapsed <= duration)
            {
                float t = timeElapsed / duration;
                float evaluated = curve.Evaluate(t);

                mesh.transform.localScale = Vector3.Lerp(Vector3.zero, maxScale, evaluated);
                Color currentColor = baseColor;
                currentColor.a = Mathf.Lerp(startAlpha, 0f, evaluated);
                material.color = currentColor;

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
            {
                Gamepad.current.ResetHaptics();
            }
            
            // Final update after loop
            mesh.transform.localScale = maxScale;
            material.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);

            yield return StartCoroutine(HighlightItems(range * curve.Evaluate(1f)));
            Destroy(mesh.gameObject);
        }

        IEnumerator HighlightItems(float currentRange)
        {
            bool detected = false;
            var items = new List<Transform>();
            foreach (var collider in colliders)
            {
                if(Vector3.Distance(collider.transform.position, originPoint.position) > currentRange)
                    continue;
                
                items.Add(collider.transform);

                foreach (var id in createdItems)
                {
                    WorldMarkerManager.Instance.DestroyMarker(id);
                }
                createdItems.Clear();
            }
            
            //sort with closest to player
            items.Sort((x, y) => Vector3.Distance(x.transform.position, originPoint.position)
                                               .CompareTo(Vector3.Distance(y.transform.position, originPoint.position)) );
            //remove all other else
            if(items.Count > maxMarkers)
                items.RemoveRange(maxMarkers, items.Count - maxMarkers);

            foreach (var collider in items)
            {
                var reaction = collider.GetComponentInParent<IReactionItem>();
                var trigger = collider.GetComponentInParent<ITriggerItem>();
                var stationary = collider.GetComponentInParent<StationaryObjectBase>();

                if (reaction != null)
                {
                    createdItems.Add(WorldMarkerManager.Instance.AddWorldMarker(collider.transform, reactionIcon, true));
                }
                else if (trigger != null)
                {
                    createdItems.Add(WorldMarkerManager.Instance.AddWorldMarker(collider.transform, triggerIcon, true));
                }
                else if (stationary != null)
                {
                    createdItems.Add(WorldMarkerManager.Instance.AddWorldMarker(collider.transform, stationaryIcon, true));
                }

                if (collider != null)
                {
                    if(collider.gameObject.TryGetComponent<DetectableObject>(out var detectableObject))
                        detectableObject.OnDetect();
                
                    if(collider.transform.parent.TryGetComponent<DetectableObject>(out var parentObject))
                        parentObject.OnDetect();
                }
                
                
                yield return new WaitForSeconds(0.1f);
                AudioManager.Instance.PlaySFX(hiderScanSFX);
            }
        }
        
    }
}