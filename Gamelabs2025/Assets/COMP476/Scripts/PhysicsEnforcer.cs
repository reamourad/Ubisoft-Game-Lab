using UnityEngine;
using FishNet.Managing;

public class PhysicsEnforcer : MonoBehaviour
{
    private void Awake()
    {
        Physics.simulationMode = SimulationMode.FixedUpdate;

#if UNITY_EDITOR
        // Prevent override in editor
        UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
    }

#if UNITY_EDITOR
    private void OnPlayModeChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
        {
            Physics.simulationMode = SimulationMode.FixedUpdate;
        }
    }
#endif
}