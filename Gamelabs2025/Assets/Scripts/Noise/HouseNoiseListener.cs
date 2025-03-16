using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class HouseNoiseListener : NetworkBehaviour
{
    [SerializeField] private float noiseThreshold = 100f;
    [SerializeField] private float noiseDecayRate = 5f;

    private readonly SyncVar<float> noiseMeter = new SyncVar<float>(0f, new SyncTypeSettings(1f));
    private readonly SyncVar<bool> isAngry = new SyncVar<bool>(false, new SyncTypeSettings(1f));

    private void Awake()
    {
        noiseMeter.OnChange += OnNoiseLevelChanged;
        isAngry.OnChange += OnAngryStateChanged;
    }

    private void OnEnable()
    {
        NoiseManager.OnNoiseGenerated += HandleNoise;
    }

    private void OnDisable()
    {
        NoiseManager.OnNoiseGenerated -= HandleNoise;
    }

    private void Update()
    {
        if (!IsServerStarted) return;

        if (noiseMeter.Value > 0)
        {
            noiseMeter.Value -= noiseDecayRate * Time.deltaTime;
            noiseMeter.Value = Mathf.Max(noiseMeter.Value, 0);
        }
    }

    private void HandleNoise(Vector3 noiseOrigin, float noiseStrength, float _)
    {
        if (!IsServerStarted) return;

        float previousNoise = noiseMeter.Value;
        noiseMeter.Value += noiseStrength;
        noiseMeter.Value = Mathf.Min(noiseMeter.Value, noiseThreshold);

        if (noiseMeter.Value >= noiseThreshold && !isAngry.Value)
        {
            isAngry.Value = true;
        }
    }

    private void OnNoiseLevelChanged(float oldValue, float newValue, bool asServer)
    {
        Debug.Log($"[HouseNoiseListener] Noise level updated: {newValue} on {(asServer ? "Server" : "Client")}");
    }

    private void OnAngryStateChanged(bool oldValue, bool newValue, bool asServer)
    {
        Debug.Log($"[HouseNoiseListener] House is now {(newValue ? "ANGRY" : "CALM")} on {(asServer ? "Server" : "Client")}");
    }

    public float GetNoisePercentage()
    {
        return noiseMeter.Value / noiseThreshold;
    }

    public bool IsAngry()
    {
        return isAngry.Value;
    }
}
