//OUTDATED GO TO NetworkPlayerItemController

/*using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HiderController : PlayerController
{
    [SerializeField] private int grabRange;
    
    public Transform grabPlacement; 
    private IGrabableItem lookingAtObject = null; 
    private IGrabableItem grabbedObject = null; 
    [SerializeField] private InScreenUI inScreenUI;
    
    //this is variables for the placement mechanic
    public Material ghostMaterial;
    private Material originalMaterial; 
    public bool isBlueprintMode = false;
    private bool isGrabButtonHeld = false;
    private int originalLayer = 0;
    [SerializeField] private float placementOffset = 0.05f;
    
    [SerializeField] private Transform wireAnchor;
    private ITriggerItem holdingWireItem;
    public static event Action<bool> OnHoldingWireItem;
    
    // Define a specific layer for temporarily placing the grabbed object
    private const int IGNORE_RAYCAST_LAYER = 2;
    
    // Update is called once per frame
    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isBlueprintMode && isGrabButtonHeld && grabbedObject != null)
        {
            //place mechanic
            FixedUpdateBlueprintMode(); 
        }
        else if (!isBlueprintMode)
        {
            //grab mechanic
           FixedUpdateLookingAtObject(); 
        }
    }

    void FixedUpdateBlueprintMode()
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
            if (inScreenUI != null)
            {
                inScreenUI.toolTipText.text = "Release to place object";
            }
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

    void FixedUpdateLookingAtObject()
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
            inScreenUI.toolTipText.gameObject.SetActive(true);
            inScreenUI.toolTipText.text = "Press " +
                                          InputReader.GetCurrentBindingText(InputReader.Instance.inputMap.Gameplay
                                              .Grab) + " to grab  " + lookingAtObject.gameObject.name;
        }
        else
        {
            inScreenUI.toolTipText.gameObject.SetActive(false);
        }
    }
    
    // Called when grab button is pressed
    public override void OnGrab()
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
            
            if (inScreenUI != null)
            {
                inScreenUI.toolTipText.gameObject.SetActive(true);
                inScreenUI.toolTipText.text = "Release to place object";
            }
        }
    }

    // Called when grab button is released
    public override void OnGrabRelease()
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
            if (inScreenUI != null)
            {
                inScreenUI.toolTipText.gameObject.SetActive(false);
            }
        }
    }

    public override void OnConnection()
    {
        if (lookingAtObject == null || grabbedObject != null)
        {
            return;
        }
        
        if (lookingAtObject.gameObject.TryGetComponent(out ITriggerItem triggerItem))
        {
            triggerItem.Connect(wireAnchor);
            holdingWireItem = triggerItem;
            OnHoldingWireItem?.Invoke(true);
        }
        else if (lookingAtObject.gameObject.TryGetComponent(out IReactionItem reactionItem))
        {
            holdingWireItem.Connect(reactionItem.WireAnchor);
            holdingWireItem = null;
            OnHoldingWireItem?.Invoke(false);
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * grabRange);
    }
}*/