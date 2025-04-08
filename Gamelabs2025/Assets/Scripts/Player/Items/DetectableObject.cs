using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using FishNet.Object;
using UnityEngine;

namespace Player.Items
{
    public abstract class DetectableObject: NetworkBehaviour
    {
        [SerializeField] private GameObject body;

        private LayerMask highlightLayer;
        private LayerMask ogLayer;
        private MeshRenderer[] meshRenderers;

        private Coroutine coroutine;
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            ogLayer = body.layer;
            highlightLayer = LayerMask.NameToLayer("Highlight");
            meshRenderers = body.GetComponentsInChildren<MeshRenderer>();
        }
        
        public void OnDetect()
        {
            if (!body) return;
            body.layer = highlightLayer;
            
            if(coroutine != null)
                StopCoroutine(coroutine);
            coroutine = StartCoroutine(AnimateDetection());
        }

        IEnumerator AnimateDetection()
        {
            yield return new WaitForEndOfFrame();
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.gameObject.layer = highlightLayer;
            }
            yield return new WaitForSeconds(5);
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.gameObject.layer = ogLayer;
            }
        }
    }
}