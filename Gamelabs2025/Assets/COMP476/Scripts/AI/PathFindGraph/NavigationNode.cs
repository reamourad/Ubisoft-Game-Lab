using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class NavigationNode : MonoBehaviour
{
    [Header("Connection Settings")]
    public float connectionRadius = 100f;
    public LayerMask obstacleMask;
    public bool autoConnectOnCreate = true;

    [Header("Visual Settings")]
    public Color nodeColor = Color.cyan;
    [Range(0.1f, 2f)] public float nodeSize = 0.5f;

    private Vector3 lastPosition;
    private bool isInitialized = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (this == null) return; // Safety check

        // Only run in editor, not in play mode
        if (autoConnectOnCreate && !Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) AutoConnectToNearbyNodes();
            };
        }
    }

    private void OnEnable()
    {
        if (this == null) return;
        lastPosition = transform.position;
        EditorApplication.update += EditorUpdate;
        isInitialized = true;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        if (!isInitialized || this == null) return;

        if (transform.position != lastPosition)
        {
            lastPosition = transform.position;
            OnNodeMoved();
        }
    }
#endif

    private void OnDestroy()
    {
        if (this == null) return;
        CleanUpConnections();
    }

    public void CleanUpConnections()
    {
        if (this == null) return;

        NavigationGraph graph = GetComponentInParent<NavigationGraph>();
        if (graph == null || graph.connections == null) return;

        // Remove all connections that involve this node
        graph.connections.RemoveAll(conn =>
            conn != null &&
            conn.fromNode == this ||
            conn.toNode == this);

        Debug.Log($"Removed all connections for node {name}");
    }

    public void AutoConnectToNearbyNodes()
    {
        if (this == null) return;

        NavigationGraph graph = GetComponentInParent<NavigationGraph>();
        if (graph == null) return;

        // Find all other nodes in the graph
        NavigationNode[] allNodes = graph.GetComponentsInChildren<NavigationNode>();
        if (allNodes == null) return;

        foreach (NavigationNode otherNode in allNodes)
        {
            if (otherNode == null || otherNode == this) continue;

            float distance = Vector3.Distance(transform.position, otherNode.transform.position);

            // Check if within connection radius
            if (distance <= connectionRadius)
            {
                // Check for obstacles between nodes
                if (!Physics.Linecast(transform.position, otherNode.transform.position, obstacleMask))
                {
                    graph.AddConnection(this, otherNode);
                }
                else
                {
                    //Debug.Log("Blocked");
                }
            }
        }
    }

    private void OnNodeMoved()
    {
        if (this == null) return;

        //Debug.Log($"Node {name} moved to {transform.position}");

        // Re-establish new connections
        if (autoConnectOnCreate)
        {
            AutoConnectToNearbyNodes();
        }
    }

    private void OnDrawGizmos()
    {
        if (this == null) return;

        // Draw the node
        Gizmos.color = new Color(nodeColor.r, nodeColor.g, nodeColor.b, 0.7f);
        Gizmos.DrawSphere(transform.position, nodeSize);

        Gizmos.color = nodeColor;
        Gizmos.DrawWireSphere(transform.position, nodeSize);
    }

    // Public method to manually connect nodes
    public void ManualConnectTo(NavigationNode otherNode)
    {
        if (this == null || otherNode == null) return;

        NavigationGraph graph = GetComponentInParent<NavigationGraph>();
        if (graph == null) return;

        graph.AddConnection(this, otherNode);
    }

    // Public method to manually disconnect nodes
    public void ManualDisconnectAll()
    {
        if (this == null) return;
        CleanUpConnections();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NavigationNode))]
public class NavigationNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        NavigationNode node = (NavigationNode)target;
        if (node == null) return; // Important safety check

        DrawDefaultInspector();

        GUILayout.Space(10);
        if (GUILayout.Button("Disconnect All", GUILayout.Height(30)))
        {
            Undo.RecordObject(node, "Disconnect All");
            node.ManualDisconnectAll();
            EditorUtility.SetDirty(node);
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Reconnect (Auto)", GUILayout.Height(30)))
        {
            Undo.RecordObject(node, "Reconnect Auto");
            node.AutoConnectToNearbyNodes();
            EditorUtility.SetDirty(node);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif