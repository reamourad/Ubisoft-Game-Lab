using System.Collections.Generic;

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
}
