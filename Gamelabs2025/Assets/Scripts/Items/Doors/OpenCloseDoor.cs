using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Player;
using Player.Audio;
using Player.NotificationSystem;
using StateManagement;
using UnityEngine;

public class NetworkOpenCloseDoor : NetworkBehaviour
{
    [SerializeField] private GameObject door;      // The door to open/close
    [SerializeField] private bool isOpen = false;  // Current state of the door (open or closed)
    public bool isPlayerNear = false;             // Whether the player is close enough to interact

    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private AudioClip lookedClip;

    [SerializeField] private AudioClip rumblingClip;
    [SerializeField] private GameObject cameraShakePrefab;
    
    [SerializeField] private bool forceClosedUntilGameStart = false;

    private float doorSpeed = 2f;
    private float targetAngle = 0f;
    private float currentAngle = 0f;

    private bool isScaring;
    private GameObject shakeObj=null;

    Coroutine doorCoroutine;
    
    private void OnEnable()
    {
        // Subscribe to input events for door interaction using OnInteract
        InputReader.Instance.OnInteractEvent += HandleInteractInput;
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        InputReader.Instance.OnInteractEvent -= HandleInteractInput;
    }
    
    private void UpdateDoor()
    {
        if(doorCoroutine != null)
            StopCoroutine(doorCoroutine);
        doorCoroutine = StartCoroutine(DoorRoutine());
    }

    private IEnumerator DoorRoutine()
    {
        float timeStep = 0;
        float startAngle = currentAngle;

        if (IsClientStarted)
        {
            if(isOpen)
                source.PlayOneShot(openClip);
            else
                source.PlayOneShot(closeClip);
        }
        
        while (timeStep <= 1)
        {
            timeStep += Time.deltaTime / 0.35f;
            currentAngle = Mathf.Lerp(startAngle, targetAngle, timeStep);
            door.transform.localRotation = Quaternion.Euler(-90, 0, currentAngle);
            yield return new WaitForEndOfFrame();
        }
    }
    
    private void HandleInteractInput()
    {
        if (isPlayerNear)
        {
            //Lock the player during preparing stage
            if (GameLookupMemory.MyLocalPlayerRole == PlayerRole.RoleType.Seeker &&
                GameController.Instance.CurrentGameStage == GameController.GameStage.Preparing)
            {
                source.PlayOneShot(lookedClip);
                return;
            }

            if (forceClosedUntilGameStart && GameController.Instance.CurrentGameStage != GameController.GameStage.Game)
            {
                source.PlayOneShot(lookedClip);
                return;
            }
            
            RPC_ToggleDoorState();
        }
    }

    [ServerRpc(RequireOwnership = false)] 
    private void RPC_ToggleDoorState()
    {
        isOpen = !isOpen;
        targetAngle = isOpen ? 90f : 0f;
        UpdateDoor();
        RPC_UpdateDoorState(isOpen, Random.value <= 0.12f);
    }

    [ObserversRpc]
    private void RPC_UpdateDoorState(bool state, bool houseAngy)
    {
        isOpen = state;
        targetAngle = isOpen ? 90f : 0f;
        UpdateDoor();
        if (houseAngy && GameController.Instance.CurrentGameStage == GameController.GameStage.Game)
        {
            TriggerScareEffects();
            if(GameController.Instance != null)
                GameController.Instance.AggitateHouse();
        }
    }
    
    private void TriggerScareEffects()
    {
        

        if(shakeObj==null)
        {
            isScaring = false;
        }

        if (!isScaring)
        {
            isScaring = true;
            // Play global monster SFX
            AudioManager.Instance.PlayMonsterSFX(rumblingClip);
            NotificationSystem.Instance.Notify("The house is agitated!");

            // Instantiate camera shake effect
            if (cameraShakePrefab != null)
            {
                shakeObj = Instantiate(cameraShakePrefab);
                Destroy(shakeObj, 3f);
            }
          
        }
        
    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkObject || !NetworkObject.IsSpawned) return;

        var playerRole = other.GetComponent<PlayerRole>();
        if (playerRole != null && playerRole.Role == GameLookupMemory.MyLocalPlayerRole)
        {
            isPlayerNear = true;
            InScreenUI.Instance.ShowInputPrompt(InputReader.Instance.inputMap.Gameplay.Interact, isOpen ? "Close" : "Open");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!NetworkObject || !NetworkObject.IsSpawned) return;
        
        var playerRole = other.GetComponent<PlayerRole>();
        if (playerRole != null && playerRole.Role == GameLookupMemory.MyLocalPlayerRole)
        {
            isPlayerNear = false;
            InScreenUI.Instance.RemoveInputPrompt(InputReader.Instance.inputMap.Gameplay.Interact);
        }
    }
}
