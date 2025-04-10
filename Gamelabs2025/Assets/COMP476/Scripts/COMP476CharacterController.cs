using System;
using Items.Interfaces;
using UnityEngine;

public class COMP476CharacterController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;
    public Transform cameraTransform;

    public bool isPlaying = false;

    float mouseX;
    float mouseY;
    float verticalLookRotation = 0f;


    public bool isPlaying=false;
    
    public IUsableItem usableItem;

    private void Start()
    {
        usableItem = GetComponentInChildren<IUsableItem>();
    }

    void Update()
    {
        if (isPlaying)
        {
            // Get movement input
            float horizontalInput = Input.GetAxis("Horizontal"); // A/D for strafe
            float verticalInput = Input.GetAxis("Vertical");     // W/S for forward/backward

            // Calculate movement vector
            Vector3 move = (transform.right * horizontalInput + transform.forward * verticalInput) * moveSpeed * Time.deltaTime;
            transform.position += move;

            // Mouse look
            mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            // Horizontal rotation (turn character)
            transform.Rotate(Vector3.up * mouseX);

            // Vertical camera rotation (look up/down)
            verticalLookRotation -= mouseY;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f); // Clamp for realism

            if (cameraTransform != null)
                cameraTransform.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
            // Apply movement
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
            
            // Activate Vacuum
            usableItem?.UseItem(Input.GetMouseButton(0));
        }

 
    }
}