using System;
using FishNet;
using Unity.VisualScripting;
using UnityEngine;

namespace Utils
{
    public class GhostTailTargetFollower : MonoBehaviour
    {
        [SerializeField] private Transform fixedPoint;
        [SerializeField] private Transform movingPoint;
        [SerializeField] private Transform ikTarget;

        [SerializeField] private Vector3 offset;
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float stoppingDistance = 0.1f;
        [SerializeField] private float maxDistance = 2f;

        private void OnValidate()
        {
            if (!Application.isPlaying && ikTarget != null)
            {
                name = "Follow_" + ikTarget.name;
                if (fixedPoint != null)
                {
                    fixedPoint.position = ikTarget.position;
                    fixedPoint.rotation = ikTarget.rotation;
                }
            }
        }

        private void Start()
        {
            if (movingPoint != null)
                movingPoint.parent = null;
        }

        private void Update()
        {
#if UNITY_SERVER
            //don't run this update on server
            if(InstanceFinder.NetworkManager != null && InstanceFinder.NetworkManager.IsServerStarted)
                return;
#endif
            
            if (fixedPoint == null || movingPoint == null)
                return;

            Vector3 targetPosition = fixedPoint.position + offset;
            Vector3 currentPosition = movingPoint.position;

            float distance = Vector3.Distance(currentPosition, targetPosition);
        
            if (distance <= stoppingDistance)
                return;

            // Clamp to maxDistance
            if (distance > maxDistance)
            {
                Vector3 clampedDirection = (targetPosition - currentPosition).normalized;
                targetPosition = currentPosition + clampedDirection * maxDistance;
            }

            // Smooth follow
            movingPoint.position = Vector3.Lerp(currentPosition, targetPosition, followSpeed * Time.deltaTime);

            if (ikTarget != null)
                ikTarget.position = movingPoint.position;
        }

        private void OnDrawGizmos()
        {
            if (fixedPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(fixedPoint.position + offset, 0.01f);
            }

            if (movingPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(movingPoint.position, 0.01f);
            }
            
            if (ikTarget != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(ikTarget.position, 0.01f);
            }
        }
    }
}