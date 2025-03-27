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
        
        public void Initialise()
        {
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
            if(cam == null)
                return;
            
            var ray = cam.ScreenPointToRay(screenCenter);
            
            if (Physics.Raycast(ray, out RaycastHit hit, maxTraceDistance,mask))
            {
                Debug.DrawRay(ray.origin, hit.point, Color.cyan);
                gui?.Enlarge();
                OnLookingAtObject?.Invoke(hit.collider);
            }
            else
            {
                OnLookingAtObject?.Invoke(null);
                Debug.DrawRay(ray.origin, ray.direction * maxTraceDistance, Color.yellow);
                gui?.Shrink();
            }
        }
    }
}