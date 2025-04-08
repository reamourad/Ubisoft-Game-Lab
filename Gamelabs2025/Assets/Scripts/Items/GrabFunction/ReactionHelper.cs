using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using NUnit.Framework.Constraints;
using UnityEngine;

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
    public Transform reactionAnchor; //where the wire should come from
    
    private void Start()
    {
        detectionAreaBoxHalfExtent = reactionArea.GetComponent<Renderer>().bounds.extents;
        detectionAreaBoxHalfExtent.y = 5f;
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
        foreach (Collider col in colliders)
        {
            if(col.gameObject == this.gameObject) continue;
            var triggerHelper = col.GetComponent<TriggerHelper>();
            //check if there is a trigger item nearby
            if (triggerHelper != null)
            {
                if (triggerHelper.isConnectedToReaction) continue;
                //change variable for check of connection 
                triggerHelper.isConnectedToReaction = true;
                isConnectedToTrigger = true;
                connectedTriggerHelper = triggerHelper;
                triggerHelper.connectedReactionHelper = this;
                RPC_OnServerConnectToTrigger(col.GetComponent<NetworkObject>(), GetComponent<NetworkObject>());
                return; 
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RPC_OnServerConnectToTrigger(NetworkObject trigger, NetworkObject reaction)
    {
        ConnectionDictionary.MakeConnection(trigger.GetComponent<ITriggerItem>(), GetComponent<IReactionItem>(), 
            trigger.GetComponent<TriggerHelper>().triggerAnchor, reactionAnchor);
        RPC_OnClientConnectToTrigger(trigger, reaction);
    }

    [ObserversRpc]
    private void RPC_OnClientConnectToTrigger(NetworkObject trigger, NetworkObject reaction)
    {
        ConnectionDictionary.MakeConnection(trigger.GetComponent<ITriggerItem>(), GetComponent<IReactionItem>(), 
            trigger.GetComponent<TriggerHelper>().triggerAnchor, reactionAnchor);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RPC_OnServerGrab()
    {
        ConnectionDictionary.ClearTriggerReactionEvents(null, GetComponent<IReactionItem>());
        RPC_OnClientGrab();
    }

    [ObserversRpc]
    private void RPC_OnClientGrab()
    {
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
