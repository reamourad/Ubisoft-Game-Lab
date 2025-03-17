using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoiseManager : NetworkBehaviour
{
    public static NoiseManager Instance { get; private set; }

    /// <summary>
    /// Event triggered when noise is generated.
    /// </summary>
    public static event Action<Vector3, float, float> OnNoiseGenerated;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("NoiseManager initialized on the Server.");
    }

    /// <summary>
    /// The server propagates noise and informs listeners.
    /// </summary>
    public void GenerateNoise(Vector3 position, float strength, float dissipation)
    {
        if (!IsServerStarted) return; // ✅ Ensures only the server runs this

        Debug.Log($"Noise generated at {position} with strength {strength} and dissipation {dissipation}");

        // 🔹 Fire the event so all listeners respond
        OnNoiseGenerated?.Invoke(position, strength, dissipation);

        RPC_NotifyClientsNoise(position, strength, dissipation);
    }

    [ObserversRpc]
    private void RPC_NotifyClientsNoise(Vector3 position, float strength, float dissipation)
    {
        Debug.Log($"Client received noise notification at {position} with strength {strength}");
    }
}
