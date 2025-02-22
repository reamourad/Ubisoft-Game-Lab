using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public class CCTVCamera : MonoBehaviour
    {
        public static readonly List<CCTVCamera> CameraList = new List<CCTVCamera>();
        public bool Active { get; private set; }
        
        [SerializeField] private float fps = 24;
        private Camera cam;
        private float elapsed;
        
        private void Start()
        {
            cam = GetComponent<Camera>();
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
        }

    }
}