using System.Collections.Generic;
using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

public static class ConnectionDictionary
{
    private static Dictionary<ITriggerItem, IReactionItem> connections = new Dictionary<ITriggerItem, IReactionItem>();

    public static void AddConnections(ITriggerItem trigger, IReactionItem reaction)
    {
        connections[trigger] = reaction;
    }
    
    public static void RemoveConnections(ITriggerItem trigger)
    {
        if (trigger == null)
            return; // Check if the trigger is null
        if (connections.ContainsKey(trigger))
        {
            connections.Remove(trigger); // Remove the connection
        }
    }
    
    public static IReactionItem GetConnectedReactions(ITriggerItem trigger)
    {
        // Check if the trigger is null
        if (trigger == null)
            return null; // Return null if the trigger is null
        return connections.TryGetValue(trigger, out var connection) ? connection : // Return the associated reaction item
            null; // No connection found
    }

    public static List<ITriggerItem> GetConnectedTriggers(IReactionItem reaction)
    {
        // Find all triggers that are connected to the given reaction item
        List<ITriggerItem> connectedTriggers = new List<ITriggerItem>();
        foreach (var kvp in connections)
        {
            if (kvp.Value == reaction)
            {
                connectedTriggers.Add(kvp.Key);
            }
        }
        return connectedTriggers; // Return the array of connected triggers
    }
    
    public static void ClearTriggerReactionEvents(ITriggerItem trigger, IReactionItem reaction)
    {
        if (trigger != null)
        {
            if (trigger.rope != null)
            {
                GameObject.Destroy(trigger.rope.gameObject);
            }
            var connectedReaction = GetConnectedReactions(trigger);
            if (connectedReaction != null && connectedReaction.rope != null)
            {
                GameObject.Destroy(connectedReaction.rope.gameObject);
            }
            if (connectedReaction != null)
            {
                trigger.OnTriggerActivated -= connectedReaction.OnTrigger;
            }
            RemoveConnections(trigger);
        }

        if (reaction != null) {
            var connectedTriggers = GetConnectedTriggers(reaction);
            if (connectedTriggers != null)
            {
                foreach (var connectedTrigger in connectedTriggers)
                {
                    if (connectedTrigger != null && connectedTrigger.rope != null)
                    {
                        GameObject.Destroy(connectedTrigger.rope.gameObject);
                    }
                    connectedTrigger.OnTriggerActivated -= reaction.OnTrigger;
                    RemoveConnections(trigger);
                }
            }
        }
    }
        
    public static void MakeConnection(ITriggerItem trigger, IReactionItem reaction, Transform triggerTransform, Transform reactionTransform, bool isServer)
    {
        if (trigger != null && reaction != null)
        {
            ClearTriggerReactionEvents(trigger, reaction);
            
            if (!isServer)
            {
                var prefab = Resources.Load<GameObject>("RopePrefab");
                Debug.Log(prefab);
                var currentRope = Rope.CreateRope(prefab, triggerTransform, reactionTransform);
                Debug.Log(currentRope);
                trigger.rope = currentRope;
                reaction.rope = currentRope;
            }

            AddConnections(trigger, reaction);
            trigger.OnTriggerActivated += reaction.OnTrigger;
        }
    }
}
