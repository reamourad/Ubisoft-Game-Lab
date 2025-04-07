using System;
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

        private void Awake()
        {
            highlightLayer = LayerMask.NameToLayer("Highlight");
        }

        public void OnDetect()
        {
            if (body == null) return;
            
            var children = body.GetComponentsInChildren<MeshRenderer>().Select(t => t.gameObject).ToList();
            var previousLayer = body.layer;
            body.layer = highlightLayer;
            children.ForEach(c => c.layer = highlightLayer);
            DOVirtual.DelayedCall(5, () =>
            {
                body.layer = previousLayer;
                children.ForEach(child => child.layer = previousLayer);
            });
        }
    }
}