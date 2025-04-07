using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Player.Audio;
using Player.UI.ControlPrompts;
using Unity.Cinemachine;
using UnityEngine;

namespace Player.Cutscene
{
    [Serializable]
    public class DialogData
    {
        public string Name;
        public string Text;
        public string CamToActivate;
        public string Next;
    }
    
    public class CutsceneSystem : MonoBehaviour
    {
        private readonly string START_KEY = "start";
        private readonly string END_KEY = "end";

        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private AudioClip sfxClick;
        
        [SerializeField] private GameObject textBox;
        [SerializeField] private TMPro.TMP_Text text;
        [SerializeField] private ControlPromptDisplayer controlPromptDisplayer;
        [SerializeField] TextAsset dialogDataSrc;
        
        private Dictionary<string, GameObject> cameras = new Dictionary<string, GameObject>();
        private Dictionary<string, DialogData> dialogData;
        private DialogData activeDialog;
        private GameObject activeCamera;
        private Coroutine textAnimationRoutine;
        private Action onDialogEnd;

        private bool animatingText;
        private void Start()
        {
            InputReader.Instance.SetToGameplayInputs();   
            foreach (var cam in GameObject.FindObjectsByType<CinemachineVirtualCameraBase>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                cameras[cam.name] = cam.gameObject;
            }
            
            InputReader.Instance.OnGrabActivateEvent += ContinueDialog;
        }
        
        public void StartDialogue(System.Action onDialogueFinished)
        {
            onDialogEnd = onDialogueFinished;
            AudioManager.Instance.PlayBG(bgmClip);
            dialogData = JsonConvert.DeserializeObject<Dictionary<string, DialogData>>(dialogDataSrc.text);
            ShowDialog(START_KEY);
        }

        private void ContinueDialog()
        {
            AudioManager.Instance.PlaySFX(sfxClick);
            if (animatingText)
            {
                SkipTextAnimation();
                return;
            }
            ShowDialog(activeDialog.Next);
        }

        private void ShowDialog(string key)
        {
            if (key == END_KEY)
            {
                EndDialog();
                return;
            }

            if(activeCamera != null)
                activeCamera.gameObject.SetActive(false);
            
            activeDialog = dialogData[key];
            activeCamera = cameras[activeDialog.CamToActivate];
            activeCamera.SetActive(true);
            textBox.SetActive(true);
            AnimateText(activeDialog.Text);
        }

        private void AnimateText(string text)
        {
            textAnimationRoutine = StartCoroutine(TextAnimationRoutine(text));
        }
        
        private void SkipTextAnimation()
        {
            if(textAnimationRoutine != null)
                StopCoroutine(textAnimationRoutine);
            
            controlPromptDisplayer.gameObject.SetActive(true);
            text.text = activeDialog.Text;
            animatingText = false;
        }

        IEnumerator TextAnimationRoutine(string textStr)
        {
            animatingText = true;
            text.text = "";
            controlPromptDisplayer.gameObject.SetActive(false);
            foreach(var ch in textStr.ToCharArray())
            {
                text.text += ch;
                yield return new WaitForSeconds(0.01f);
            }
            animatingText = false;
            controlPromptDisplayer.gameObject.SetActive(true);
        }
        
        private void EndDialog()
        {
            InputReader.Instance.OnGrabActivateEvent -= ContinueDialog;
            textBox.SetActive(false);
            onDialogEnd?.Invoke();
        }
    }
}