using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Networking
{
    public class Timer : NetworkBehaviour
    {
        private readonly SyncVar<int> timer = new();
        private Action onComplete;
        
        private bool isStarted;
        private bool isPaused;
        
        public void Initialize(int startValue, Action onComplete)
        {
            timer.Value = startValue;
            this.onComplete = onComplete;
        }
        
        [Server]
        public void StartTimer()
        {
            if (isStarted)
                return;
            
            isStarted = true;
            isPaused = false;
            StartCoroutine(TimerCoroutine());
        }
        
        private IEnumerator TimerCoroutine()
        {
            while (!isPaused && timer.Value > 0)
            {
                yield return new WaitForSeconds(1);

                timer.Value--;

                if (timer.Value > 0) continue;
                onComplete?.Invoke();
                isStarted = false;
            }
        }
        
        [Server]
        public void PauseTimer()
        {
            isStarted = false;
            isPaused = true;
        }
        
        [Server]
        public void IncreaseTime(int amount)
        {
            timer.Value += amount;
        }
        
    }
}
