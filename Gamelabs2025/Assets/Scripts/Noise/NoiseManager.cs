using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoiseManager : NetworkBehaviour
{
    public static NoiseManager Instance { get; private set; }

    private readonly List<INoiseListener> listeners = new List<INoiseListener>();

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
    /// Registers a noise listener (e.g., traps, AI) to receive noise events.
    /// </summary>
    public void RegisterListener(INoiseListener listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    public void UnregisterListener(INoiseListener listener)
    {
        listeners.Remove(listener);
    }

    /// <summary>
    /// The server propagates noise and informs listeners.
    /// </summary>
    public void GenerateNoise(Vector3 position, float strength, float dissipation)
    {
        if (!IsServerStarted) return; // ✅ Ensures only the server runs this

        Debug.Log($"Noise generated at {position} with strength {strength} and dissipation {dissipation}");

        foreach (var listener in listeners)
        {
            listener.ReceiveNoise(position, strength, dissipation);
        }

        RPC_NotifyClientsNoise(position, strength, dissipation);
    }

    [ObserversRpc]
    private void RPC_NotifyClientsNoise(Vector3 position, float strength, float dissipation)
    {
        Debug.Log($"Client received noise notification at {position} with strength {strength}");
    }
}
