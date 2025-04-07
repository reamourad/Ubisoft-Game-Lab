using System;
using System.Collections;
using StateManagement;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Player.WorldMarker
{
    public class WorldMarker : MonoBehaviour
    {
        [SerializeField] private Image markerIcon;
        [SerializeField] private TMPro.TMP_Text distText;
        
        Transform target;
        RectTransform rectTransform;

        private bool ready = false;
        private string myId;
        private CanvasGroup canvasGroup;
        
        private Coroutine coroutine;
        
        public void Set(string id, Transform target, Sprite sprite, bool animate)
        {
            myId = id;
            canvasGroup = transform.AddComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
            markerIcon.sprite = sprite;
            this.target = target;
            
            ready = !animate;
            if(animate)
                StartCoroutine(Prepare());
            
            coroutine = StartCoroutine(KillRoutine());
        }

        private IEnumerator KillRoutine()
        {
            yield return new WaitForSeconds(5f);
            float timeStep = 0;
            while (timeStep <= 1)
            {
                timeStep += Time.deltaTime/0.25f;
                canvasGroup.alpha = Mathf.Lerp(1f,0f, timeStep);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            coroutine = null;
            WorldMarkerManager.Instance.DestroyMarker(myId);
        }

        private void OnDestroy()
        {
            if(coroutine != null)
                StopCoroutine(coroutine);
        }

        IEnumerator Prepare()
        {
            distText.text = ComputeDistanceFromLocal().ToString();
            rectTransform.anchoredPosition = Vector2.zero;
            yield return new WaitForSeconds(0.5f);
            var activeCamera = Camera.main;
            var uiPos = GetScreenPosition(activeCamera);
            float timeStep = 0;
            while (timeStep <= 1)
            {
                uiPos = GetScreenPosition(activeCamera);
                timeStep += Time.deltaTime / 1f;
                rectTransform.position = Vector2.Lerp(rectTransform.position, uiPos, timeStep);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            ready = true;
        }
        

        public void UpdatePositions()
        {
            if(target == null)
                return;
            
            if(!ready)
                return;
            
            var activeCam = Camera.main;
            if (activeCam == null || !activeCam.enabled)
            {
                markerIcon.enabled = false;
                return;
            }
            
            rectTransform.position = GetScreenPosition(activeCam);
            distText.text = $"{ComputeDistanceFromLocal()}m";
            markerIcon.enabled = true;
        }

        private int ComputeDistanceFromLocal()
        {
            var localPlayer = GameLookupMemory.LocalPlayer.transform.position;
            return (int)Vector3.Distance(localPlayer, target.position);
        }
        
        private Vector3 GetScreenPosition(Camera activeCam)
        {
            var screenPosition = activeCam.WorldToScreenPoint(target.position);
            // Flip direction if behind camera (so we can clamp it properly)
            if (screenPosition.z < 0)
            {
                screenPosition *= -1;
            }

            // Clamp the position to stay inside screen bounds, leaving a little padding
            float padding = 50f; // You can tweak this
            screenPosition.x = Mathf.Clamp(screenPosition.x, padding, Screen.width - padding);
            screenPosition.y = Mathf.Clamp(screenPosition.y, padding, Screen.height - padding);
            return screenPosition;
        }
    }
}