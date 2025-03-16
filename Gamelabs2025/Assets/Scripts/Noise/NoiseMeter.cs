using UnityEngine;
using TMPro;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class NoiseMeterUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI noiseText;
    private HouseNoiseListener houseNoise;

    public override void OnStartClient()
    {
        base.OnStartClient();

        houseNoise = FindFirstObjectByType<HouseNoiseListener>();
        if (houseNoise == null)
        {
            Debug.LogError("[NoiseMeterUI] No HouseNoiseListener found!");
            return;
        }

        houseNoise.noiseMeter.OnChange += UpdateNoiseUI;
    }

    private void OnDisable()
    {
        if (houseNoise != null)
        {
            houseNoise.noiseMeter.OnChange -= UpdateNoiseUI;
        }
    }

    private void UpdateNoiseUI(float oldNoise, float newNoise, bool asServer)
    {
        noiseText.text = $"Noise Level: {newNoise}%";
    }
}
