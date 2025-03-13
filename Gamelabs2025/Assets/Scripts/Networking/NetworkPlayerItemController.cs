using System;
using FishNet.Object;
using UnityEngine;

namespace Networking
{
    public class NetworkPlayerItemController : NetworkBehaviour
    {
        [SerializeField] private int grabRange;
    
        public Transform grabPlacement; 
        private IGrabableItem lookingAtObject = null; 
        private IGrabableItem grabbedObject = null; 
        private Camera playerCamera;
    
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
                else
                {
                    InScreenUI.Instance.SetToolTipText("");
                }
            }

            void UpdateBlueprintMode()
            {
                
                GameObject objectToPlace = grabbedObject.gameObject;
                    
                // Move the grabbed object to the Ignore Raycast layer, so we don't
                int currentLayer = objectToPlace.layer;
                objectToPlace.layer = IGNORE_RAYCAST_LAYER;
                
                Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                
                Ray screenCenterRay = playerCamera.ScreenPointToRay(screenCenter);
                
                //TODO: look into the grab range since we are in third person now @rea
                //TODO: change the raycast for a box cast 
                //Raycast so object follows crosshair 
                if (Physics.Raycast(screenCenterRay, out RaycastHit hit, grabRange, ~LayerMask.GetMask("Player", "Ignore Raycast"), QueryTriggerInteraction.Ignore))
                {
                    Debug.Log(hit.collider.gameObject);
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

            private Vector3 point; 
            void UpdateLookingAtObject()
            {
                if(!IsOwner){return;}
                
                lookingAtObject = null;
                
                if(playerCamera == null)
                    playerCamera = Camera.main;
                
                // Origin at the center of your character
                Vector3 origin = transform.position;
    
                // Direction your character is facing
                Vector3 direction = transform.forward;
    
                // Box dimensions
                Vector3 halfExtents = new Vector3(0.5f, 1.0f, 0.1f); // width, height, depth
    
                // Character's rotation
                Quaternion orientation = transform.rotation;
    
                // Maximum distance to check
                float maxDistance = 3.0f;
    
                // Layer mask for objects to check
                LayerMask layerMask = itemLayerMask;
                
                // Raycast from the center of player view
                Ray ray = new Ray(transform.position, transform.forward);
                if (Physics.BoxCast(origin, halfExtents, direction, out RaycastHit hit, orientation, maxDistance, layerMask))
                {
                    point = hit.point;
                    Debug.DrawLine(transform.position, hit.point, Color.red);
                    lookingAtObject = hit.collider.GetComponent<IGrabableItem>();
                }
                Visualization.DrawBoxCastBox(origin, halfExtents, direction, orientation, maxDistance, Color.green);
                
                // Check if the object has the IGrabbable interface
                if (lookingAtObject != null)
                {
                    if (InScreenUI.Instance != null)
                    {
                        InScreenUI.Instance.SetToolTipText("Press " +
                                                           InputReader.GetCurrentBindingText(InputReader.Instance.inputMap.Gameplay
                                                               .Grab) + " to grab  " + lookingAtObject.gameObject.name);
                    }
                    
                }
                else
                {
                    if (InScreenUI.Instance != null)
                    {
                        InScreenUI.Instance.SetToolTipText("");
                    }
                    
                }
            }
            

            // Called when grab button is pressed
            public void OnGrab()
            {
                isGrabButtonHeld = true;
                
                //grab mechanic
                if (grabbedObject == null)
                {
                    //nothing happens if youre not currently looking at something
                    if (lookingAtObject == null) { return; }
                    
                                
                    // Move to object to grab placement and parent with the player
                    grabbedObject = lookingAtObject;
                    GameObject objToGrab = grabbedObject.gameObject;
                    Rigidbody rb = objToGrab.GetComponent<Rigidbody>();
                    if (rb != null) 
                    {
                        rb.isKinematic = true;
                    }

                    objToGrab.transform.position = grabPlacement.position;
                    objToGrab.transform.SetParent(this.transform);
                    RPC_SendGrabMessageToOtherClients(playerCamera.transform.position, playerCamera.transform.forward);
                    
                }
                //place mechanic
                else if (!isBlueprintMode)
                {
                    GameObject objectToPlace = grabbedObject.gameObject;
                    objectToPlace.transform.parent = null; 
                    // keep original data for the material + layer
                    Renderer renderer = objectToPlace.GetComponent<Renderer>();
                    if (renderer != null) 
                    {
                        originalMaterial = renderer.material;
                        renderer.material = ghostMaterial; // Change to blueprint material
                    }
                    originalLayer = objectToPlace.layer;
                    
                    // Set blueprint mode 
                    isBlueprintMode = true;

                    InScreenUI.Instance.SetToolTipText("Release to place object");
                }
            }
            
            [ObserversRpc]
            private void RPC_SendGrabMessageToOtherClients(Vector3 position, Vector3 forward)
            {
                if (Physics.Raycast(position, forward, out RaycastHit hit, grabRange, itemLayerMask))
                {
                    lookingAtObject = hit.collider.GetComponent<IGrabableItem>();
                }
                OnGrab();
            }

            // Called when grab button is released
            public void OnGrabRelease()
            {
                isGrabButtonHeld = false;

                if (isBlueprintMode && grabbedObject != null)
                {
                    GameObject objectToPlace = grabbedObject.gameObject;

                    objectToPlace.transform.SetParent(null);

                    //Restore original layer
                    objectToPlace.layer = originalLayer;

                    //Restore physics
                    Rigidbody rb = objectToPlace.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                    }

                    //Restore original material
                    Renderer renderer = objectToPlace.GetComponent<Renderer>();
                    if (renderer != null && originalMaterial != null)
                    {
                        renderer.material = originalMaterial;
                    }

                    //Reset variables
                    grabbedObject = null;
                    isBlueprintMode = false;

                    // Update UI

                    InScreenUI.Instance.SetToolTipText("");
                }
            }
    }
}