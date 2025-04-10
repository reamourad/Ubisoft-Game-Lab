using System.ComponentModel.Design.Serialization;
using System.Runtime.InteropServices.WindowsRuntime;
using FishNet.Object;
using HighlightPlus;
using Items.Interfaces;
using Networking;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

public class HiderLookManager : NetworkBehaviour
{
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private float lookRange = 3.0f;
    [SerializeField] private Vector3 boxCastHalfExtents = new Vector3(0.5f, 1.0f, 0.1f);
    public bool isActive = true;
    
    private NetworkPlayerConnectionController playerConnectionController;

    // The current object the player is looking at
    private GameObject currentLookTarget;
    
    private void Start()
    {
        playerConnectionController = GetComponent<NetworkPlayerConnectionController>();
    }
    
    private void Update()
    {
        // Clear previous highlight
        if(currentLookTarget != null)
            HighlightObject(currentLookTarget, false);
        
        currentLookTarget = null;

        // Perform the raycast
        UpdateLookingAt();

        if (currentLookTarget != null && IsOwner)
        {
            HighlightObject(currentLookTarget, true);
        }
           
    }

    private void HighlightObject(GameObject obj, bool highlight)
    {
        var he = GhostHighlighterRegisterer.GetHighlightEffect(obj);
        if (he != null)
        {
            he.highlighted = highlight;
            Debug.Log(he.name);
        }
    }

    private void UpdateLookingAt()
    {
        if (!IsOwner || !isActive) { return;}
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        Quaternion orientation = transform.rotation;
        
        if (Physics.BoxCast(origin, boxCastHalfExtents, direction, out RaycastHit hit, orientation, lookRange))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);
            currentLookTarget = hit.collider.gameObject;
        }

        if (currentLookTarget == null)
        {
            return;
        }

        var grabable = currentLookTarget.GetComponent<IHiderGrabableItem>();
        var connectable = currentLookTarget.GetComponent<IConnectable>();
        
        if (InScreenUI.Instance == null) return; 
        
        InScreenUI.Instance.RemoveInputPrompt(InputReader.Instance.inputMap.Gameplay.Grab);
        InScreenUI.Instance.RemoveInputPrompt(InputReader.Instance.inputMap.Gameplay.Look);
        
        if (grabable != null)
        {
            InScreenUI.Instance.ShowInputPrompt(InputReader.Instance.inputMap.Gameplay.Grab, "Grab");
        }

        if (connectable != null)
        {
            InScreenUI.Instance.ShowInputPrompt(InputReader.Instance.inputMap.Gameplay.ConnectItems, "Connect");
        }    
    }           


    public void SetActive(bool active)
    {
        isActive = active; 
        if (active == false)
        {
            if(IsOwner)
                InScreenUI.Instance.ClearInputPrompts();
        }
    }
    
    /// Get the object player is currently looking at
    public GameObject GetCurrentLookTarget()
    {
        return currentLookTarget;
    }
}
