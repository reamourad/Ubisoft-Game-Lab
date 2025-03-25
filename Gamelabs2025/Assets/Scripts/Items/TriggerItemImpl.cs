using System;
using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;
using UnityEngine.AI;

/*
[RequireComponent(typeof(LineRenderer))]
public class TriggerItemImpl : MonoBehaviour, ITriggerItem
{
    private LineRenderer lr;
    [SerializeField] private Transform wireAnchor;
    private NavMeshPath path;
    private Transform target;
    private IReactionItem reactionItem;
    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
        path = new NavMeshPath();
        if (wireAnchor == null)
        {
            wireAnchor = transform;
        }
    }

    public void Connect(Transform newTarget)
    {
        target = newTarget;
        lr.enabled = true;
        RefreshPath();
    }
    
    public void Connect(IReactionItem newReactionItem)
    {
        reactionItem = newReactionItem;
        Connect(reactionItem.WireAnchor);
    }

    public void RefreshPath()
    {
        if (reactionItem != null)
        {
            target = reactionItem.WireAnchor;
        }
        
        if (target == null)
        {
            lr.enabled = false;
            return;
        }
        
        var startPosition = wireAnchor.position;
        var targetPosition = target.position;
        targetPosition.y = startPosition.y = -1.76f;
        var result = NavMesh.CalculatePath(startPosition, targetPosition, NavMesh.AllAreas, path);
        Debug.Log($"Path result: {result}");
        lr.positionCount = path.corners.Length;
        lr.SetPositions(path.corners);
    }

    private void OnDrawGizmos()
    {
        if (wireAnchor != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(wireAnchor.position, 0.3f);
        }
        
        if (target != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(target.position, 0.3f);
        }
    }

    protected virtual void OnTrigger()
    {
        reactionItem?.OnTrigger();
    }

    public Rope rope { get; set; }
}
*/
