using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static COMP476HiderMovement;

public class ChooseRandomNode : BTAction
{
    private readonly float _dangerRadius;
    private readonly bool _avoidPlayer;

    public ChooseRandomNode(BTBlackboard bb, bool avoidPlayer, float dangerRadius = 14f) : base(bb)
    {
        _avoidPlayer = avoidPlayer;
        _dangerRadius = dangerRadius;
    }

    public override BTStatus Update()
    {
        var pathfinder = bb.Get<Pathfinder>("Pathfinder");
        var ghost = bb.Get<Transform>("Self");
        var player = bb.Get<Transform>("Player");

        if (pathfinder == null || ghost == null)
        {
            Debug.LogWarning("Missing pathfinder or ghost reference.");
            return BTStatus.Failure;
        }

        // Use the new random wander method
        NavigationNode targetNode = pathfinder.FindRandomWanderTarget(
            ghost,
            maxSteps: 10, // adjust as needed
            avoidTransform: _avoidPlayer ? player : null
        );

        if (targetNode == null)
        {
            Debug.LogWarning("Could not find a valid wander target.");
            return BTStatus.Failure;
        }

        bb.Set("TargetNode", targetNode);
        return BTStatus.Success;
    }
}


public class MoveToNode : BTAction
{
    private readonly float _waypointThreshold;
    private readonly float _repathDistance;
    private NavigationNode _currentAccessPoint;
    private bool _alertPlayer;

    public MoveToNode(BTBlackboard bb, bool alertPlayer = false, float waypointThreshold = 0.7f, float repathDistance = 4f) : base(bb)
    {
        _waypointThreshold = waypointThreshold;
        _repathDistance = repathDistance;
        _alertPlayer = alertPlayer;
    }

    public override BTStatus Update()
    {
        // Get references
        var targetNode = bb.Get<NavigationNode>("TargetNode");
        var movement = bb.Get<COMP476HiderMovement>("Movement");
        var transform = bb.Get<Transform>("Self");
        var pathfinder = bb.Get<Pathfinder>("Pathfinder");

        if (targetNode == null || movement == null || transform == null || pathfinder == null)
        {
            return BTStatus.Failure;
        }

        // Check if reached final destination
        if (Vector3.Distance(transform.position, targetNode.transform.position) <= _waypointThreshold)
        {
            return BTStatus.Success;
        }

        // Repath if needed (no current access point or close to it)
        if ((_currentAccessPoint == null ||
             Vector3.Distance(transform.position, _currentAccessPoint.transform.position) <= _repathDistance)
            )
        {
            Debug.Log("repath");
            _currentAccessPoint = pathfinder.FindOptimalAccessPoint(transform, targetNode,
                _alertPlayer ? bb.Get<Transform>("Player") : null, 14f);

            if (_currentAccessPoint == null)
                return BTStatus.Failure;
        }

        // Move toward access point
        if (_currentAccessPoint != null)
        {
            movement.MoveToward(_currentAccessPoint.transform.position, _waypointThreshold);
        }

        return BTStatus.Running;
    }

    public override void OnEnter()
    {
        // Reset pathfinding on start
        _currentAccessPoint = null;

        bb.Set("DebugColor", _alertPlayer ? Color.red : Color.cyan);

        var collider = bb.Get<Collider>("Collider");
        //if (_alertPlayer) GhostColliderHelper.EnableColliderForSeconds(bb.Get<MonoBehaviour>("SelfMono"), collider, 3f);

        bb.Get<COMP476HiderMovement>("Movement").SetBoost(_alertPlayer);

        bb.Set("NodeInfo", $"MoveToNode : alertPlayer = {_alertPlayer}");
    }

    public override void OnExit()
    {
        bb.Get<COMP476HiderMovement>("Movement").Stop();
        _currentAccessPoint = null;

        bb.Set("DebugColor", Color.black);
        bb.Get<COMP476HiderMovement>("Movement").SetBoost(false);
        bb.Set("NodeInfo", $"MoveToNode : done");
    }
}

public class PlayerDetectionNode : BTCondition
{
    private readonly float _minDetectionRange;
    private readonly float _maxDetectionRange;
    private readonly float _detectionAngle;
    private readonly LayerMask _obstructionMask;
    private readonly float _cooldownDuration = 5f;

    private float _lastDetectionTime;
    private bool _isInCooldown;

    public PlayerDetectionNode(BTBlackboard bb,
                             float minDetectionRange = 10f,
                             float maxDetectionRange = 30f,
                             float detectionAngle = 140f) : base(bb)
    {
        _minDetectionRange = minDetectionRange;
        _maxDetectionRange = maxDetectionRange;
        _detectionAngle = detectionAngle;
        _obstructionMask = bb.Get<LayerMask>("ObstructionMask");
    }

    public override bool Check()
    {
        // Check cooldown first
        if (_isInCooldown)
        {
            if (Time.time - _lastDetectionTime >= _cooldownDuration)
            {
                _isInCooldown = false;
            }
            else
            {
                return true;
            }
        }

        Transform self = bb.Get<Transform>("Self");
        Transform player = bb.Get<Transform>("Player");
        Vector3 toPlayer = player.position - self.position;
        float distance = toPlayer.magnitude;

        // Always detect if very close (bypasses cooldown)
        if (distance <= _minDetectionRange)
        {
            UpdateDetectionState(true);
            return true;
        }

        // Check if within max range
        if (distance <= _maxDetectionRange)
        {
            float angleToPlayer = Vector3.Angle(self.forward, toPlayer.normalized);
            float detectionProbability = Mathf.Clamp01(1 - (angleToPlayer / _detectionAngle));
            float effectiveRange = Mathf.Lerp(_minDetectionRange, _maxDetectionRange, detectionProbability);

            if (distance <= effectiveRange && !Physics.Linecast(self.position, player.position, _obstructionMask))
            {
                UpdateDetectionState(true);
                return true;
            }
        }

        UpdateDetectionState(false);
        return false;
    }

    private void UpdateDetectionState(bool detected)
    {
        if (detected)
        {
            _lastDetectionTime = Time.time;
            _isInCooldown = true;
        }
    }
}

public class CorneredCondition : BTCondition
{
    public CorneredCondition(BTBlackboard blackboard) : base(blackboard)
    {
    }

    public override bool Check()
    {
        var pathfinder = bb.Get<Pathfinder>("Pathfinder");

        // Get all nodes and filter if needed
        var allNodes = pathfinder.graph.GetComponentsInChildren<NavigationNode>();
        var validNodes = new List<NavigationNode>();

        foreach (var node in allNodes)
        {
            if (pathfinder.GetSafeDistance(bb.Get<Transform>("Self"), node) < pathfinder.GetSafeDistance(bb.Get<Transform>("Player"), node))
            {
                validNodes.Add(node);
            }
        }

        // Fallback if all nodes are in danger zone
        if (validNodes.Count == 0)
        {
            Debug.LogWarning("No safe nodes available - ghost is cornered");
            return true;
        }

        // Select random node from valid candidates
        var randomNode = validNodes[Random.Range(0, validNodes.Count)];
        bb.Set("TargetNode", randomNode);

        return false;
    }
}

public class PanicRunNode : BTAction
{
    private readonly float _panicSpeedMultiplier;
    private readonly float _minSafeDistance;
    private readonly float _panicDuration;
    private float _panicEndTime;

    public PanicRunNode(BTBlackboard bb, float panicSpeedMultiplier = 2f, float minSafeDistance = 20f, float panicDuration = 5f)
        : base(bb)
    {
        _panicSpeedMultiplier = panicSpeedMultiplier;
        _minSafeDistance = minSafeDistance;
        _panicDuration = panicDuration;
    }

    public override BTStatus Update()
    {
        var self = bb.Get<Transform>("Self");
        var player = bb.Get<Transform>("Player");
        var movement = bb.Get<COMP476HiderMovement>("Movement");

        // Calculate direct flee direction (ignore nodes/paths)
        Vector3 fleeDirection = (self.position - player.position).normalized;
        Vector3 targetPosition = self.position + fleeDirection * _minSafeDistance;

        // Move directly away at boosted speed
        movement.SetBoost(true);
        movement.MoveToward(targetPosition, 0.1f);

        // Check panic duration
        if (Time.time >= _panicEndTime && _panicDuration != -1)
        {
            return BTStatus.Success;
        }

        return BTStatus.Running;
    }

    public override void OnEnter()
    {
        _panicEndTime = Time.time + _panicDuration;
        bb.Get<COMP476HiderMovement>("Movement").SetBoost(true);

        var collider = bb.Get<Collider>("Collider");
        //GhostColliderHelper.EnableColliderForSeconds(bb.Get<MonoBehaviour>("SelfMono"), collider, 3f);

        bb.Set("DebugColor", Color.magenta); // Distinct panic color
        Debug.Log("PANIC MODE ACTIVATED!");

        bb.Set("NodeInfo", $"Panic run");
    }

    public override void OnExit()
    {
        bb.Get<COMP476HiderMovement>("Movement").Stop();
        bb.Get<COMP476HiderMovement>("Movement").SetBoost(false);
        bb.Set("DebugColor", Color.black);
        bb.Set("NodeInfo", $"Panic run : done");
    }
}

public class TeleportToClosestNode : BTAction
{
    private readonly bool _avoidPlayer;
    private readonly float _dangerRadius;

    public TeleportToClosestNode(BTBlackboard bb, bool avoidPlayer = true, float dangerRadius = 14f) : base(bb)
    {
        _avoidPlayer = avoidPlayer;
        _dangerRadius = dangerRadius;
    }

    public override BTStatus Update()
    {
        var pathfinder = bb.Get<Pathfinder>("Pathfinder");
        var ghost = bb.Get<Transform>("Self");
        var player = bb.Get<Transform>("Player");

        if (pathfinder == null || ghost == null)
        {
            Debug.LogWarning("Missing pathfinder or ghost reference.");
            return BTStatus.Failure;
        }

        NavigationNode closestNode = null;
        float closestDistance = Mathf.Infinity;

        foreach (var node in pathfinder.graph.GetComponentsInChildren<NavigationNode>())
        {
            float dist = Vector3.Distance(ghost.position, node.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestNode = node;
            }
        }

        if (closestNode == null)
        {
            Debug.LogWarning("No valid node found to teleport to.");
            return BTStatus.Failure;
        }

        ghost.position = closestNode.transform.position;
        bb.Set("TargetNode", closestNode);

        Debug.Log($"Ghost teleported to node: {closestNode.name}");
        return BTStatus.Success;
    }
}
