using System;
using FishNet.Object;
using UnityEngine;

namespace Player.IK
{
    public class IKAssigner : MonoBehaviour
    {
        [SerializeField] Transform targetLeft;
        [SerializeField] Transform targetRight;
        private SeekerIKController controller;
        private void Start()
        {
            controller = GetComponentInParent<SeekerIKController>();
            if (controller != null)
            {
                controller.SetTarget(targetLeft, targetRight);
            }
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.StopIK();
            }
        }
    }
}