using System;
using System.Collections;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;

namespace Utils
{
    public class FPSAimDetector : NetworkBehaviour
    {
        [SerializeField] LayerMask mask;
        [SerializeField] private float maxTraceDistance = 3;
        [SerializeField] private Camera cam;
        private Vector2 screenCenter;
        
        public Action<Collider> OnLookingAtObject;
        private FPSAimerGui gui;

        private Func<Collider, bool> DetectionTest = null;
        
        public void Initialise(Func<Collider, bool> detectionTest)
        {
            DetectionTest = detectionTest;
            StartCoroutine(ClientInitialiser());
        }

        
        IEnumerator ClientInitialiser()
        {
            yield return new WaitUntil(() => Camera.main != null);
            Debug.Log("FPSAimDetector:::Initialising");
            screenCenter = new Vector3((float)Screen.width / 2, (float)Screen.height / 2, 0);
            if(cam == null)
                cam = Camera.main;
            
            gui = Instantiate(Resources.Load("FPSAimerCanvas")).GetComponent<FPSAimerGui>();
        }
        
        private void FixedUpdate()
        {
            if(!cam)
                return;
            
            var ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxTraceDistance,mask))
            {
                if (DetectionTest == null || !DetectionTest(hit.collider))
                {
                    RayTestFail(ray);
                    return;
                }

                Debug.DrawRay(ray.origin, hit.point, Color.cyan);
                gui?.Enlarge();
                OnLookingAtObject?.Invoke(hit.collider);
            }
            else
            {
                RayTestFail(ray);
            }
        }

        private void RayTestFail(Ray ray)
        {
            OnLookingAtObject?.Invoke(null);
            Debug.DrawRay(ray.origin, ray.direction * maxTraceDistance, Color.yellow);
            gui?.Shrink();
        }
    }
}