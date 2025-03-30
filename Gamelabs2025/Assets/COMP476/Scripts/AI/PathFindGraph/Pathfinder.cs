using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class Pathfinder : MonoBehaviour
{
    [Header("References")]
    public NavigationGraph graph;
    public NavigationNode startNode;
    public NavigationNode endNode;

    public LayerMask obstructionMask;
    public Transform testTransform;

    [Header("Visual Settings")]
    public Color pathColor = Color.green;
    public Color accessibleColor = Color.blue;
    public Color blockedColor = Color.red;
    public float pathNodeSizeMultiplier = 1.5f;
    public bool drawPathGizmo = true;

    private List<NavigationNode> accessibleNodes = new List<NavigationNode>();
    private NavigationNode optimalAccessPoint;
    private List<NavigationNode> currentPath = new List<NavigationNode>();

    /// <summary>
    /// Finds all accessible nodes from transform, then returns the one with shortest path to destination
    /// </summary>
    public NavigationNode FindOptimalAccessPoint(Transform transform, NavigationNode destination)
    {
        if (graph == null || transform == null || destination == null)
        {
            Debug.LogWarning("Missing references in FindOptimalAccessPoint");
            return null;
        }

        accessibleNodes.Clear();
        optimalAccessPoint = null;
        float shortestPathLength = Mathf.Infinity;

        // Get all accessible nodes
        foreach (var node in graph.GetComponentsInChildren<NavigationNode>())
        {
            if (!Physics.Linecast(transform.position, node.transform.position, obstructionMask))
            {
                accessibleNodes.Add(node);
            }
        }

        // Find the accessible node with shortest path to destination
        foreach (var node in accessibleNodes)
        {
            var path = FindShortestPath(node, destination);
            if (path != null)
            {
                float pathLength = CalculatePathLength(path);
                if (pathLength < shortestPathLength)
                {
                    shortestPathLength = pathLength;
                    optimalAccessPoint = node;
                    currentPath = path;
                }
            }
        }

        HighlightAccessibility();
        return optimalAccessPoint;
    }

    private float CalculatePathLength(List<NavigationNode> path)
    {
        float length = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            length += Vector3.Distance(path[i].transform.position, path[i + 1].transform.position);
        }
        return length;
    }

    private void HighlightAccessibility()
    {
        // Reset all nodes first
        foreach (var node in graph.GetComponentsInChildren<NavigationNode>())
        {
            node.nodeColor = Color.white;
#if UNITY_EDITOR
            EditorUtility.SetDirty(node);
#endif
        }

        // Highlight accessible nodes
        foreach (var node in accessibleNodes)
        {
            node.nodeColor = accessibleColor;
#if UNITY_EDITOR
            EditorUtility.SetDirty(node);
#endif
        }

        // Highlight optimal access point
        if (optimalAccessPoint != null)
        {
            optimalAccessPoint.nodeColor = Color.yellow;
#if UNITY_EDITOR
            EditorUtility.SetDirty(optimalAccessPoint);
#endif
        }
    }

    public void FindAndShowPath()
    {
        if (graph == null || startNode == null || endNode == null)
        {
            Debug.LogWarning("Pathfinder missing references");
            return;
        }

        currentPath = FindShortestPath(startNode, endNode);
        HighlightPath(currentPath);
    }

    public List<NavigationNode> FindShortestPath(NavigationNode start, NavigationNode end)
    {
        // A* Pathfinding implementation
        var openSet = new List<NavigationNode> { start };
        var closedSet = new HashSet<NavigationNode>();
        var cameFrom = new Dictionary<NavigationNode, NavigationNode>();
        var gScore = new Dictionary<NavigationNode, float>();
        var fScore = new Dictionary<NavigationNode, float>();

        // Initialize scores
        foreach (var node in graph.GetComponentsInChildren<NavigationNode>())
        {
            gScore[node] = Mathf.Infinity;
            fScore[node] = Mathf.Infinity;
        }

        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);

        while (openSet.Count > 0)
        {
            var current = GetLowestFScoreNode(openSet, fScore);

            if (current == end)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var connection in graph.connections)
            {
                if (connection.fromNode == current)
                {
                    var neighbor = connection.toNode;
                    ProcessNeighbor(current, neighbor, gScore, fScore, cameFrom, openSet, closedSet, end);
                }
                else if (connection.toNode == current)
                {
                    var neighbor = connection.fromNode;
                    ProcessNeighbor(current, neighbor, gScore, fScore, cameFrom, openSet, closedSet, end);
                }
            }
        }

        return null; // No path found
    }

    private void ProcessNeighbor(NavigationNode current, NavigationNode neighbor,
        Dictionary<NavigationNode, float> gScore, Dictionary<NavigationNode, float> fScore,
        Dictionary<NavigationNode, NavigationNode> cameFrom, List<NavigationNode> openSet,
        HashSet<NavigationNode> closedSet, NavigationNode end)
    {
        if (closedSet.Contains(neighbor)) return;

        float tentativeGScore = gScore[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);

        if (!openSet.Contains(neighbor))
        {
            openSet.Add(neighbor);
        }
        else if (tentativeGScore >= gScore[neighbor])
        {
            return;
        }

        cameFrom[neighbor] = current;
        gScore[neighbor] = tentativeGScore;
        fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, end);
    }

    private NavigationNode GetLowestFScoreNode(List<NavigationNode> nodes, Dictionary<NavigationNode, float> fScore)
    {
        NavigationNode lowestNode = nodes[0];
        float lowestScore = fScore[lowestNode];

        foreach (var node in nodes)
        {
            if (fScore[node] < lowestScore)
            {
                lowestScore = fScore[node];
                lowestNode = node;
            }
        }

        return lowestNode;
    }

    private float Heuristic(NavigationNode a, NavigationNode b)
    {
        return Vector3.Distance(a.transform.position, b.transform.position);
    }

    private List<NavigationNode> ReconstructPath(Dictionary<NavigationNode, NavigationNode> cameFrom, NavigationNode current)
    {
        var path = new List<NavigationNode> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }

    private void HighlightPath(List<NavigationNode> path)
    {
        // Reset all nodes first
        foreach (var node in graph.GetComponentsInChildren<NavigationNode>())
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(node);
#endif
            node.nodeColor = Color.cyan; // Default color
        }

        // Color the path nodes
        if (path != null)
        {
            foreach (var node in path)
            {
                if (node != null)
                {
                    node.nodeColor = pathColor;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(node);
#endif
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawPathGizmo) return;

        // Draw accessibility lines
        if (testTransform != null)
        {
            foreach (var node in graph.GetComponentsInChildren<NavigationNode>())
            {
                bool isAccessible = accessibleNodes.Contains(node);
                bool isOptimal = (node == optimalAccessPoint);

                Color color = isOptimal ? Color.yellow :
                             isAccessible ? accessibleColor : blockedColor;
                float width = isOptimal ? 4f : isAccessible ? 2f : 1f;

                Handles.color = color;
                Handles.DrawAAPolyLine(width, testTransform.position, node.transform.position);
            }
        }

        // Draw path if available
        if (currentPath != null && currentPath.Count > 1)
        {
            Handles.color = pathColor;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                if (currentPath[i] != null && currentPath[i + 1] != null)
                {
                    Handles.DrawAAPolyLine(4f,
                        currentPath[i].transform.position,
                        currentPath[i + 1].transform.position);
                }
            }
        }
    }

    [CustomEditor(typeof(Pathfinder))]
    public class PathfinderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Pathfinder pathfinder = (Pathfinder)target;

            GUILayout.Space(10);
            if (GUILayout.Button("Find Optimal Path", GUILayout.Height(30)))
            {
                if (pathfinder.testTransform != null && pathfinder.endNode != null)
                {
                    Undo.RecordObject(pathfinder, "Find Optimal Path");
                    pathfinder.FindOptimalAccessPoint(pathfinder.testTransform, pathfinder.endNode);
                    EditorUtility.SetDirty(pathfinder);
                }
            }

            if (GUILayout.Button("Clear All", GUILayout.Height(30)))
            {
                Undo.RecordObject(pathfinder, "Clear All");
                pathfinder.accessibleNodes.Clear();
                pathfinder.currentPath.Clear();
                pathfinder.HighlightAccessibility();
                EditorUtility.SetDirty(pathfinder);
            }
        }
    }
#endif
}