using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

[RequireComponent(typeof(COMP476HiderMovement))]
public class GhostBehaviorTree : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Pathfinder _pathfinder;
    [SerializeField] private LayerMask _obstructionMask;

    [Header("Behavior Settings")]
    [SerializeField] private float _sightRange = 10f;
    [SerializeField] private float _waypointThreshold = 0.5f;

    private BehaviorTree _bt;
    private COMP476HiderMovement _movement;
    private Transform _player;
    private List<GhostBehaviorTree> _allGhosts = new List<GhostBehaviorTree>();

    private void Awake()
    {
        _movement = GetComponent<COMP476HiderMovement>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
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

        // Initialize behavior tree with your root node
        _bt = new BehaviorTree(blackboard, CreateRootNode(blackboard));
    }

    private IBTNode CreateRootNode(BTBlackboard bt)
    {

        return new BTRepeat(new BTSequence(
            new ChooseRandomNode(bt, false),
            new MoveToNode(bt)
        ));
    }

    private void Update()
    {
        // Update blackboard data
        _bt.Blackboard.Set("PlayerPosition", _player.position);
        _bt.Blackboard.Set("SelfPosition", transform.position);

        // Run behavior tree
        _bt.Update();
    }
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