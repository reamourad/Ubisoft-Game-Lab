using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NavigationGraph : MonoBehaviour
{
    [System.Serializable]
    public class Connection
    {
        public NavigationNode fromNode;
        public NavigationNode toNode;
        [Tooltip("Connection visual color")]
        public Color connectionColor = Color.white;
        [Range(0.1f, 5f)] public float lineThickness = 1f;
    }

    [Header("Node Connections")]
    public List<Connection> connections = new List<Connection>();

    [Header("Gizmo Settings")]
    public bool alwaysVisible = true;
    public bool drawNodeLabels = true;
    public bool drawConnectionWeights = true;

    [Header("Transform Accessibility Settings")]
    public LayerMask obstructionMask;
    public Transform testTransform; // For editor debugging
    public Color accessibleColor = Color.green;
    public Color blockedColor = Color.red;

    public void ClearAllConnections()
    {
        connections.Clear();
        Debug.Log("Cleared all connections in the graph");
    }

    public void ReconnectAllNodes()
    {
        ClearAllConnections();

        NavigationNode[] allNodes = GetComponentsInChildren<NavigationNode>();
        if (allNodes == null || allNodes.Length == 0) return;

        foreach (NavigationNode node in allNodes)
        {
            if (node != null)
            {
                node.AutoConnectToNearbyNodes();
            }
        }

        Debug.Log($"Reconnected all {allNodes.Length} nodes in the graph");
    }

    private void OnDrawGizmos()
    {
        if (alwaysVisible) DrawConnections();
    }

    private void OnDrawGizmosSelected()
    {
        if (!alwaysVisible) DrawConnections();

        if (testTransform != null) {

            // Draw lines to all nodes with accessibility check
            foreach (var node in GetComponentsInChildren<NavigationNode>())
            {
                bool isAccessible = !Physics.Linecast(testTransform.position, node.transform.position, obstructionMask);
                if (!isAccessible) continue;
                Color lineColor = isAccessible ? accessibleColor : blockedColor;
                float lineWidth = isAccessible ? 2f : 1f;

#if UNITY_EDITOR
                Handles.color = lineColor;
                Handles.DrawAAPolyLine(lineWidth, testTransform.position, node.transform.position);
#endif
                // Draw small indicator at node position
                Gizmos.color = isAccessible ? accessibleColor : blockedColor;
                Gizmos.DrawSphere(node.transform.position, 0.2f);
            }
        }
    }

    private void DrawConnections()
    {
        if (connections == null) return;

        // First pass: Draw all connection lines
        foreach (var connection in connections)
        {
            if (connection.fromNode != null && connection.toNode != null)
            {
                Vector3 fromPos = connection.fromNode.transform.position;
                Vector3 toPos = connection.toNode.transform.position;

                // Thicker line in the background
                Gizmos.color = new Color(0, 0, 0, 1f);
                Gizmos.DrawLine(fromPos, toPos);

                // Primary colored line
                Gizmos.color = connection.connectionColor;
#if UNITY_EDITOR



                UnityEditor.Handles.color = connection.connectionColor;
                UnityEditor.Handles.DrawAAPolyLine(connection.lineThickness, fromPos, toPos);
#else
                Gizmos.DrawLine(fromPos, toPos);
#endif

                // Draw direction arrow
                DrawDirectionArrow(fromPos, toPos, connection.connectionColor);
            }
        }

        // Second pass: Draw weights and labels (on top of lines)
        foreach (var connection in connections)
        {
            if (connection.fromNode != null && connection.toNode != null)
            {
                Vector3 midPoint = Vector3.Lerp(
                    connection.fromNode.transform.position,
                    connection.toNode.transform.position,
                    0.5f);

                // Draw connection weight
                if (drawConnectionWeights)
                {
#if UNITY_EDITOR
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;
                    style.fontStyle = FontStyle.Bold;
                    UnityEditor.Handles.Label(
                        midPoint + Vector3.up * 0.2f,
                        Vector3.Distance(
                            connection.fromNode.transform.position,
                            connection.toNode.transform.position
                        ).ToString("F1"),
                        style);
#endif
                }
            }
        }

        // Draw node labels
        if (drawNodeLabels)
        {
            foreach (var node in GetComponentsInChildren<NavigationNode>())
            {
#if UNITY_EDITOR
                GUIStyle style = new GUIStyle();
                style.normal.textColor = node.nodeColor;
                style.fontStyle = FontStyle.Bold;
                UnityEditor.Handles.Label(
                    node.transform.position + Vector3.up * 0.3f,
                    node.gameObject.name,
                    style);
#endif
            }
        }
    }

    private void DrawDirectionArrow(Vector3 from, Vector3 to, Color color)
    {
        Vector3 direction = (to - from).normalized;
        float arrowSize = 0.5f;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 160, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 200, 0) * Vector3.forward;

        Gizmos.color = color;
        Gizmos.DrawRay(to, right * arrowSize);
        Gizmos.DrawRay(to, left * arrowSize);
    }

    public void AddConnection(NavigationNode from, NavigationNode to)
    {
        if (from == null || to == null || from == to) return;

        // Check if connection already exists
        foreach (var conn in connections)
        {
            if ((conn.fromNode == from && conn.toNode == to) ||
                (conn.fromNode == to && conn.toNode == from))
            {
                //Debug.LogWarning("Connection already exists between these nodes");
                return;
            }
        }

        connections.Add(new Connection
        {
            fromNode = from,
            toNode = to,
            connectionColor = Color.Lerp(from.nodeColor, to.nodeColor, 0.5f)
        });
        //Debug.Log($"Added connection between {from.name} and {to.name}");
    }

    public List<NavigationNode> GetAccessibleNodes(Transform transform)
    {
        List<NavigationNode> accessibleNodes = new List<NavigationNode>();
        if (transform == null) return accessibleNodes;

        foreach (var node in GetComponentsInChildren<NavigationNode>())
        {
            if (!Physics.Linecast(transform.position, node.transform.position, obstructionMask))
            {
                accessibleNodes.Add(node);
            }
        }

        return accessibleNodes;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NavigationGraph))]
public class NavigationGraphEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NavigationGraph graph = (NavigationGraph)target;

        GUILayout.Space(15);
        EditorGUILayout.LabelField("Graph Actions", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All Connections", GUILayout.Height(30)))
        {
            Undo.RecordObject(graph, "Clear All Connections");
            graph.ClearAllConnections();
            EditorUtility.SetDirty(graph);
        }

        if (GUILayout.Button("Reconnect All Nodes", GUILayout.Height(30)))
        {
            Undo.RecordObject(graph, "Reconnect All Nodes");
            graph.ReconnectAllNodes();
            EditorUtility.SetDirty(graph);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (GUILayout.Button("Test Transform Accessibility", GUILayout.Height(30)))
        {
            if (graph.testTransform != null)
            {
                Undo.RecordObject(graph, "Test Transform Accessibility");
                EditorUtility.SetDirty(graph);
                SceneView.RepaintAll(); // Refresh the scene view
            }
            else
            {
                Debug.LogWarning("Assign a test transform first!");
            }
        }
    }
}
#endif