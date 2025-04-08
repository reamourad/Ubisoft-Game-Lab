using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;

public class TriggerHelper : NetworkBehaviour
{

    public ITriggerItem triggerItem;
    private bool isGrabbed = false;
    public float radius = 5f;
    public GameObject triggerArea;
    public LayerMask detectionLayerMask;
    private List<Collider> collidersCache = new List<Collider>();
    public bool isConnectedToReaction = false;

    public void ShowTriggerArea()
    {
        triggerArea.SetActive(true);
    }

    public void HideTriggerArea()
    {
        triggerArea.SetActive(false);
    }
    public void OnGrabbed()
    {
        isGrabbed = true;
        RPC_OnServerGrab();
    }
    
    public void OnReleased()
    {
        isGrabbed = false;
        // Hide reaction areas when released
        foreach (Collider col in collidersCache)
        {
            var reactionHelper = col.GetComponent<ReactionHelper>();
            if (reactionHelper != null)
            {
                reactionHelper.HideReactionArea();
            }
        }
        collidersCache.Clear();
        
        //check if trigger item is around
        Collider[] colliders= Physics.OverlapSphere(gameObject.transform.position, radius, detectionLayerMask);
        foreach (Collider col in colliders)
        {
            if(col.gameObject == this.gameObject) continue;
            var reactionHelper = col.GetComponent<ReactionHelper>();
            //check if there is a reaction item nearby
            if (reactionHelper != null)
            {
                RPC_OnServerConnectToTrigger(GetComponent<NetworkObject>(), col.GetComponent<NetworkObject>());
                return; 
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RPC_OnServerConnectToTrigger(NetworkObject trigger, NetworkObject reaction)
    {
        ConnectionDictionary.MakeConnection(GetComponent<ITriggerItem>(), reaction.GetComponent<IReactionItem>(), transform, reaction.transform);
        RPC_OnClientConnectToTrigger(trigger, reaction);
    }

    [ObserversRpc]
    private void RPC_OnClientConnectToTrigger(NetworkObject trigger, NetworkObject reaction)
    {
        ConnectionDictionary.MakeConnection(GetComponent<ITriggerItem>(), reaction.GetComponent<IReactionItem>(), transform, reaction.transform);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RPC_OnServerGrab()
    {
        ConnectionDictionary.ClearTriggerReactionEvents(GetComponent<ITriggerItem>(), null);
        RPC_OnClientGrab();
    }

    [ObserversRpc]
    private void RPC_OnClientGrab()
    {
        ConnectionDictionary.ClearTriggerReactionEvents(GetComponent<ITriggerItem>(), null);
    }
    
    
    
    //shows the area you can connect to
    public void LateUpdate()
    {
        if (!isGrabbed) return;
        foreach (Collider col in collidersCache)
        {
            var reactionHelper = col.GetComponent<ReactionHelper>();
            if (reactionHelper == null) continue; 
            //check if the items are still in range
            float distance = Vector3.Distance(transform.position, col.gameObject.transform.position);
            if (distance > radius)
            {
                reactionHelper.HideReactionArea();
            }
        }

        Collider[] colliders= Physics.OverlapSphere(gameObject.transform.position, radius, detectionLayerMask);
        foreach (Collider col in colliders)
        {
            if(col.gameObject == this.gameObject) continue;
            var reactionHelper = col.GetComponent<ReactionHelper>();
            //check if there is a trigger item nearby
            if (reactionHelper != null)
            {
                reactionHelper.ShowReactionArea();
            }
        }
        collidersCache = colliders.ToList();
    }

    private void ShowReactionArea()
    {
        triggerArea.SetActive(true);
    }
        
    private void HideReactionArea()
    {
        triggerArea.SetActive(false);
    }
}
