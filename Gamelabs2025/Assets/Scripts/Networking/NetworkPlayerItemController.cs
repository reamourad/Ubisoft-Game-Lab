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
    
        // Define a specific layer for temporarily placing the grabbed object
        private const int IGNORE_RAYCAST_LAYER = 2;

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
                else if (!isBlueprintMode)
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
                    
                //Raycast so object follows crosshair 
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, grabRange))
                {
                    // Get the collider's bounds for proper placement
                    Collider objCollider = objectToPlace.GetComponent<Collider>();
                    Vector3 placementPosition = hit.point;
                    
                    if (objCollider != null)
                    {
                        // Calculate how much to offset from the hit point based on object's bounds
                        // This prevents the object from clipping through the ground or other objects
                        float yOffset = objCollider.bounds.extents.y + placementOffset;
                        
                        // Apply the offset along the surface normal
                        placementPosition = hit.point + hit.normal * yOffset;
                    }
                    
                    // Move the blueprint to the adjusted position
                    objectToPlace.transform.position = placementPosition;
                    
                    // Align blueprint with the surface normal
                    objectToPlace.transform.up = hit.normal;
                    
                    // Update UI text
                    InScreenUI.Instance.SetToolTipText("Release to place object");
                }
                else
                {
                    // No valid surface found
                    objectToPlace.transform.position = playerCamera.transform.position + playerCamera.transform.forward * grabRange/2;
                    objectToPlace.transform.up = Vector3.up; // Default orientation
                }
                    
                // Restore the original layer
                objectToPlace.layer = currentLayer;
            }

            void UpdateLookingAtObject()
            {
                lookingAtObject = null;

                // Raycast from the center of the camera's view
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, grabRange))
                {
                    lookingAtObject = hit.collider.GetComponent<IGrabableItem>();
                }

                // Check if the object has the IGrabbable interface
                if (lookingAtObject != null)
                {
                    InScreenUI.Instance.SetToolTipText("Press " +
                                                       InputReader.GetCurrentBindingText(InputReader.Instance.inputMap.Gameplay
                                                           .Grab) + " to grab  " + lookingAtObject.gameObject.name);
                }
                else
                {
                    InScreenUI.Instance.SetToolTipText("");
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
                    objToGrab.transform.SetParent(grabPlacement);
                    RPC_SendGrabMessageToOtherClients(playerCamera.transform.position, playerCamera.transform.forward);
                    
                }
                //place mechanic
                else if (!isBlueprintMode)
                {
                    GameObject objectToPlace = grabbedObject.gameObject;
                    
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
                if (Physics.Raycast(position, forward, out RaycastHit hit, grabRange))
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
            
            private void OnDrawGizmos()
            {
                if(playerCamera == null) {return;}
                Gizmos.color = Color.red;
                Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * grabRange);
            }
    }
}