using System.Collections.Generic;
using UnityEngine;

public class ChooseRandomNode : BTAction
{
    bool avoidPlayer;

    public ChooseRandomNode(BTBlackboard blackboard, bool avoidPlayer) : base(blackboard)
    {
        this.avoidPlayer = avoidPlayer;
    }

    public override BTStatus Update()
    {
        // Get all valid nodes
        var pathfinder = bb.Get<Pathfinder>("Pathfinder");
        var allNodes = pathfinder.graph.GetComponentsInChildren<NavigationNode>();

        // Select random node
        var randomNode = allNodes[Random.Range(0, allNodes.Length)];
        bb.Set("TargetNode", randomNode);

        return BTStatus.Success;
    }
}

public class MoveToNode : BTAction
{
    private readonly float _waypointThreshold;
    private readonly float _repathDistance;
    private NavigationNode _currentAccessPoint;

    public MoveToNode(BTBlackboard bb, float waypointThreshold = 0.5f, float repathDistance = 2f) : base(bb)
    {
        _waypointThreshold = waypointThreshold;
        _repathDistance = repathDistance;
    }

    public override BTStatus Update()
    {
        // Get references
        var targetNode = bb.Get<NavigationNode>("TargetNode");
        var movement = bb.Get<COMP476HiderMovement>("Movement");
        var transform = bb.Get<Transform>("Self");
        var pathfinder = bb.Get<Pathfinder>("Pathfinder");

        // Check if reached final destination
        if (Vector3.Distance(transform.position, targetNode.transform.position) <= _waypointThreshold)
        {
            return BTStatus.Success;
        }

        // Repath if needed (no current access point or close to it)
        if (_currentAccessPoint == null ||
            Vector3.Distance(transform.position, _currentAccessPoint.transform.position) <= _repathDistance)
        {
            _currentAccessPoint = pathfinder.FindOptimalAccessPoint(transform, targetNode);
            if (_currentAccessPoint == null) return BTStatus.Failure;
        }

        // Move toward current access point
        movement.MoveToward(_currentAccessPoint.transform.position, _waypointThreshold);

        return BTStatus.Running;
    }

    public override void OnEnter()
    {
        // Reset pathfinding on start
        _currentAccessPoint = null;
    }

    public override void OnExit()
    {
        bb.Get<COMP476HiderMovement>("Movement").Stop();
        _currentAccessPoint = null;
    }
}