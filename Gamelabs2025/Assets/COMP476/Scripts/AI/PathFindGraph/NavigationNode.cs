using UnityEngine;

[ExecuteInEditMode]
public class NavigationNode : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color nodeColor = Color.cyan;
    [Range(0.1f, 2f)] public float nodeSize = 0.5f;

    private void OnDrawGizmos()
    {
        // Draw the node sphere with solid color and outline
        Gizmos.color = new Color(nodeColor.r, nodeColor.g, nodeColor.b, 0.7f);
        Gizmos.DrawSphere(transform.position, nodeSize);

        Gizmos.color = nodeColor;
        Gizmos.DrawWireSphere(transform.position, nodeSize);
    }

    private void OnDestroy()
    {
        // When a node is destroyed, remove any connections to it
        var graph = GetComponentInParent<NavigationGraph>();
        if (graph != null)
        {
            graph.connections.RemoveAll(c => c.fromNode == this || c.toNode == this);
        }
    }
}