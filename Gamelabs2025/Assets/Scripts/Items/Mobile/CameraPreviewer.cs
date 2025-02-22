using System;
using UnityEngine;

namespace Items
{
    public class CameraPreviewer : MonoBehaviour
    {
        [SerializeField] InputReader inputReader;
        
        private int id = -1;
        CCTVCamera previewCamera;
        
        Camera mainCamera;
        
        private void Start()
        {
            mainCamera = Camera.main;
            mainCamera.gameObject.SetActive(false);
            inputReader.OnCCTVCameraSwitchEvent += InputReaderOnOnCCTVCameraSwitchEvent;
            inputReader.OnCloseUIEvent += InputReaderOnOnCloseUIEvent;
            
            //Move this to appropriate location later
            Debug.Log("Enabling UI Inputs ONLY!!");
            inputReader.SetToUIInputs();
            CycleCamera(1);
        }
        
        private void OnDestroy()
        {
            inputReader.OnCCTVCameraSwitchEvent -= InputReaderOnOnCCTVCameraSwitchEvent;
            inputReader.OnCloseUIEvent -= InputReaderOnOnCloseUIEvent;
            
            //Move this to appropriate location later
            Debug.Log("Enabling UI Game Inputs ONLY!!");
            inputReader.SetToGameplayInputs();
        }

        private void InputReaderOnOnCloseUIEvent()
        {
           Close();
        }

        private void InputReaderOnOnCCTVCameraSwitchEvent(float value)
        {
            if (value >= 0.9f)
            {
                CycleCamera(1);
            }
            else if (value <= -0.9f)
            {
                CycleCamera(-1);
            }
        }

        public void CycleCamera(float dir=1)
        {
            if (previewCamera)
                previewCamera.ActivateCamera(false);
            
            if (dir > 0)
                id = (id + 1) % CCTVCamera.CameraList.Count;
            else if(dir < 0)
            {
                id -= 1;
                if (id < 0)
                    id = CCTVCamera.CameraList.Count - 1;
            }
            
            previewCamera = CCTVCamera.CameraList[id];
            previewCamera.ActivateCamera(true);
        }
        
        public void Open()
        {
            // nothing here for now, use animations later
        }

        public void Close(bool deleteAfterClose = true)
        {
            if(previewCamera)
                previewCamera.ActivateCamera(false);
            mainCamera.gameObject.SetActive(true);
            
            if(deleteAfterClose)
                Destroy(this.gameObject);
        }
        
    }
}