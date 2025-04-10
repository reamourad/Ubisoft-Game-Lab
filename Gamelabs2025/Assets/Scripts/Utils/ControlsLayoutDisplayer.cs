using System;
using Player;
using StateManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utils
{
    public class ControlsLayoutDisplayer : MonoBehaviour
    {
        [SerializeField] private GameObject GamepadObject;
        [SerializeField] private GameObject GamepadSeeker;
        [SerializeField] private GameObject GamepadHider;

        [SerializeField] private GameObject KeybaordObject;
        [SerializeField] private GameObject KeyboardSeeker;
        [SerializeField] private GameObject KeyboardHider;


        private void ResetGui()
        {
            GamepadObject.SetActive(false);
            GamepadSeeker.SetActive(false);
            GamepadHider.SetActive(false);
            
            KeybaordObject.SetActive(false);
            KeyboardHider.SetActive(false);
            KeyboardSeeker.SetActive(false);
        }
        
        private void Update()
        {
            if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
            {
                ShowGamepad();
            }
            else if (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame)
            {
                ShowKeyboard();
            }
        }

        private void ShowGamepad()
        {
            ResetGui();
            GamepadObject.SetActive(true);
            if(GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.Seeker)
                GamepadSeeker.SetActive(true);
            else if(GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.Hider)
                GamepadHider.SetActive(true);
        }

        private void ShowKeyboard()
        {
            ResetGui();
            KeybaordObject.SetActive(true);
            if(GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.Seeker)
                KeyboardSeeker.SetActive(true);
            else if(GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.Hider)
                KeyboardHider.SetActive(true);
        }
    }
}