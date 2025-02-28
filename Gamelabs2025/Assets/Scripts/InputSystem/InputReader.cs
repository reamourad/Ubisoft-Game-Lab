using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, InputMap.IGameplayActions, InputMap.IUIActions
{
    //making it into a singleton accessible in all classes
    public static InputReader Instance { get; private set; }
    
    public InputMap inputMap;
    
    //Events
    public event Action<Vector2> OnMoveEvent;
    public event Action OnGrabActivateEvent;
    
    public event Action OnGrabReleaseEvent;
    public event Action<bool> OnUseEvent;
    public event Action<Vector2> OnLookEvent;
    public event Action OnPlacementModeEvent;
    public event Action OnConnectItemsEvent;
    
    private static bool isGamepad = false;

    public event Action OnCloseUIEvent;
    public event Action<float> OnCCTVCameraSwitchEvent;
    

    private void OnEnable()
    {
        //singleton initialization
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Multiple InputReaders found! Using the first instance.");
            return;
        }
        
        //initialize the input map
        if (inputMap == null)
        {
            inputMap = new InputMap();
            
            inputMap.Gameplay.SetCallbacks(this);
            inputMap.UI.SetCallbacks(this);
            
            //start with the gameplay inputs
            SetToGameplayInputs();
        }
    }

    public void OnDisable()
    {
        inputMap.Gameplay.Disable();
        inputMap.UI.Disable();
    }

    //Enable gameplay inputs only
    public void SetToGameplayInputs()
    {
        inputMap.Gameplay.Enable();
        //disable every other maps
        inputMap.UI.Disable();
    }
    
    //Enable UI inputs only 
    public void SetToUIInputs()
    {
        inputMap.UI.Enable();
        //disable every other maps
        inputMap.Gameplay.Disable();
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        //Debug.Log(context.ReadValue<Vector2>());
        OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnGrabActivateEvent?.Invoke();
        }

        if (context.phase == InputActionPhase.Canceled)
        {
            OnGrabReleaseEvent?.Invoke();
        }
    }
    
    public void OnPlacementMode(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnPlacementModeEvent?.Invoke();
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        //Debug.Log(context.ReadValue<Vector2>());
        OnLookEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnUseItem(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnUseEvent?.Invoke(true);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            OnUseEvent?.Invoke(false);
        }
    }
<<<<<<< HEAD

    public void OnConnectItems(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnConnectItemsEvent?.Invoke();
        }
    }

=======
    
>>>>>>> main
    public static string GetCurrentBindingText(InputAction action)
    {
        // This is a bit of a hack, it has to be called from a FixedUpdate or Update method to work properly
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            isGamepad = true;
        }
        else if (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame)
        {
            isGamepad = false;
        }
        
        if (isGamepad)
        {
            return action.GetBindingDisplayString(1, InputBinding.DisplayStringOptions.DontIncludeInteractions);
        }
        else // Defaults to keyboard
        {
            return action.GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
        }
    }

    public void OnCCTVSwitchCameras(InputAction.CallbackContext context)
    {
        OnCCTVCameraSwitchEvent?.Invoke(context.ReadValue<float>());
    }

    public void OnClose(InputAction.CallbackContext context)
    {
        OnCloseUIEvent?.Invoke();
    }
}
