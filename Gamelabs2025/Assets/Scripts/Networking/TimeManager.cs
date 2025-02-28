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
        [SerializeField] private TimeManagerUI ui;
        
        private readonly SyncVar<int> timer = new();
        private readonly SyncVar<string> timerTitle = new(string.Empty);
        private Action onComplete;
        
        private bool isStarted;
        private bool isPaused;
        
        private Coroutine timerRoutine;
        
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

        public void Initialize(int startValue, Action onComplete, string title="")
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
            timerRoutine = StartCoroutine(TimerCoroutine());
        }

        public void StopActiveTimer()
        {
            if(timerRoutine != null)
                StopCoroutine(timerRoutine);
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
                break;
            }
        }

        private void UpdateText(int previousValue, int newValue, bool isServer)
        {
            ui.SetTimerText($"{newValue / 60}:{newValue % 60:D2}");
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
