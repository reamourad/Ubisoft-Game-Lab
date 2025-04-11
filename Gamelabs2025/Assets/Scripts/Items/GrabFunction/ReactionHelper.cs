using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.AI;

public class ReactionHelper : NetworkBehaviour
{
    public IReactionItem reactionItem;
    private bool isGrabbed = false;
    public float radius = 5f;
    public Vector3 detectionAreaBoxHalfExtent = new Vector3(5f, 5f, 5f);
    public LayerMask detectionLayerMask;
    public GameObject reactionArea;
    private List<Collider> collidersCache = new List<Collider>();
    public bool isConnectedToTrigger = false;
    public TriggerHelper connectedTriggerHelper;
    
    private void Start()
    {
        detectionAreaBoxHalfExtent = reactionArea.GetComponent<Renderer>().bounds.extents;
        detectionAreaBoxHalfExtent.y = 1f;
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
        isConnectedToTrigger = false;
        if (connectedTriggerHelper != null)
        {
            Debug.Log("isConnectedToReaction = false");
            connectedTriggerHelper.isConnectedToReaction = false;
            connectedTriggerHelper = null;
        }       
        RPC_OnServerGrab();
    }
    
    public void OnReleased()
    {
        isGrabbed = false;
        // Hide trigger areas when released
        foreach (Collider col in collidersCache)
        {
            var triggerHelper = col.GetComponent<TriggerHelper>();
            if (triggerHelper != null)
            {
                triggerHelper.HideTriggerArea();
            }
        }
        collidersCache.Clear();
        
        //check if trigger item is around
        Collider[] colliders= Physics.OverlapBox(gameObject.transform.position, detectionAreaBoxHalfExtent, Quaternion.identity,detectionLayerMask);
        var closestCollider = FindClosestTrigger(colliders);
        if(closestCollider == null) return;
        var tHelper = closestCollider.GetComponent<TriggerHelper>();
        //check if there is a trigger item nearby
        if (tHelper != null)
        {
            if (tHelper.isConnectedToReaction) return;
            //change variable for check of connection 
            tHelper.isConnectedToReaction = true;
            isConnectedToTrigger = true;
            connectedTriggerHelper = tHelper;
            tHelper.connectedReactionHelper = this;
            RPC_OnServerConnectToTrigger(closestCollider.GetComponent<NetworkObject>(), GetComponent<NetworkObject>());
        }
    }


    private Collider FindClosestTrigger(Collider[] colliders)
    {
        float minDistance = float.MaxValue;
        Collider closestCollider = null;
        foreach (Collider col in colliders)
        {
            if(col.gameObject == gameObject) continue;
            if(col.gameObject.GetComponent<TriggerHelper>() == null) continue;

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
        ConnectionDictionary.MakeConnection(trigger.GetComponent<ITriggerItem>(), GetComponent<IReactionItem>(), 
            trigger.transform, transform, true);
        RPC_OnClientConnectToTrigger(trigger, reaction);
    }

    [ObserversRpc]
    private void RPC_OnClientConnectToTrigger(NetworkObject trigger, NetworkObject reaction)
    {
        
        EnableCollision(true);
        ConnectionDictionary.MakeConnection(trigger.GetComponent<ITriggerItem>(), GetComponent<IReactionItem>(), 
            trigger.transform, transform, false);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RPC_OnServerGrab()
    {
        EnableCollision(false);
        ConnectionDictionary.ClearTriggerReactionEvents(null, GetComponent<IReactionItem>());
        RPC_OnClientGrab();
    }

    [ObserversRpc]
    private void RPC_OnClientGrab()
    {
        EnableCollision(false);
        ConnectionDictionary.ClearTriggerReactionEvents(null, GetComponent<IReactionItem>());
    }
    
    
    
    //shows the area you can connect to
    public void LateUpdate()
    {
       
        if (!isGrabbed) return;
        foreach (Collider col in collidersCache)
        {
            var triggerHelper = col.GetComponent<TriggerHelper>();
            if (triggerHelper == null) continue; 
            if(triggerHelper.isConnectedToReaction) continue;
            //check if the items are still in range
            float distance = Vector3.Distance(transform.position, col.gameObject.transform.position);
            if (distance > radius)
            {
                triggerHelper.HideTriggerArea();
            }
        }

        Collider[] colliders= Physics.OverlapSphere(gameObject.transform.position, radius, detectionLayerMask);
        foreach (Collider col in colliders)
        {
            if(col.gameObject == this.gameObject ) continue;
            var triggerHelper = col.GetComponent<TriggerHelper>();
            //check if there is a trigger item nearby
            if (triggerHelper != null)
            {
                if(triggerHelper.isConnectedToReaction) continue;
                triggerHelper.ShowTriggerArea();
            }
        }
        collidersCache = colliders.ToList();
    }

    public void ShowReactionArea()
    {
        reactionArea.SetActive(true);
    }
        
    public void HideReactionArea()
    {
        reactionArea.SetActive(false);
    }
}
