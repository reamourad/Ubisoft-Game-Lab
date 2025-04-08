using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;

[RequireComponent(typeof(COMP476HiderMovement))]
public class GhostBehaviorTree : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Pathfinder _pathfinder;
    [SerializeField] private LayerMask _obstructionMask;
    [SerializeField] private Collider _collider;

    [Header("Behavior Settings")]
    [SerializeField] private float _sightRange = 10f;
    [SerializeField] private float _waypointThreshold = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool _enableDebug = true;
    [SerializeField] private Color _debugColor = Color.cyan;

    private BehaviorTree _bt;
    private COMP476HiderMovement _movement;
    private Transform _player;
    private List<GhostBehaviorTree> _allGhosts = new List<GhostBehaviorTree>();

    private void Awake()
    {
        _movement = GetComponent<COMP476HiderMovement>();
        _player = FindFirstObjectByType<COMP476CharacterController>().transform;
        _allGhosts = new List<GhostBehaviorTree>(FindObjectsByType<GhostBehaviorTree>(FindObjectsSortMode.None));

        if(_pathfinder == null)
        {
            _pathfinder = FindFirstObjectByType<Pathfinder>();
        }

        InitializeBehaviorTree();
    }

    private void InitializeBehaviorTree()
    {
        // Create blackboard and populate initial data
        var blackboard = new BTBlackboard();
        blackboard.Set("Self", transform);
        blackboard.Set("Movement", _movement);
        blackboard.Set("Player", _player);
        blackboard.Set("AllGhosts", _allGhosts);
        blackboard.Set("Pathfinder", _pathfinder);
        blackboard.Set("ObstructionMask", _obstructionMask);
        blackboard.Set("SightRange", _sightRange);
        blackboard.Set("WaypointThreshold", _waypointThreshold);
        blackboard.Set("Collider", _collider);

        // Initialize behavior tree with your root node
        _bt = new BehaviorTree(blackboard, CreateRootNode(blackboard));
    }

    private IBTNode CreateRootNode(BTBlackboard bt)
    {
        return new BTRepeat(
            new BTSelector(
                new BTInverter(
                    new BTSelector(
                        new ChooseRandomNode(bt, true),
                        new PanicRunNode(bt)
                    )
                ),
                new BTSequence(
                    new ChooseRandomNode(bt, true),
                    new MonitoredActionNode(
                        new MoveToNode(bt, true),
                        new PlayerDetectionNode(bt)
                    )
                ),
                new BTSequence(
                    new ChooseRandomNode(bt, false),
                    new MonitoredActionNode(
                        new MoveToNode(bt),
                        new BTConditionInverter(bt, new PlayerDetectionNode(bt))
                    )
                )
            )
        );
    }

    private void Update()
    {
        // Update blackboard data
        _bt.Blackboard.Set("PlayerPosition", _player.position);
        _bt.Blackboard.Set("SelfPosition", transform.position);

        // Run behavior tree
        _bt.Update();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_enableDebug) return;

        // Draw debug sphere
        Gizmos.color = _bt?.Blackboard.Get<Color>("DebugColor") ?? _debugColor;
        Gizmos.DrawSphere(transform.position, 0.5f);

        // Draw line to current target if available
        if (_bt?.Blackboard.Has("TargetNode") ?? false)
        {
            var targetNode = _bt.Blackboard.Get<NavigationNode>("TargetNode");
            if (targetNode != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, targetNode.transform.position);
            }
        }

        // Draw sight range
        //Handles.color = new Color(0, 1, 1, 0.1f);
        //Handles.DrawSolidDisc(transform.position, Vector3.up, _sightRange);
    }
#endif
}

// Modified BehaviorTree class to manage execution
public class BehaviorTree
{
    public BTBlackboard Blackboard { get; }
    private IBTNode _root;

    public BehaviorTree(BTBlackboard blackboard, IBTNode rootNode)
    {
        Blackboard = blackboard;
        _root = rootNode;
    }

    public void Update()
    {
        _root?.Update();
    }
}