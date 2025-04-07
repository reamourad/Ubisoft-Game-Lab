using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

namespace Items
{
    public class CCTVCamera : NetworkBehaviour
    {
        public static readonly List<CCTVCamera> CameraList = new List<CCTVCamera>();
        Animator animator;
        
        public bool Active { get; private set; }
        
        [SerializeField] private float fps = 24;
        [SerializeField] private string cameraName;
        
        private Camera cam;
        private float elapsed;
        
        public string CameraName => cameraName;
        
        private void Start()
        {
            cam = GetComponent<Camera>();
            animator = GetComponentInChildren<Animator>();
            CameraList.Add(this);
        }

        private void Update()
        {
            if(!Active)
                return;
            
            elapsed += Time.deltaTime;
            if (elapsed > 1 / fps) {
                elapsed = 0;
                cam.Render();
            }
        }
        
        public void ActivateCamera(bool activate)
        {
            Active = activate;
            RPC_ServerUpdateActiveStatusOnCamera(activate);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RPC_ServerUpdateActiveStatusOnCamera(bool activate)
        {
            animator.SetBool("Active", activate);
        }
        
    }
}