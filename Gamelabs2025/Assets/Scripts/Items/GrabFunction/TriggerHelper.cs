using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using HighlightPlus;
using UnityEngine;
using UnityEngine.AI;

public class TriggerHelper : NetworkBehaviour
{
    private bool isGrabbed = false;
    public float radius = 5f;
    public Vector3 detectionAreaBoxHalfExtent = new Vector3(5f, 5f, 5f);
    public LayerMask detectionLayerMask;
    public GameObject triggerArea;
    public bool isConnectedToReaction = false;
    public ReactionHelper connectedReactionHelper;
    public GameObject lineRendererPrefab;
    private LineRenderer currentLineRenderer;
    
    private void Start()
    {
        detectionAreaBoxHalfExtent = triggerArea.GetComponent<Renderer>().bounds.extents;
        detectionAreaBoxHalfExtent.y = 1f;
        lineRendererPrefab = Resources.Load<GameObject>("LineRendererConnectionPrefab");
    }
    
    private void EnableCollision(bool enable)
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = enable;
        }
    }
    
    public void OnGrabbed()
    {
        isGrabbed = true;
        //undo the connection 
        isConnectedToReaction = false;
        if (connectedReactionHelper != null)
        {
            Debug.Log("isConnectedToReaction = false");
            connectedReactionHelper.isConnectedToTrigger = false;
            connectedReactionHelper = null;
        }       
        RPC_OnServerGrab();
        
        if (TryGetComponent<HighlightEffect>(out var hightlightEffect))
        {
            hightlightEffect.highlighted = true;
        }
    }
    
    public void OnReleased()
    {
        if (currentLineRenderer != null)
        {
            Destroy(currentLineRenderer.gameObject);
            currentLineRenderer = null;
        }
        isGrabbed = false;
        
        //check if trigger item is around
        Collider[] colliders= Physics.OverlapBox(gameObject.transform.position, detectionAreaBoxHalfExtent, Quaternion.identity,detectionLayerMask);
        var closestCollider = FindClosestTrigger(colliders);
        if(closestCollider == null) return;
        var rHelper = closestCollider.GetComponent<ReactionHelper>();
        //check if there is a trigger item nearby
        if (rHelper != null)
        {
            if (rHelper.isConnectedToTrigger) return;
            //change variable for check of connection 
            rHelper.isConnectedToTrigger = true;
            isConnectedToReaction = true;
            connectedReactionHelper = rHelper;
            rHelper.connectedTriggerHelper = this;
            RPC_OnServerConnectToTrigger(GetComponent<NetworkObject>(), closestCollider.GetComponent<NetworkObject>());
        }
        
        if (TryGetComponent<HighlightEffect>(out var hightlightEffect))
        {
            hightlightEffect.highlighted = false;
        }
    }


    private Collider FindClosestTrigger(Collider[] colliders)
    {
        float minDistance = float.MaxValue;
        Collider closestCollider = null;
        foreach (Collider col in colliders)
        {
            if(col.gameObject == gameObject) continue;
            if(col.gameObject.GetComponent<ReactionHelper>() == null) continue;

            if (Mathf.Abs(transform.position.y - col.transform.position.y) > 2f) continue; 
            float currentDistance = GetPathDistance(col.gameObject.transform.position, transform.position);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                closestCollider = col;
            }
        }

        return closestCollider; 
    }
    
    float GetPathDistance(Vector3 start, Vector3 end)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
        {
            if (path.status != NavMeshPathStatus.PathComplete)
                return -1f; // No valid path

            float distance = 0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return distance;
        }
        return -1f;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RPC_OnServerConnectToTrigger(NetworkObject trigger, NetworkObject reaction)
    {
        EnableCollision(true);
        ConnectionDictionary.MakeConnection(GetComponent<ITriggerItem>(), reaction.GetComponent<IReactionItem>(), 
            transform, reaction.transform, true);
        RPC_OnClientConnectToTrigger(trigger, reaction);
    }

    [ObserversRpc]
    private void RPC_OnClientConnectToTrigger(NetworkObject trigger, NetworkObject reaction)
    {
        
        EnableCollision(true);
        ConnectionDictionary.MakeConnection(GetComponent<ITriggerItem>(), reaction.GetComponent<IReactionItem>(), 
            transform, reaction.transform, false);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RPC_OnServerGrab()
    {
        EnableCollision(false);
        ConnectionDictionary.ClearTriggerReactionEvents(GetComponent<ITriggerItem>(), null);
        RPC_OnClientGrab();
    }

    [ObserversRpc]
    private void RPC_OnClientGrab()
    {
        EnableCollision(false);
        ConnectionDictionary.ClearTriggerReactionEvents(GetComponent<ITriggerItem>(), null);
    }
    
    //shows the area you can connect to
    public void LateUpdate()
    {
        if (!isGrabbed) return;
        
        Collider[] colliders= Physics.OverlapBox(gameObject.transform.position, detectionAreaBoxHalfExtent, Quaternion.identity,detectionLayerMask);
        Collider triggerCollider = FindClosestTrigger(colliders);
        if (triggerCollider != null)
        {
            // Create the line renderer if it doesn't exist
            if ( currentLineRenderer == null)
            {
                var lineRendererGameObject = Instantiate(lineRendererPrefab);
                currentLineRenderer = lineRendererGameObject.GetComponent<LineRenderer>();
            }
        
            // Set the positions
            currentLineRenderer.positionCount = 2;
            currentLineRenderer.SetPosition(0, transform.position);
            currentLineRenderer.SetPosition(1, triggerCollider.transform.position);
        }
        else
        {
            // Destroy the line renderer if there's no trigger in range
            if (currentLineRenderer != null)
            {
                currentLineRenderer.positionCount = 0;
            }
        }
    }           
}
