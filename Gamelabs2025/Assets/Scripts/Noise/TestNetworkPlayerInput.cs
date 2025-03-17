using FishNet.Object;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

public class TestNetworkPlayerInput : NetworkBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) && !IsOwner) // Press 'J' to trigger noise
        {
            Debug.Log($"Not owner");
        }

        //if (!IsOwner) return; // Ensure only the owning player processes input

        if (Input.GetKeyDown(KeyCode.J)) // Press 'J' to trigger noise
        {
            RequestNoiseServerRpc(transform.position, 10f, 0.5f);
        }
    }

    /// <summary>
    /// Clients use this to request noise generation from the Server.
    /// </summary>
    [ServerRpc(RequireOwnership = true)]
    private void RequestNoiseServerRpc(Vector3 position, float strength, float dissipation)
    {
        if (!IsServerStarted) return; // ✅ Ensure only the server processes noise

        Debug.Log($"[Server] Noise requested from player at {position}");
        NoiseManager.Instance?.GenerateNoise(position, strength, dissipation);
    }
}
