using System;
using System.Collections;
using UnityEngine;

namespace Utils
{
    public class FPSAimerGui : MonoBehaviour
    {
        [SerializeField] private Transform recticle;
        [SerializeField] private float sizeMultiplier = 1.5f;
        [SerializeField] private float defaultRecticleAlpha = 0.5f;
        
        private Vector2 originalSize;
        private bool enlarged = false;
        private Coroutine highlightRoutine;

        private RectTransform rt;
        private CanvasGroup cg;
        
        private void Start()
        {
            originalSize = recticle.GetComponent<RectTransform>().sizeDelta;
            cg = recticle.GetComponent<CanvasGroup>();
            rt = recticle.GetComponent<RectTransform>();
        }

        public void Shrink()
        {
            if(!enlarged)
                return;
            
            if(highlightRoutine != null)
                StopCoroutine(highlightRoutine);
            highlightRoutine = StartCoroutine(HighlightRoutine(false));
            enlarged = false;
        }

        public void Enlarge()
        {
            if(enlarged)
                return;
            
            if(highlightRoutine != null)
                StopCoroutine(highlightRoutine);
            highlightRoutine = StartCoroutine(HighlightRoutine(true));
            enlarged = true;
        }
        
        IEnumerator HighlightRoutine(bool enlarge)
        {
            float timeStep = 0;
            var currSize = rt.sizeDelta;
            var newSize = enlarge? sizeMultiplier * originalSize:originalSize;
            var alpha = enlarge ? 1 : defaultRecticleAlpha;
            var currAlpha = cg.alpha;
            while (timeStep <= 1)
            {
                timeStep += Time.deltaTime/0.15f;
                rt.sizeDelta = Vector2.Lerp(currSize, newSize, timeStep);
                cg.alpha = Mathf.Lerp(currAlpha, alpha, timeStep);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}