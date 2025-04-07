using System;
using StateManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utils
{
    public class HapticsActivator : MonoBehaviour
    {
        [SerializeField] bool distanceBased = true;
        
        [SerializeField] private float closeStrengthLow;
        [SerializeField] private float closeStrengthHigh;
        
        [SerializeField] private float FarStrengthLow;
        [SerializeField] private float FarStrengthHigh;
        
        [SerializeField] private float DistanceThresh;
        [SerializeField] private float Liftime = 1f;
        private void Start()
        {
            var player = GameLookupMemory.LocalPlayer;
            var distance = Vector3.Distance(player.transform.position, transform.position);
            var normalizedDistance = Mathf.Clamp01(distance / DistanceThresh);

            if (Gamepad.current == null && Gamepad.current.wasUpdatedThisFrame)
            {
                if (distanceBased)
                {
                    Gamepad.current.SetMotorSpeeds(Mathf.Lerp(closeStrengthLow, FarStrengthLow, normalizedDistance), 
                        Mathf.Lerp(closeStrengthHigh, FarStrengthHigh, normalizedDistance));
                }
                else
                {
                    Gamepad.current.SetMotorSpeeds(closeStrengthLow, closeStrengthLow);
                }
            }
            
            Destroy(gameObject, Liftime);
        }

        private void OnDestroy()
        {
            if (Gamepad.current == null)
            {
               Gamepad.current.ResetHaptics();
            }
        }
    }
}