using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraObstructionHandler : MonoBehaviour
{
    public Transform player; // Assign the player's Transform
    public LayerMask obstructionMask; // Set this in the Inspector to filter which objects should be transparent

    private Dictionary<Renderer, float> originalAlphas = new Dictionary<Renderer, float>(); // Stores original alpha values
    private HashSet<Collider> previousHitColliders = new HashSet<Collider>(); // Store hit colliders from last frame

    private void Update()
    {
        HandleObstructions();
    }

    void HandleObstructions()
    {
        Vector3 direction = transform.position - player.position;
        float distance = direction.magnitude;
        RaycastHit[] hits = Physics.RaycastAll(player.position, direction.normalized, distance, obstructionMask);

        HashSet<Collider> currentHitColliders = new HashSet<Collider>(); // Stores hit colliders this frame

        foreach (RaycastHit hit in hits)
        {
            Collider col = hit.collider;
            currentHitColliders.Add(col); // Add current frame's colliders to the set

            Renderer rend = col.GetComponent<Renderer>();
            if (rend != null)
            {
                FadeObject(rend, 0.09f); // Make transparent
            }
        }

        // Find objects that were hit last frame but NOT hit this frame
        HashSet<Collider> objectsToRestore = new HashSet<Collider>(previousHitColliders);
        objectsToRestore.ExceptWith(currentHitColliders);

        foreach (Collider col in objectsToRestore)
        {
            Renderer rend = col.GetComponent<Renderer>();
            if (rend != null)
            {
                RestoreObject(rend);
            }
        }

        // Update previous hit colliders for next frame
        previousHitColliders = currentHitColliders;
    }

    void FadeObject(Renderer rend, float alpha)
    {
        Material mat = rend.material;
        if (mat.HasProperty("_BaseColor")) // URP uses _BaseColor
        {
            if (!originalAlphas.ContainsKey(rend)) // Store original alpha only once
            {
                originalAlphas[rend] = mat.GetColor("_BaseColor").a;
            }

            Color color = mat.GetColor("_BaseColor");
            color.a = alpha;
            mat.SetColor("_BaseColor", color);
        }
    }

    void RestoreObject(Renderer rend)
    {
        if (originalAlphas.ContainsKey(rend))
        {
            Material mat = rend.material;
            if (mat.HasProperty("_BaseColor"))
            {
                Color color = mat.GetColor("_BaseColor");
                color.a = originalAlphas[rend]; // Restore original alpha
                mat.SetColor("_BaseColor", color);
            }
        }
    }

    // 🔹 Draw Gizmos to Debug Raycast in Scene View
    private void OnDrawGizmos()
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Vector3 direction = transform.position - player.position;
        Gizmos.DrawLine(player.position, transform.position);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(player.position, 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
