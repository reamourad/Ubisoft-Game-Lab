using System.Collections.Generic;
using UnityEngine;
using System.Linq;


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

    public Color lastPathColor = new Color(1f, 0.5f, 0f); // Orange color
    private List<NavigationNode> _lastPath = new List<NavigationNode>();

    private NavigationNode optimalAccessPoint;
    private List<NavigationNode> currentPath = new List<NavigationNode>();

    private Dictionary<Transform, GhostPathCache> _ghostPathCache = new();

    public NavigationNode FindOptimalAccessPoint(Transform ghost, NavigationNode destination,
                                             Transform avoidTransform = null, float avoidRadius = 5f)
    {
        if (graph == null || ghost == null || destination == null)
        {
            Debug.LogWarning("Missing references in FindOptimalAccessPoint");
            return null;
        }

        if (!_ghostPathCache.TryGetValue(ghost, out var cache))
        {
            cache = new GhostPathCache();
            _ghostPathCache[ghost] = cache;
        }

        bool sameQuery = cache.lastDestination == destination &&
                         cache.lastAvoidTransform == avoidTransform &&
                         cache.path != null && cache.path.Count > 0;

        if (sameQuery)
        {
            for (int i = cache.path.Count - 1; i >= 0; i--)
            {
                var node = cache.path[i];
                if (!Physics.Linecast(ghost.position, node.transform.position, obstructionMask))
                {
                    cache.accessPoint = node;
                    return node;
                }
            }
        }

        // Update cached query
        cache.lastDestination = destination;
        cache.lastAvoidTransform = avoidTransform;

        // Step 1: Pre-filter accessible nodes
        var accessibleNodes = new List<NavigationNode>();

        foreach (var node in graph.GetComponentsInChildren<NavigationNode>())
        {
            if (!Physics.Linecast(ghost.position, node.transform.position, obstructionMask) &&
                !IsNodeApproachingDanger(ghost, node, avoidTransform))
            {
                accessibleNodes.Add(node);
            }
        }

        if (accessibleNodes.Count == 0)
        {
            Debug.LogWarning("No accessible nodes found");
            return null;
        }

        // Step 2: Rank nodes by estimated value
        var ranked = accessibleNodes
            .OrderBy(n =>
                Vector3.Distance(n.transform.position, destination.transform.position) + // closeness to goal
                Vector3.Distance(n.transform.position, ghost.position) * 0.5f             // closer to ghost is good
            )
            .Take(2) // Only try the best one or two
            .ToList();

        foreach (var candidate in ranked)
        {
            Debug.Log($"[A* RUN] Trying path from {candidate.name}");
            var path = FindShortestPath(candidate, destination, avoidTransform, avoidRadius);

            if (path != null && path.Count > 0)
            {
                for (int i = path.Count - 1; i >= 0; i--)
                {
                    var node = path[i];
                    if (!Physics.Linecast(ghost.position, node.transform.position, obstructionMask))
                    {
                        cache.path = path;
                        cache.accessPoint = node;

                        _lastPath = new List<NavigationNode>(path);
                        HighlightLastPath();
                        HighlightAccessibility();

                        return node;
                    }
                }
            }
        }

        Debug.LogWarning("[A* GAVE UP] Could not find a viable path (0–2 attempts max)");
        return null;
    }


    public NavigationNode FindRandomWanderTarget(Transform ghost, int maxSteps = 10, Transform avoidTransform = null, float avoidRadius = 14f)
    {
        if (ghost == null || graph == null)
            return null;

        if (!_ghostPathCache.TryGetValue(ghost, out var cache))
        {
            cache = new GhostPathCache();
            _ghostPathCache[ghost] = cache;
        }

        // ✅ Step 1: Start from visible node
        NavigationNode startNode = null;
        float minDist = Mathf.Infinity;

        foreach (var node in graph.GetComponentsInChildren<NavigationNode>())
        {
            if (!Physics.Linecast(ghost.position, node.transform.position, obstructionMask) &&
                !IsNodeApproachingDanger(ghost, node, avoidTransform))
            {
                float dist = Vector3.Distance(ghost.position, node.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    startNode = node;
                }
            }
        }

        if (startNode == null)
        {
            Debug.LogWarning("No safe start node found for wandering");
            return null;
        }

        // ✅ Step 2: Random walk avoiding danger
        var path = new List<NavigationNode> { startNode };
        var visited = new HashSet<NavigationNode> { startNode };
        var current = startNode;

        for (int i = 0; i < maxSteps; i++)
        {
            var neighbors = GetConnectedNodes(current)
                .Where(n => !visited.Contains(n) && !IsNodeApproachingDanger(ghost, n, avoidTransform))
                .ToList();

            if (neighbors.Count == 0) break;

            var next = neighbors[Random.Range(0, neighbors.Count)];
            path.Add(next);
            visited.Add(next);
            current = next;
        }

        if (path.Count < 2)
        {
            Debug.LogWarning("Random wander path was too short or too close to danger");
            return null;
        }

        // ✅ Step 3: Cache result
        cache.path = path;
        cache.lastDestination = path[^1]; // final node in path
        cache.accessPoint = path[1]; // first movement step

        _lastPath = new List<NavigationNode>(path);
        HighlightLastPath();
        HighlightAccessibility();

        return cache.lastDestination;
    }

    private List<NavigationNode> GetConnectedNodes(NavigationNode node)
    {
        var neighbors = new List<NavigationNode>();

        foreach (var connection in graph.connections)
        {
            if (connection.fromNode == node && connection.toNode != null)
            {
                neighbors.Add(connection.toNode);
            }
            else if (connection.toNode == node && connection.fromNode != null)
            {
                neighbors.Add(connection.fromNode);
            }
        }

        return neighbors;
    }


    public List<NavigationNode> FindShortestPath(NavigationNode start, NavigationNode end, Transform avoidTransform = null, float avoidRadius = 5f)
    {
        // A* Pathfinding with avoidance
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
                NavigationNode neighbor = null;
                if (connection.fromNode == current)
                {
                    neighbor = connection.toNode;
                }
                else if (connection.toNode == current)
                {
                    neighbor = connection.fromNode;
                }

                if (neighbor != null &&
                    !IsNodeApproachingDanger(current.transform, neighbor, avoidTransform))
                {
                    ProcessNeighbor(current, neighbor, gScore, fScore, cameFrom, openSet, closedSet, end);
                }
            }
        }

        // Fallback - try without avoidance if no path found
        if (avoidTransform != null)
        {
            Debug.LogWarning("Couldn't find safe path - attempting without avoidance");
            return FindShortestPath(start, end);
        }

        return null;
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

        /*// Highlight accessible nodes
        foreach (var node in accessibleNodes)
        {
            node.nodeColor = accessibleColor;
#if UNITY_EDITOR
            EditorUtility.SetDirty(node);
#endif
        }*/

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

    private bool IsNodeApproachingDanger(Transform ghost, NavigationNode node, Transform dangerTransform)
    {
        if (dangerTransform == null || ghost == null || node == null)
            return false;

        Vector3 ghostPos = ghost.position;
        Vector3 nodePos = node.transform.position;
        Vector3 playerPos = dangerTransform.position;

        // ✅ Condition 1: Is node closer to player than ghost is?
        float ghostToPlayer = Vector3.Distance(ghostPos, playerPos);
        float nodeToPlayer = Vector3.Distance(nodePos, playerPos);
        bool gettingCloser = nodeToPlayer < ghostToPlayer;

        if (!gettingCloser) return false;

        // ✅ Condition 2: Is the line from ghost to node close to the player?
        float lineToPlayerDistance = DistanceFromPointToLine(playerPos, ghostPos, nodePos);

        const float proximityThreshold = 3.5f; // tweakable danger radius along path

        return lineToPlayerDistance < proximityThreshold;
    }

    private float DistanceFromPointToLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDir = lineEnd - lineStart;
        Vector3 pointDir = point - lineStart;

        float lineLength = lineDir.magnitude;
        if (lineLength == 0f) return Vector3.Distance(point, lineStart);

        float t = Mathf.Clamp01(Vector3.Dot(pointDir, lineDir.normalized) / lineLength);
        Vector3 closestPoint = lineStart + lineDir * t;

        return Vector3.Distance(point, closestPoint);
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
        Vector3 diff = b.transform.position - a.transform.position;

        float horizontalDistance = new Vector2(diff.x, diff.z).magnitude;
        float verticalDifference = Mathf.Abs(diff.y);

        float verticalPenalty = verticalDifference > 3f ? verticalDifference * 10f : 0f;

        return horizontalDistance + verticalPenalty;
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

    public float GetSafeDistance(Transform userTransform, NavigationNode node)
    {
        if (graph == null || userTransform == null || node == null) return Mathf.Infinity;

        // Find the closest accessible node to the user
        var accessPoint = FindOptimalAccessPoint(userTransform, node);

        if (accessPoint == null) return Mathf.Infinity;

        // Calculate path length from access point to target node
        var path = FindShortestPath(accessPoint, node);
        float pathLength = path != null ? CalculatePathLength(path) : Mathf.Infinity;

        // Calculate direct distance from user to access point
        float accessDistance = Vector3.Distance(userTransform.position, accessPoint.transform.position);

        // Return the combined distance
        return pathLength + accessDistance;
    }

    private void HighlightLastPath()
    {
        if (_lastPath != null && _lastPath.Count > 0)
        {
            foreach (var node in _lastPath)
            {
                if (node != null)
                {
                    node.nodeColor = lastPathColor;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(node);
#endif
                }
            }
        }
    }

    private class GhostPathCache
    {
        public NavigationNode lastDestination;
        public Transform lastAvoidTransform;
        public List<NavigationNode> path;
        public NavigationNode accessPoint;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawPathGizmo) return;

        /*// Draw accessibility lines
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
        }*/

        // Draw path if available
        /*if (currentPath != null && currentPath.Count > 1)
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
        }*/

        // Draw last path in a different color
        if (_lastPath != null && _lastPath.Count > 1)
        {
            Handles.color = lastPathColor;
            for (int i = 0; i < _lastPath.Count - 1; i++)
            {
                if (_lastPath[i] != null && _lastPath[i + 1] != null)
                {
                    Handles.DrawAAPolyLine(2f, // Thinner line for last path
                        _lastPath[i].transform.position,
                        _lastPath[i + 1].transform.position);
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
                //pathfinder.accessibleNodes.Clear();
                pathfinder.currentPath.Clear();
                pathfinder.HighlightAccessibility();
                EditorUtility.SetDirty(pathfinder);
            }
        }
    }
#endif
}

