using FishNet.Object;
using Items.Interfaces;
using UnityEngine;
using Utils;

public class HiderLookManager : NetworkBehaviour
{
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private float lookRange = 3.0f;
    [SerializeField] private Vector3 boxCastHalfExtents = new Vector3(0.5f, 1.0f, 0.1f);

    // The current object the player is looking at
    private GameObject currentLookTarget;
    
    private void Update()
    {
        // Clear previous result
        currentLookTarget = null;

        // Perform the raycast
        UpdateLookingAt();
    }

    private void UpdateLookingAt()
    {
        if (!IsOwner) { return;}
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        Quaternion orientation = transform.rotation;
        
        if (Physics.BoxCast(origin, boxCastHalfExtents, direction, out RaycastHit hit, orientation, lookRange, itemLayerMask))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);
            currentLookTarget = hit.collider.gameObject;
        }
        var grabable = currentLookTarget?.GetComponent<IGrabableItem>();
        var connectable = currentLookTarget?.GetComponent<IConnectable>();
        
        InScreenUI.Instance.SetToolTipText("");
        // Check if the object has the IGrabbable interface
        if (grabable != null)
        {
            if(InScreenUI.Instance != null)
            {
                InScreenUI.Instance.SetToolTipText("Press " + 
                                                   InputReader.GetCurrentBindingText(InputReader.Instance.inputMap.Gameplay.Grab) 
                                                   + " to grab  " + grabable.gameObject.name);
            }
        }

        if (connectable != null)
        {
            if(InScreenUI.Instance != null)
            {
                InScreenUI.Instance.AddToolTipText("Press " + 
                                                   InputReader.GetCurrentBindingText(InputReader.Instance.inputMap.Gameplay.ConnectItems) 
                                                   + " to connect  " + grabable.gameObject.name);
            }
        }

    }

    /// <summary>
    /// Get the object player is currently looking at
    /// </summary>
    public GameObject GetCurrentLookTarget()
    {
        return currentLookTarget;
    }
}
