using System;
using FishNet.Object;
using Player.Inventory;
using UnityEngine;
using Utils;

namespace SeekerGrabSystem
{
    [RequireComponent(typeof(FPSAimDetector))]
    [RequireComponent(typeof(SeekerInventory))]
    public class SeekerPickupDropSystem : NetworkBehaviour
    {
        private FPSAimDetector aimDetector;
        private SeekerInventory seekerInventory;
        private SeekerWorldDummy ctxDummy;
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                seekerInventory = GetComponent<SeekerInventory>();
                aimDetector = GetComponent<FPSAimDetector>();
                
                InputReader.Instance.OnGrabActivateEvent += OnPickup;
                InputReader.Instance.OnGrabActivateEvent += OnDrop;
                
                aimDetector.OnLookingAtObject += OnLookingAtObject;
                aimDetector.Initialise();
            }
        }
        
        private void OnDestroy()
        {
            if(aimDetector != null)
                aimDetector.OnLookingAtObject -= OnLookingAtObject;
            InputReader.Instance.OnGrabActivateEvent -= OnPickup;
        }

        private void OnLookingAtObject(Collider obj)
        {
            if (obj == null)
            {
                if (ctxDummy != null)
                    ctxDummy.Highlight(false);
                ctxDummy = null;
                return;
            }
            ctxDummy = obj.GetComponentInParent<SeekerWorldDummy>();
            if(ctxDummy != null)
                ctxDummy.Highlight(true);
        }
        
        private void OnPickup()
        {
            Debug.Log("SeekerPickupSystem::::OnPickup");
            if(ctxDummy == null) return;
            seekerInventory.AddToInventory(ctxDummy.ItemReference, ctxDummy.Icon);
            ctxDummy.OnPickedUp();
        }
        
        private void OnDrop()
        {
            Debug.Log("SeekerPickupSystem::::OnPickup");
            if(seekerInventory == null) return;
            seekerInventory.RemoveActiveItem();
        }
    }
}