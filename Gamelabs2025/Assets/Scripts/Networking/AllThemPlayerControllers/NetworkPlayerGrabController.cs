using System;
using System.Collections.Generic;
using FishNet.Object;
using Items.Interfaces;
using Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Networking
{
    [RequireComponent(typeof(HiderLookManager))]
    public class NetworkPlayerGrabController : NetworkBehaviour
    {
        [SerializeField] private int grabRange;
    
        public Transform grabPlacement; 
        private NetworkObject lookingAtObject = null; 
        private NetworkObject grabbedObject = null; 
        private Camera playerCamera;
        private HiderLookManager hiderLookManager;
    
        //this is variables for the placement mechanic
        public Material ghostMaterial;
        public Material invalidPlacementMaterial;
        private Dictionary<Renderer, Material> originalMaterial;
        private Transform originalTransform; 
        private bool isBlueprintMode = false;
        private bool isGrabButtonHeld = false;
        private int originalLayer = 0;
        [SerializeField] private float placementOffset = 0.05f;
        
        [SerializeField] float minPlacementDistance = 2.0f;
    
        // Define a specific layer for temporarily placing the grabbed object
        private const int IGNORE_RAYCAST_LAYER = 2;
        [SerializeField] private LayerMask itemLayerMask;
        private Collider originalBoxCollider = null;

        private void Start()
        {
            playerCamera = Camera.main;
            hiderLookManager = GetComponent<HiderLookManager>();
            originalMaterial = new Dictionary<Renderer, Material>();
        }

        // Update is called once per frame
            void Update()
            {
                if (isBlueprintMode && grabbedObject != null)
                {
                    //place mechanic
                    originalTransform = grabbedObject.transform;
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

                    if (originalBoxCollider != null)
                    {
                        float yOffset = originalBoxCollider.bounds.extents.y + placementOffset;
                        placementPosition = hit.point + hit.normal * yOffset;
                    }
                    
                    objectToPlace.transform.position = placementPosition;
                    objectToPlace.transform.up = hit.normal;
                    
                    SendToClosestNavMeshPoint(placementPosition, objectToPlace);
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
    
                        if (originalBoxCollider != null)
                        {
                            yOffset = originalBoxCollider.bounds.extents.y;
                        }


                        objectToPlace.transform.position = floorHit.point + new Vector3(0, yOffset, 0);
                        objectToPlace.transform.up = floorHit.normal; // Align with floor normal
                    }
                }
                objectToPlace.layer = currentLayer;
            }

            void SendToClosestNavMeshPoint(Vector3 position, GameObject objectToPlace)
            {
                //put the object on the floor 
                RaycastHit floorHit;
                if (Physics.Raycast(position, Vector3.down, out floorHit, 20f))
                {
                    Renderer objectRenderer = objectToPlace.GetComponent<Renderer>();
                    float yOffset = 0f;
    
                    if (originalBoxCollider != null)
                    {
                        yOffset = originalBoxCollider.bounds.extents.y;
                    }


                    objectToPlace.transform.position = floorHit.point + new Vector3(0, yOffset, 0);
                    objectToPlace.transform.up = floorHit.normal; // Align with floor normal
                }
                
                NavMeshHit navHit;
                bool onNavMesh = NavMesh.SamplePosition(position, out navHit, 2.0f, NavMesh.AllAreas);
                if (onNavMesh)
                {
                    Renderer objectRenderer = objectToPlace.GetComponent<Renderer>();
                    float yOffset = 0f;
    
                    if (objectRenderer != null)
                    {
                        Bounds bounds = objectRenderer.bounds;
                        yOffset = bounds.extents.y;
                    }

                    objectToPlace.transform.position = navHit.position + new Vector3(0, yOffset, 0);
                    objectToPlace.transform.up = navHit.normal; // Align with floor normal
                    
                    // Update UI text
                    InScreenUI.Instance.SetToolTipText("Release to place object");
                }
                else
                {
                    SetGrabbedObjectMaterial(invalidPlacementMaterial);
                }
            }

            void SetGrabbedObjectMaterial(Material material)
            {
                Renderer renderer = grabbedObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = material;
                }
            }

            void UpdateLookingAtObject()
            {
                if (hiderLookManager.GetCurrentLookTarget()?.GetComponent<IHiderGrabableItem>() != null)
                {
                    lookingAtObject = hiderLookManager.GetCurrentLookTarget().GetComponent<NetworkObject>(); 
                }
                else
                {
                    lookingAtObject = null;
                }
            }
            

            // Called when grab button is pressed
            public void OnGrab()
            {
                //isGrabButtonHeld = true;
                
                //grab mechanic
                if (grabbedObject == null)
                {
                    // PickupObject(lookingAtObject);
                    // //inform server 
                    // RPC_InformServerOnGrab(grabbedObject);
                    grabbedObject = lookingAtObject;
                    if (grabbedObject != null)
                    {
                        originalBoxCollider = grabbedObject.GetComponent<BoxCollider>();
                        RPC_InformServerOnGrab(grabbedObject); 
                        EnterBlueprintMode();
                    }
                        
                }
                else
                {
                    //already grabbed something
                    OnGrabRelease();
                }
                //place mechanic
                // else if (!isBlueprintMode)
                // {
                //    EnterBlueprintMode();
                // }
            }

            void PickupObject(NetworkObject networkObject)
            {
                /*//nothing happens if youre not currently looking at something
                if (networkObject == null) { return; }
                
                hiderLookManager.SetActive(false);

                if (IsOwner)
                {
                    var dir = (networkObject.transform.position - transform.position).normalized;
                    if (Physics.Raycast(grabPlacement.transform.position, dir, out RaycastHit hit, 20f))
                    {
                        Debug.Log($"Hit Collider (Hider): {hit.collider.name}");
                        if(hit.collider != null && hit.collider.gameObject != networkObject.gameObject)
                            return;
                    }
                }
                
                
                // Move to object to grab placement and parent with the player
                grabbedObject = networkObject;
                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                if (rb != null) 
                {
                    rb.isKinematic = true;
                }
                
                grabbedObject.transform.position = grabPlacement.position;
                grabbedObject.transform.SetParent(this.transform);
                
                //grab function helpers 
                var reactionHelper = grabbedObject.GetComponent<ReactionHelper>();
                if (reactionHelper != null)
                {
                    if(IsOwner)
                        reactionHelper.OnGrabbed();
                }
                
                var triggerHelper = grabbedObject.GetComponent<TriggerHelper>();
                if (triggerHelper != null)
                {
                    if(IsOwner)
                        triggerHelper.OnGrabbed();
                }*/
                
                grabbedObject = networkObject;
            }

            void EnterBlueprintMode()
            {
                //grab function helpers 
                var reactionHelper = grabbedObject.GetComponent<ReactionHelper>();
                if (reactionHelper != null)
                {
                    if(IsOwner)
                        reactionHelper.OnGrabbed();
                }
                
                var triggerHelper = grabbedObject.GetComponent<TriggerHelper>();
                if (triggerHelper != null)
                {
                    if(IsOwner)
                        triggerHelper.OnGrabbed();
                }
                
                hiderLookManager.SetActive(true);
                grabbedObject.transform.parent = null; 
                // keep original data for the material + layer
                originalMaterial.Clear();
                Renderer[] renderer = grabbedObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderer)
                {
                    if (r != null) 
                    {
                        originalMaterial.Add(r, r.material);
                        r.material = ghostMaterial; // Change to blueprint material
                    }
                }
                
                originalLayer = grabbedObject.gameObject.layer;
                    
                // Set blueprint mode 
                isBlueprintMode = true;

                InScreenUI.Instance.SetToolTipText("Release to place object");
            }
            
            [ServerRpc]
            private void RPC_InformServerOnGrab(NetworkObject obj)
            {
                Debug.Log("Received Grab Message from observer");
                PickupObject(obj);
                BroadcastPickupToClients(obj);
            }
            
            [ObserversRpc(ExcludeOwner = true)]
            void BroadcastPickupToClients(NetworkObject obj){
                PickupObject(obj);
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
                    grabbedObject.gameObject.layer = originalLayer;
                    

                    //Restore original material
                    Renderer[] renderer = grabbedObject.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in renderer)
                    {
                        if (r != null && originalMaterial != null)
                        {
                            r.material = originalMaterial[r];
                        }
                    }

                    //turn collision back on 
                    //set the collision off 
                    Collider collider = grabbedObject.GetComponent<Collider>();
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                    
                    //inform server 
                    Debug.Log("Informing server to place object " + grabbedObject.name);
                    RPC_InformServerOnPlace(grabbedObject.transform.position, grabbedObject.transform.rotation);
                    
                    
                    //placed a reaction item down
                    var reactionHelper = grabbedObject.GetComponent<ReactionHelper>();
                    Debug.Log(grabbedObject.name);
                    if (reactionHelper != null)
                    {
                        reactionHelper.OnReleased();
                    }
                    
                    //placed a trigger item down
                    var triggerHelper = grabbedObject.GetComponent<TriggerHelper>();
                    Debug.Log(grabbedObject.name);
                    if (triggerHelper != null)
                    {
                        triggerHelper.OnReleased();
                    }
                    //Reset variables
                    grabbedObject = null;
                    isBlueprintMode = false;
                    
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
                    //rb.isKinematic = false;
                }
                //update the position and rotation
                grabbedObject.transform.position = position;
                grabbedObject.transform.rotation = rotation;
                
                grabbedObject = null;
                
               
            }
    }
}