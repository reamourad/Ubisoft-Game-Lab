using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

namespace Networking
{
    public class TimeManager : NetworkBehaviour
    {
        [SerializeField] private TMP_Text timerText;
        
        private readonly SyncVar<int> timer = new();
        private Action onComplete;
        
        private bool isStarted;
        private bool isPaused;
        
        private static TimeManager instance;
        public static TimeManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<TimeManager>();
                
                return instance;
            }
        }

        private void Start()
        {
            timer.OnChange += UpdateText;
            //Initialize(300, null);
        }

        public void Initialize(int startValue, Action onComplete)
        {
            if (!NetworkUtility.IsServer) return;
            timer.Value = startValue;
            this.onComplete = onComplete;
            StartTimer();
        }
        
        private void StartTimer()
        {
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

        private void UpdateText(int previousValue, int newValue, bool isServer)
        {
            timerText.text = $"{newValue / 60}:{newValue % 60:D2}";
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
