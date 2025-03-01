using FishNet.Object;
using TMPro;
using UnityEngine;
using System.Collections;

public class NoiseMeter : NetworkBehaviour, INoiseListener
{
    [SerializeField] private TextMeshProUGUI noiseAlertText; // Assign in Inspector
    [SerializeField] private float displayTime = 2.5f; // Time before fading out
    [SerializeField] private Color highNoiseColor = Color.red;
    [SerializeField] private Color lowNoiseColor = Color.yellow;
    [SerializeField] private Transform playerTransform; // Player position reference

    private Coroutine fadeCoroutine;

    private void Start()
    {
        NoiseManager.Instance?.RegisterListener(this); // Register as a noise listener

        if (noiseAlertText != null)
            noiseAlertText.text = "";
    }

    private void OnDestroy()
    {
        NoiseManager.Instance?.UnregisterListener(this); // Unregister when destroyed
    }

    public void ReceiveNoise(Vector3 noiseOrigin, float noiseStrength, float dissipation)
    {
        if (!IsServerStarted) return; // ✅ Only the server processes noise

        if (playerTransform == null)
            return;

        float distance = Vector3.Distance(playerTransform.position, noiseOrigin);

        // Apply dissipation to noise strength
        float adjustedStrength = noiseStrength * (1f - (dissipation * Mathf.Clamp01(distance)));

        // Skip if too weak
        if (adjustedStrength < 1f)
            return;

        Debug.Log($"UI Noise Detector received noise: {adjustedStrength} at {noiseOrigin}");

        // Update UI
        RPC_UpdateNoiseMeter(adjustedStrength, noiseOrigin);
    }

    [ObserversRpc]
    private void RPC_UpdateNoiseMeter(float strength, Vector3 origin)
    {
        if (noiseAlertText == null) return;

        // Set text color based on strength
        noiseAlertText.color = strength > 10f ? highNoiseColor : lowNoiseColor;

        // Update UI text
        noiseAlertText.text = $"🔊 Noise Level: {strength}\nFrom: {origin}";

        // Reset fade-out effect
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutText());
    }

    private IEnumerator FadeOutText()
    {
        yield return new WaitForSeconds(displayTime);

        float fadeDuration = 1f;
        Color startColor = noiseAlertText.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            noiseAlertText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        noiseAlertText.text = "";
    }
}
