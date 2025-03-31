using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Player.UI.ControlPrompts
{
    public class ControlPromptDisplayer : MonoBehaviour
    {
        [SerializeField] private Image target;
        [SerializeField] private InputActionReference actionReference;

        InputAction inputAction;
        
        private void Start()
        {
            inputAction = actionReference.action;
            UpdateImage();
        }

        void LateUpdate()
        {
            if(inputAction == null)
                return;
            
            bool valid = Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame || 
                         Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

            if (valid)
                UpdateImage();
        }

        public void SetActionReference(InputAction action)
        {
            inputAction = action;
            UpdateImage();
        }
        
        private void UpdateImage()
        {
            target.sprite = ControlImageLoader.Load(InputReader.GetCurrentBindingText(inputAction));
        }
    }
}