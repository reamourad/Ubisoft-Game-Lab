using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class GhostAI : MonoBehaviour
{
    public enum NavigationMode { Sequential, Dynamic }

    [Header("Navigation Settings")]
    [SerializeField] private Pathfinder _pathfinder;
    [SerializeField] private NavigationMode _navigationMode = NavigationMode.Sequential;
    [SerializeField] private float _waypointThreshold = 0.5f;
    [SerializeField] private float _repathInterval = 1f;
    [SerializeField] private float _accessCheckRadius = 3f;

    [Header("Movement")]
    [SerializeField] private COMP476HiderMovement _movementController;
    [SerializeField] private float _movementPrediction = 0.5f;

    private NavigationGraph _graph;
    private List<NavigationNode> _currentPath = new List<NavigationNode>();
    private int _currentWaypointIndex;
    private float _lastRepathTime;
    private NavigationNode _currentDestination;

    private void Start()
    {
        InitializeComponents();
        SetRandomDestination();
    }

    private void InitializeComponents()
    {
        _pathfinder ??= FindFirstObjectByType<Pathfinder>();
        _movementController ??= GetComponent<COMP476HiderMovement>();
        _graph = _pathfinder.graph;
    }

    private void Update()
    {
        if (_currentPath.Count == 0) return;

        switch (_navigationMode)
        {
            case NavigationMode.Sequential:
                FollowPathSequentially();
                break;
            case NavigationMode.Dynamic:
                FollowPathDynamically();
                break;
        }
    }

    #region Navigation Behaviors
    private void FollowPathSequentially()
    {
        Vector3 currentWaypoint = _currentPath[_currentWaypointIndex].transform.position;
        MoveToward(currentWaypoint);

        if (Vector3.Distance(transform.position, currentWaypoint) < _waypointThreshold)
        {
            _currentWaypointIndex++;
            if (_currentWaypointIndex >= _currentPath.Count)
            {
                SetRandomDestination();
            }
        }
    }

    private void FollowPathDynamically()
    {
        // Check for better access points periodically
        if (Time.time - _lastRepathTime > _repathInterval)
        {
            _lastRepathTime = Time.time;
            CheckForBetterAccessPoint();
        }

        // Move toward current waypoint
        Vector3 currentWaypoint = _currentPath[_currentWaypointIndex].transform.position;
        MoveToward(currentWaypoint);

        // Progress to next waypoint if reached
        if (Vector3.Distance(transform.position, currentWaypoint) < _waypointThreshold)
        {
            _currentWaypointIndex++;
            if (_currentWaypointIndex >= _currentPath.Count)
            {
                SetRandomDestination();
            }
        }
    }

    private void CheckForBetterAccessPoint()
    {
        // Find all accessible nodes in radius
        var nearbyNodes = _graph.GetComponentsInChildren<NavigationNode>();
        foreach (var node in nearbyNodes)
        {
            if (Vector3.Distance(transform.position, node.transform.position) <= _accessCheckRadius &&
                !Physics.Linecast(transform.position, node.transform.position, _pathfinder.obstructionMask))
            {
                // Check if this node provides a better path to destination
                var newPath = _pathfinder.FindShortestPath(node, _currentDestination);
                if (newPath != null && newPath.Count < _currentPath.Count)
                {
                    _currentPath = newPath;
                    _currentWaypointIndex = 0;
                    break;
                }
            }
        }
    }
    #endregion

    #region Movement
    private void MoveToward(Vector3 targetPosition)
    {
        Vector3 predictedPosition = transform.position + _movementController.GetCurrentVelocity() * _movementPrediction;
        Vector3 localTarget = transform.InverseTransformPoint(targetPosition);

        _movementController.SetVerticalInput(Mathf.Clamp(localTarget.z, -1f, 1f));
        _movementController.SetHorizontalInput(Mathf.Clamp(localTarget.x, -1f, 1f));

        // Handle vertical movement if needed
        float verticalDiff = targetPosition.y - predictedPosition.y;
        _movementController.SetAscendInput(Mathf.Clamp(verticalDiff, -1f, 1f));
    }
    #endregion

    #region Destination Management
    public void SetDestination(NavigationNode destination)
    {
        if (_graph == null || destination == null) return;

        _currentDestination = destination;
        NavigationNode startNode = _pathfinder.FindOptimalAccessPoint(transform, destination);

        if (startNode != null)
        {
            _currentPath = _pathfinder.FindShortestPath(startNode, destination);
            _currentWaypointIndex = 0;
        }
    }

    public void SetRandomDestination()
    {
        if (_graph == null || _graph.connections.Count == 0) return;

        // Get a random connected node
        int randomIndex = Random.Range(0, _graph.connections.Count);
        var randomConnection = _graph.connections[randomIndex];
        var randomNode = Random.value > 0.5f ? randomConnection.fromNode : randomConnection.toNode;

        SetDestination(randomNode);
    }
    #endregion

    #region Editor Tools
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_currentPath == null || _currentPath.Count == 0) return;

        // Draw path
        Handles.color = Color.magenta;
        for (int i = 0; i < _currentPath.Count - 1; i++)
        {
            Handles.DrawLine(_currentPath[i].transform.position, _currentPath[i + 1].transform.position);
        }

        // Draw current waypoint
        if (_currentWaypointIndex < _currentPath.Count)
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(_currentPath[_currentWaypointIndex].transform.position, Vector3.up, _waypointThreshold);
            Handles.DrawLine(transform.position, _currentPath[_currentWaypointIndex].transform.position);
        }

        // Draw dynamic check radius
        if (_navigationMode == NavigationMode.Dynamic)
        {
            Handles.color = new Color(0, 1, 1, 0.1f);
            Handles.DrawSolidDisc(transform.position, Vector3.up, _accessCheckRadius);
        }
    }

    [CustomEditor(typeof(GhostAI))]
    public class GhostAIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GhostAI ghostAI = (GhostAI)target;

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Navigation Controls", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Random Destination"))
            {
                Undo.RecordObject(ghostAI, "Set Random Destination");
                ghostAI.SetRandomDestination();
                EditorUtility.SetDirty(ghostAI);
            }

            if (GUILayout.Button("Switch Navigation Mode"))
            {
                Undo.RecordObject(ghostAI, "Switch Navigation Mode");
                ghostAI._navigationMode = ghostAI._navigationMode == GhostAI.NavigationMode.Sequential
                    ? GhostAI.NavigationMode.Dynamic
                    : GhostAI.NavigationMode.Sequential;
                EditorUtility.SetDirty(ghostAI);
            }
            GUILayout.EndHorizontal();
        }
    }
#endif
    #endregion
}