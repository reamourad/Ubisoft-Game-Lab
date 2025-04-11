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
        [SerializeField]
        private FPSAimDetector aimDetector;
        [SerializeField]
        private SeekerInventory seekerInventory;
        private SeekerWorldDummy ctxDummy;
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                InputReader.Instance.OnGrabActivateEvent += OnPickup;
                InputReader.Instance.OnDropItemEvent += OnDrop;
                
                aimDetector.OnLookingAtObject += OnLookingAtObject;
                aimDetector.Initialise(DetectionTest);
            }
        }

        private bool DetectionTest(Collider other)
        {
            if (!other)
                return false;
            return other.CompareTag("WorldDummyItem");
        }
        
        private void OnDestroy()
        {
            if(aimDetector != null)
                aimDetector.OnLookingAtObject -= OnLookingAtObject;
            InputReader.Instance.OnGrabActivateEvent -= OnPickup;
            InputReader.Instance.OnDropItemEvent -= OnDrop;
        }

        private void OnLookingAtObject(Collider obj)
        {
            if (!obj)
            {
                if (ctxDummy != null)
                    ctxDummy.Highlight(false);
                ctxDummy = null;
                InScreenUI.Instance?.RemoveInputPrompt(InputReader.Instance.inputMap.Gameplay.Grab);
                return;
            }
            ctxDummy = obj.GetComponentInParent<SeekerWorldDummy>();
            if (ctxDummy != null)
            {
                ctxDummy.Highlight(true);
                InScreenUI.Instance.ShowInputPrompt(InputReader.Instance.inputMap.Gameplay.Grab,"Pick up");
            }
        }
        
        private void OnPickup()
        {
            Debug.Log("SeekerPickupSystem::::OnPickup");
            if(ctxDummy == null) return;
            if(!seekerInventory.HasStorage) return;
            seekerInventory.AddToInventory(ctxDummy.ItemReference, ctxDummy.Icon);
            ctxDummy.OnPickedUp();
        }
        
        private void OnDrop()
        {
            Debug.Log("SeekerPickupSystem::::OnDrop");
            if(seekerInventory == null) return;
            seekerInventory.RemoveActiveItem();
        }
    }
}