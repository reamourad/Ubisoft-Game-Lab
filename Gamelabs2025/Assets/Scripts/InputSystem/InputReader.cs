using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, InputMap.IGameplayActions, InputMap.IUIActions
{
    private InputMap inputMap;
    
    //Events
    public event Action<Vector2> OnMoveEvent;
    public event Action OnGrabEvent;
    public event Action OnLookEvent;

    private void OnEnable()
    {
        if (inputMap == null)
        {
            inputMap = new InputMap();
            
            inputMap.Gameplay.SetCallbacks(this);
            inputMap.UI.SetCallbacks(this);
            
            //start with the gameplay inputs
            SetToGameplayInputs();
        }
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
        OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnGrabEvent?.Invoke();
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
    }
}
