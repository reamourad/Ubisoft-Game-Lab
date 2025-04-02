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

        [Header("For Composite Mapping, Enable this boolean and then set which part of the binding this Img shows")]
        [SerializeField] private bool hasPositiveNegative;
        [SerializeField] private bool showPositive;
        
        InputAction inputAction;
        
        private void Start()
        {
            if(actionReference)
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
            target.sprite = ControlImageLoader.Load(inputAction, hasPositiveNegative, showPositive);
        }
    }
}