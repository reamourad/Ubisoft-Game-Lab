using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;

public class TriggerHelper : NetworkBehaviour
{

     public ITriggerItem triggerItem;
    private bool isGrabbed = false;
    public float radius = 5f;
    public Vector3 detectionAreaBoxHalfExtent = new Vector3(5f, 5f, 5f);
    public LayerMask detectionLayerMask;
    public GameObject triggerArea;
    private List<Collider> collidersCache = new List<Collider>();
    public bool isConnectedToReaction = false;
    public ReactionHelper connectedReactionHelper;
    public Transform triggerAnchor; //where the wire should come from
    
    private void Start()
    {
        detectionAreaBoxHalfExtent = triggerArea.GetComponent<Renderer>().bounds.extents;
        detectionAreaBoxHalfExtent.y = 5f;
    }
    public void OnGrabbed()
    {
        Debug.Log("Trigger item Grabbed");
        isGrabbed = true;
        //undo the connection 
        isConnectedToReaction = false;
        if (connectedReactionHelper != null)
        {
            Debug.Log("isConnectedToTrigger = false");
            connectedReactionHelper.isConnectedToTrigger = false;
            connectedReactionHelper = null;
        }       
        RPC_OnServerGrab();
    }
    
    public void OnReleased()
    {
        Debug.Log("Trigger item Released");
        isGrabbed = false;
        // Hide trigger areas when released
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
        Collider[] colliders= Physics.OverlapBox(gameObject.transform.position, detectionAreaBoxHalfExtent, Quaternion.identity,detectionLayerMask);
        foreach (Collider col in colliders)
        {
            if(col.gameObject == this.gameObject) continue;
            var reactionHelper = col.GetComponent<ReactionHelper>();
            //check if there is a trigger item nearby
            if (reactionHelper != null)
            {
                if (reactionHelper.isConnectedToTrigger) continue;
                //change variable for check of connection 
                reactionHelper.isConnectedToTrigger = true;
                isConnectedToReaction = true;
                connectedReactionHelper = reactionHelper;
                reactionHelper.connectedTriggerHelper = this;
                RPC_OnServerConnectToTrigger(GetComponent<NetworkObject>(), col.GetComponent<NetworkObject>());
                return; 
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RPC_OnServerConnectToTrigger(NetworkObject trigger, NetworkObject reaction)
    {
        ConnectionDictionary.MakeConnection(GetComponent<ITriggerItem>(), reaction.GetComponent<IReactionItem>(), 
            triggerAnchor, reaction.GetComponent<ReactionHelper>().reactionAnchor);
        RPC_OnClientConnectToTrigger(trigger, reaction);
    }

    [ObserversRpc]
    private void RPC_OnClientConnectToTrigger(NetworkObject trigger, NetworkObject reaction)
    {
        ConnectionDictionary.MakeConnection(GetComponent<ITriggerItem>(), reaction.GetComponent<IReactionItem>(), 
            triggerAnchor, reaction.GetComponent<ReactionHelper>().reactionAnchor);
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
            Debug.Log(col.gameObject.name);
            var reactionHelper = col.GetComponent<ReactionHelper>();
            if (reactionHelper == null) continue; 
            if(reactionHelper.isConnectedToTrigger) continue;
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
            if(col.gameObject == this.gameObject ) continue;
            var reactionHelper = col.GetComponent<ReactionHelper>();
            //check if there is a trigger item nearby
            if (reactionHelper != null)
            {
                if(reactionHelper.isConnectedToTrigger) continue;
                reactionHelper.ShowReactionArea();
            }
        }
        collidersCache = colliders.ToList();
    }

    public void ShowTriggerArea()
    {
        triggerArea.SetActive(true);
    }
        
    public void HideTriggerArea()
    {
        triggerArea.SetActive(false);
    }
}
