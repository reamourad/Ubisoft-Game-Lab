using UnityEngine;

public interface INoiseListener
{
    void ReceiveNoise(Vector3 noiseOrigin, float noiseStrength, float dissipation);
}
