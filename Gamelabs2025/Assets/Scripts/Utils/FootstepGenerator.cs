using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils
{
    public class FootstepGenerator : NetworkBehaviour
    {
        [SerializeField] private float stepInterval = 0.5f; // Time between steps
        [SerializeField] private float minSpeed = 0.1f; // Minimum speed to trigger footstep
        [SerializeField] private List<AudioClip> footsteps;
        [SerializeField] private AudioSource source;

        private Vector3 lastPos;
        private float stepTimer;
        private int footStepIndex = 0;

        private float speed;

        public override void OnStartClient()
        {
            base.OnStartClient();
            lastPos = transform.position;
            stepTimer = stepInterval;
        }

        private void Update()
        {
            if (!IsClientStarted)
                return;

            Vector3 currentPos = transform.position;
            float delta = Vector3.Distance(currentPos, lastPos);
            speed = Mathf.Lerp(speed, delta / Time.deltaTime, 0.1f); // Smoothing

            lastPos = currentPos;

            if (speed >= minSpeed)
            {
                stepTimer -= Time.deltaTime;

                while (stepTimer <= 0f)
                {
                    stepTimer += stepInterval;
                    var clip = footsteps[Random.Range(0, footsteps.Count)];
                    source.PlayOneShot(clip);
                }
            }
            else if (speed < 0.01f)
            {
                stepTimer = stepInterval;
            }
        }
    }
}