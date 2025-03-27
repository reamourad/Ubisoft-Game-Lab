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
    public bool manualModeOnly = false;  // New flag to disable all automatic behavior

    [Header("Visual Settings")]
    public Color nodeColor = Color.cyan;
    [Range(0.1f, 2f)] public float nodeSize = 0.5f;

    private Vector3 lastPosition;
    private bool isInitialized = false;

    private void Start()
    {
        if (!manualModeOnly && autoConnectOnCreate && Application.isPlaying)
        {
            AutoConnectToNearbyNodes();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (manualModeOnly) return;

        CleanUpConnections();
        // Only run in editor, not in play mode
        if (autoConnectOnCreate && !Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () => AutoConnectToNearbyNodes();
        }
    }

    private void OnEnable()
    {
        if (manualModeOnly) return;

        lastPosition = transform.position;
        EditorApplication.update += EditorUpdate;
        isInitialized = true;
    }

    private void OnDisable()
    {
        if (manualModeOnly) return;

        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        if (manualModeOnly || !isInitialized) return;

        if (transform.position != lastPosition)
        {
            lastPosition = transform.position;
            OnNodeMoved();
        }
    }
#endif

    private void OnDestroy()
    {
        if (manualModeOnly) return;
        CleanUpConnections();
    }

    public void CleanUpConnections()
    {
        NavigationGraph graph = GetComponentInParent<NavigationGraph>();
        if (graph == null) return;

        // Remove all connections that involve this node
        graph.connections.RemoveAll(conn =>
            conn.fromNode == this || conn.toNode == this);

        Debug.Log($"Removed all connections for node {name}");
    }

    public void AutoConnectToNearbyNodes()
    {
        if (manualModeOnly) return;

        NavigationGraph graph = GetComponentInParent<NavigationGraph>();
        if (graph == null) return;

        // Find all other nodes in the graph
        NavigationNode[] allNodes = graph.GetComponentsInChildren<NavigationNode>();

        foreach (NavigationNode otherNode in allNodes)
        {
            if (otherNode == this) continue;

            float distance = Vector3.Distance(transform.position, otherNode.transform.position);

            // Check if within connection radius
            if (distance <= connectionRadius || true)
            {
                // Check for obstacles between nodes
                if (!Physics.Linecast(transform.position, otherNode.transform.position, obstacleMask))
                {
                    graph.AddConnection(this, otherNode);
                }
                else
                {
                    Debug.Log("Blocked");
                }
            }
        }
    }

    private void OnNodeMoved()
    {
        if (manualModeOnly) return;

        Debug.Log($"Node {name} moved to {transform.position}");

        // Clean up old connections
        CleanUpConnections();

        // Re-establish new connections
        if (autoConnectOnCreate)
        {
            AutoConnectToNearbyNodes();
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the node
        Gizmos.color = new Color(nodeColor.r, nodeColor.g, nodeColor.b, 0.7f);
        Gizmos.DrawSphere(transform.position, nodeSize);

        Gizmos.color = nodeColor;
        Gizmos.DrawWireSphere(transform.position, nodeSize);
    }

    // Public method to manually connect nodes
    public void ManualConnectTo(NavigationNode otherNode)
    {
        NavigationGraph graph = GetComponentInParent<NavigationGraph>();
        if (graph == null) return;

        graph.AddConnection(this, otherNode);
    }
}