using System;
using FishNet.Object;
using Items.Interfaces;
using UnityEngine;

namespace Networking
{
    [RequireComponent(typeof(HiderLookManager))]
    public class NetworkPlayerGrabController : NetworkBehaviour
    {
        [SerializeField] private int grabRange;
    
        public Transform grabPlacement; 
        private GameObject lookingAtObject = null; 
        private GameObject grabbedObject = null; 
        private Camera playerCamera;
        private HiderLookManager hiderLookManager;
    
        //this is variables for the placement mechanic
        public Material ghostMaterial;
        private Material originalMaterial; 
        private bool isBlueprintMode = false;
        private bool isGrabButtonHeld = false;
        private int originalLayer = 0;
        [SerializeField] private float placementOffset = 0.05f;
        
        [SerializeField] float minPlacementDistance = 2.0f;
    
        // Define a specific layer for temporarily placing the grabbed object
        private const int IGNORE_RAYCAST_LAYER = 2;
        [SerializeField] private LayerMask itemLayerMask;

        private void Start()
        {
            playerCamera = Camera.main;
            hiderLookManager = GetComponent<HiderLookManager>();
        }

        // Update is called once per frame
            void Update()
            {
                if (isBlueprintMode && isGrabButtonHeld && grabbedObject != null)
                {
                    //place mechanic
                    UpdateBlueprintMode(); 
                }
                else if (!isBlueprintMode && grabbedObject == null)
                {
                    //grab mechanic
                   UpdateLookingAtObject(); 
                }
            }

            void UpdateBlueprintMode()
            {
                GameObject objectToPlace = grabbedObject.gameObject;
                    
                // Move the grabbed object to the Ignore Raycast layer, so we don't
                int currentLayer = objectToPlace.layer;
                objectToPlace.layer = IGNORE_RAYCAST_LAYER;
                
                Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

                if (!playerCamera)
                {
                    playerCamera = Camera.main;
                }
                
                Ray screenCenterRay = playerCamera.ScreenPointToRay(screenCenter);
                
                //TODO: look into the grab range since we are in third person now @rea
                //TODO: change the raycast for a box cast 
                //Raycast check if the object is in front of player 
                if (Physics.Raycast(screenCenterRay, out RaycastHit hit, grabRange, ~LayerMask.GetMask("Player", "Ignore Raycast"), QueryTriggerInteraction.Ignore))
                {
                    Vector3 placementPosition = hit.point;
                    
                    /*TODO: if the hit collider game object is not a valid placement, we want to send a ray from the item down until it hits something,
                     if it does check if that's a valid placement, if not then put the box there with a red outline, 
                     if you release when youre at a red outline, the object goes back to your handsGet the collider's bounds for proper placement */
                    
                    Collider objCollider = objectToPlace.GetComponent<Collider>();
                    
                    if (objCollider != null)
                    {
                        float yOffset = objCollider.bounds.extents.y + placementOffset;
                        placementPosition = hit.point + hit.normal * yOffset;
                    }
                    
                    objectToPlace.transform.position = placementPosition;
                    objectToPlace.transform.up = hit.normal;
                    
                    // Update UI text
                    InScreenUI.Instance.SetToolTipText("Release to place object");
                }
                else
                {
                    // No valid surface found
                    Debug.Log("Raycast did not hit anything.");
                    objectToPlace.transform.position = playerCamera.transform.position + playerCamera.transform.forward * grabRange;
                    objectToPlace.transform.up = Vector3.up;
                    
                    //put the object on the floor 
                    RaycastHit floorHit;
                    if (Physics.Raycast(objectToPlace.transform.position, Vector3.down, out floorHit, 20f))
                    {
                        Renderer objectRenderer = objectToPlace.GetComponent<Renderer>();
                        float yOffset = 0f;
    
                        if (objectRenderer != null)
                        {
                            Bounds bounds = objectRenderer.bounds;
                            yOffset = bounds.extents.y;
                        }

                        objectToPlace.transform.position = floorHit.point + new Vector3(0, yOffset, 0);
                        objectToPlace.transform.up = floorHit.normal; // Align with floor normal
                    }
                }
                
                objectToPlace.layer = currentLayer;
            } 

            void UpdateLookingAtObject()
            {
                if (hiderLookManager.GetCurrentLookTarget()?.GetComponent<IHiderGrabableItem>() != null)
                {
                    lookingAtObject = hiderLookManager.GetCurrentLookTarget(); 
                }
                else
                {
                    lookingAtObject = null;
                }
            }
            

            // Called when grab button is pressed
            public void OnGrab()
            {
                isGrabButtonHeld = true;
                
                //grab mechanic
                if (grabbedObject == null)
                {
                   
                    PickupObject();
                    //inform server 
                    RPC_InformServerOnGrab();
                }
                //place mechanic
                else if (!isBlueprintMode)
                {
                   EnterBlueprintMode();
                  
                }
            }

            void PickupObject()
            {
                //nothing happens if youre not currently looking at something
                if (lookingAtObject == null) { return; }
                
                hiderLookManager.SetActive(false); 
                // Move to object to grab placement and parent with the player
                grabbedObject = lookingAtObject;
                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                if (rb != null) 
                {
                    rb.isKinematic = true;
                }

                grabbedObject.transform.position = grabPlacement.position;
                grabbedObject.transform.SetParent(this.transform);
            }

            void EnterBlueprintMode()
            {
                hiderLookManager.SetActive(true);
                grabbedObject.transform.parent = null; 
                // keep original data for the material + layer
                Renderer renderer = grabbedObject.GetComponent<Renderer>();
                if (renderer != null) 
                {
                    originalMaterial = renderer.material;
                    renderer.material = ghostMaterial; // Change to blueprint material
                }
                originalLayer = grabbedObject.layer;
                    
                // Set blueprint mode 
                isBlueprintMode = true;

                InScreenUI.Instance.SetToolTipText("Release to place object");
            }
            
            [ServerRpc]
            private void RPC_InformServerOnGrab()
            {
                Debug.Log("Received Grab Message from observer");
                PickupObject();
                BroadcastPickupToClients();
            }
            
            [ObserversRpc(ExcludeOwner = true)]
            void BroadcastPickupToClients(){
                PickupObject();
            }
            
            [ServerRpc]
            private void RPC_InformServerOnPlace(Vector3 position, Quaternion rotation)
            {
                Debug.Log("Received Grab Message from observer");
                PlaceObjectAt(position, rotation);
                BroadcastOnPlaceToClients(position, rotation);
            }
            
            [ObserversRpc(ExcludeOwner = true)]
            void BroadcastOnPlaceToClients(Vector3 position, Quaternion rotation){
                PlaceObjectAt(position, rotation);
            }

            // Called when grab button is released
            public void OnGrabRelease()
            {
                isGrabButtonHeld = false;
                //move the box if we are in blueprint mode 
                if (isBlueprintMode && grabbedObject != null)
                {

                    grabbedObject.transform.SetParent(null);

                    //Restore original layer
                    grabbedObject.layer = originalLayer;

                    //Restore physics
                    Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                    }

                    //Restore original material
                    Renderer renderer = grabbedObject.GetComponent<Renderer>();
                    if (renderer != null && originalMaterial != null)
                    {
                        renderer.material = originalMaterial;
                    }
                    //inform server 
                    RPC_InformServerOnPlace(grabbedObject.transform.position, grabbedObject.transform.rotation);
                    
                    //Reset variables
                    grabbedObject = null;
                    isBlueprintMode = false;

                    // Update UI
                    InScreenUI.Instance.SetToolTipText("");
                }
            }

            void PlaceObjectAt(Vector3 position, Quaternion rotation)
            {
                if(grabbedObject == null) {return;}

                //unparent the object 
                grabbedObject.transform.SetParent(null);
                //reset its rigidbody status
                //Restore physics
                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
                //update the position and rotation
                grabbedObject.transform.position = position;
                grabbedObject.transform.rotation = rotation;
                
                grabbedObject = null;
            }
    }
}