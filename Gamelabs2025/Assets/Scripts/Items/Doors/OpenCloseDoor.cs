using FishNet.Object;
using UnityEngine;

public class NetworkOpenCloseDoor : NetworkBehaviour
{
    [SerializeField] private GameObject door;      // The door to open/close
    [SerializeField] private bool isOpen = false;  // Current state of the door (open or closed)
    public bool isPlayerNear = false;             // Whether the player is close enough to interact

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

        if (!NetworkObject || !NetworkObject.IsSpawned) return; // Avoid null reference

        if (!IsOwner)
            return;

        // Check if the player is close enough to interact
        if (isPlayerNear)
        {
            // Show the input prompt only when the player is near the door
            InScreenUI.Instance.ShowInputPrompt(InputReader.Instance.inputMap.Gameplay.Grab, isOpen ? "Close" : "Open");
        }
        else
        {
            // Remove the prompt when the player is not near
            InScreenUI.Instance.RemoveInputPrompt(InputReader.Instance.inputMap.Gameplay.Grab);
        }
    }

    private void HandleInteractInput()
    {
        if (isPlayerNear && IsOwner)
        {
            // Trigger the door action on the server when the player interacts
            ToggleDoorState();
        }
    }

    [ServerRpc(RequireOwnership = false)] // Ensures the server processes this RPC
    private void ToggleDoorState()
    {
        isOpen = !isOpen;

        // Synchronize door state across the network
        UpdateDoorState(isOpen);

        // Perform the door open/close logic (e.g., animate the door)
        if (isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    [ObserversRpc]
    private void UpdateDoorState(bool state)
    {
        isOpen = state;

        // Make sure all clients reflect the correct state of the door
        if (isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        if (door != null)
        {
            door.transform.Rotate(0, 90, 0); // Rotate the door 90 degrees around Y-axis to open it
        }
    }

    private void CloseDoor()
    {
        if (door != null)
        {
            door.transform.Rotate(0, -90, 0); // Rotate the door -90 degrees around Y-axis to close it
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkObject || !NetworkObject.IsSpawned) return; // Prevent null errors

        if (other.CompareTag("Player") && IsOwner)
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!NetworkObject || !NetworkObject.IsSpawned) return; // Prevent null errors

        // Check if the player exits the door's interaction range
        if (other.CompareTag("Player") && IsOwner)
        {
            isPlayerNear = false;
            InScreenUI.Instance.RemoveInputPrompt(InputReader.Instance.inputMap.Gameplay.Grab); // Remove prompt
        }
    }
}