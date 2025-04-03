using FishNet.Object;
using UnityEngine;

public class NetworkOpenCloseDoor : NetworkBehaviour
{
    [SerializeField] private GameObject door;      // The door to open/close
    [SerializeField] private bool isOpen = false;  // Current state of the door (open or closed)
    public bool isPlayerNear = false;             // Whether the player is close enough to interact

    private float doorSpeed = 2f;
    private float targetAngle = 0f;
    private float currentAngle = 0f;


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


    private void Update()
    {
        if (!NetworkObject || !NetworkObject.IsSpawned) return; 

     
        if (isPlayerNear)
        {
           
            InScreenUI.Instance.ShowInputPrompt(InputReader.Instance.inputMap.Gameplay.Interact, isOpen ? "Close" : "Open");
        }
        else
        {
            
            InScreenUI.Instance.RemoveInputPrompt(InputReader.Instance.inputMap.Gameplay.Interact);
        }

        
        if (door != null)
        {
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * doorSpeed);
            door.transform.localRotation = Quaternion.Euler(-90, 0, currentAngle);
        }
    }

    private void HandleInteractInput()
    {
        if (isPlayerNear)
        {
            
            ToggleDoorState();
            Debug.Log("INTERACT DOOR");
        }
    }

    [ServerRpc(RequireOwnership = false)] 
    private void ToggleDoorState()
    {
        isOpen = !isOpen;
        UpdateDoorState(isOpen); 
    }

    [ObserversRpc]
    private void UpdateDoorState(bool state)
    {
        isOpen = state;
        targetAngle = isOpen ? 90f : 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkObject || !NetworkObject.IsSpawned) return;

        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!NetworkObject || !NetworkObject.IsSpawned) return;

     
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            InScreenUI.Instance.RemoveInputPrompt(InputReader.Instance.inputMap.Gameplay.Interact); 
        }
    }
}
