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
    public InputMap inputMap;
    public float movementInputDuration;
    public bool movementInputDetected;
    public Vector2 moveComposite;
    public Vector2 mouseDelta;
    
    //Events
    public event Action<Vector2> OnMoveEvent;
    public event Action OnGrabActivateEvent;
    
    public event Action OnGrabReleaseEvent;
    public event Action<bool> OnUseEvent;
    public event Action<Vector2> OnLookEvent;
    public event Action OnPlacementModeEvent;
    public event Action OnConnectItemsEvent;
    public event Action OnHiderItemScanEvent;

    public event Action<int> OnEquipInventoryItemEvent;
    public event Action OnToggleEquippedItemEvent;
    
    public event Action OnDropItemEvent;
    
    private static bool isGamepad = false;

    public event Action OnCloseUIEvent;
    public event Action<float> OnCCTVCameraSwitchEvent;
    public event Action OnCCTVMarkedEvent;
    
    public event Action OnCrouchActivated;
    public event Action OnCrouchDeactivated;

    public event Action OnJumpPerformed;

    public event Action OnLockOnToggled;

    public event Action OnSprintActivated;
    public event Action OnSprintDeactivated;

    public event Action OnWalkToggled;

    public event Action OnTutorialNextEvent;

    public event Action OnTutorialBackEvent;
    


    /// <summary>
    /// Main Menu Inputs
    /// </summary>
    public event Action OnMainMenuStart;
    public event Action<float> OnMainMenuNavigationUpDown;
    public event Action<float, bool> OnMainMenuNavigationLeftRight;
    public event Action OnMainMenuAccept;
    public event Action OnMainMenuBack;
    
    // Menu Inputs
    public event Action<float> OnSettingsTabSwitchEvent;
    public event Action OnPauseEvent;
    

    public event Action OnInteractEvent;

    private static InputReader _instance = null;

    public static InputReader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<InputReader>("InputReaderAsset");
                _instance.Initialise();
            }
            return _instance;
        }
    }
    private void Initialise()
    {
        inputMap = new InputMap();
        inputMap.Gameplay.SetCallbacks(this);
        inputMap.UI.SetCallbacks(this);
        //start with the gameplay inputs
        SetToGameplayInputs();
    }
    
    public void OnDisable()
    {
        if(inputMap == null)
            return;
        
        inputMap.Gameplay.Disable();
        inputMap.UI.Disable();
    }

    //Enable gameplay inputs only
    public void SetToGameplayInputs()
    {
        inputMap.Gameplay.Enable();
        //disable every other maps
        inputMap.UI.Disable();
        Debug.Log("Game Input kachow");
    }
    
    //Enable UI inputs only 
    public void SetToUIInputs()
    {
        inputMap.UI.Enable();
        //disable every other maps
        inputMap.Gameplay.Disable();
        Debug.Log("UI Input");
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        //Debug.Log(context.ReadValue<Vector2>());
        moveComposite = context.ReadValue<Vector2>();
        movementInputDetected = moveComposite.magnitude > 0;
        OnMoveEvent?.Invoke(moveComposite);
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

    public void OnConnectItems(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnConnectItemsEvent?.Invoke();
        }
    }

    public void OnEquipInventoryItem1(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
            OnEquipInventoryItemEvent?.Invoke(1);
    }

    public void OnEquipInventoryItem2(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
            OnEquipInventoryItemEvent?.Invoke(2);
    }

    public void OnToggleEquipInventoryItem(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
            OnToggleEquippedItemEvent?.Invoke();
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
            OnDropItemEvent?.Invoke();
    }

    public void OnHiderItemScan(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
            OnHiderItemScanEvent?.Invoke();
            
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
            OnPauseEvent?.Invoke();
    }

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
        if(context.phase == InputActionPhase.Performed)
            OnCCTVCameraSwitchEvent?.Invoke(context.ReadValue<float>());
    }

    public void OnClose(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
            OnCloseUIEvent?.Invoke();
    }

    public void OnMainMenu_NavigationUpDown(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnMainMenuNavigationUpDown?.Invoke(context.ReadValue<float>());
        }
    }

    public void OnMainMenu_NavigationLeftRight(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            OnMainMenuNavigationLeftRight?.Invoke(context.ReadValue<float>(), true);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            OnMainMenuNavigationLeftRight?.Invoke(context.ReadValue<float>(), false);
        }
        
    }

    public void OnMainMenu_Start(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnMainMenuStart?.Invoke();
        }
    }
    
    public void OnMainMenu_Accept(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnMainMenuAccept?.Invoke();
        }
    }

    public void OnMainMenu_Back(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnMainMenuBack?.Invoke();
        }
    }

    public void OnMarkCCTV(InputAction.CallbackContext context)
    {
       if(context.phase == InputActionPhase.Performed)
           OnCCTVMarkedEvent?.Invoke();
    }

    public void OnTutorialNext(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
            OnTutorialNextEvent?.Invoke();
    }
    
    public void OnTutorialBack(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
            OnTutorialBackEvent?.Invoke();
    }

    public void OnSettingsTabSwitch(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
            OnSettingsTabSwitchEvent?.Invoke(context.ReadValue<float>());
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnInteractEvent?.Invoke();
        }
          
    }
}
