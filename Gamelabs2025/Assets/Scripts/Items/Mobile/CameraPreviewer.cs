using System;
using System.Collections;
using Networking;
using Player.Audio;
using Player.WorldMarker;
using StateManagement;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Items
{
    public class CameraPreviewer : MonoBehaviour
    {
        [SerializeField] InputReader inputReader;
        [SerializeField] private TMPro.TMP_Text cameraName;
        [FormerlySerializedAs("icon")] 
        [SerializeField] private GameObject helpGUIObject;
        [SerializeField] private Sprite worldMarkerIcon;
        [SerializeField] private AudioClip markerSound;
        
        private static int id = -1;
        CCTVCamera previewCamera;
        
        private static string currentWorldMarker = "";
        Camera mainCamera;
        
        private void Start()
        {
            inputReader = InputReader.Instance;
            
            mainCamera = Camera.main;
            mainCamera.gameObject.SetActive(false);
            inputReader.OnCCTVCameraSwitchEvent += InputReaderOnOnCCTVCameraSwitchEvent;
            inputReader.OnCloseUIEvent += InputReaderOnOnCloseUIEvent;
            inputReader.OnCCTVMarkedEvent += OnCameraMarked;
            
            //Move this to appropriate location later
            inputReader.SetToUIInputs();
            CycleCamera(1);
            StartCoroutine(RestartGUICanvas());
            GameLookupMemory.LocalPlayer.GetComponent<SeekerGraphicsManager>().SetRendererEnabled(true);
        }

        private void OnCameraMarked()
        {
            if(previewCamera == null)
                return;
            
            if(!string.IsNullOrEmpty(currentWorldMarker))
                WorldMarkerManager.Instance.DestroyMarker(currentWorldMarker);
            
            currentWorldMarker = WorldMarkerManager.Instance.AddWorldMarker(previewCamera.transform, worldMarkerIcon, true);
            AudioManager.Instance.PlaySFX(markerSound);
            Close();
        }

        private void OnDestroy()
        {
            inputReader.OnCCTVCameraSwitchEvent -= InputReaderOnOnCCTVCameraSwitchEvent;
            inputReader.OnCloseUIEvent -= InputReaderOnOnCloseUIEvent;
            inputReader.OnCCTVMarkedEvent -= OnCameraMarked;
            
            //Move this to appropriate location later
            GameLookupMemory.LocalPlayer.GetComponent<SeekerGraphicsManager>().SetRendererEnabled(false);
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
            
            if(CCTVCamera.CameraList == null || CCTVCamera.CameraList.Count == 0)
                return;
            
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
            cameraName.text = previewCamera.CameraName;
        }

        private IEnumerator RestartGUICanvas()
        {
            helpGUIObject.SetActive(false);
            yield return new WaitForSeconds(.25f);
            helpGUIObject.SetActive(true);
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