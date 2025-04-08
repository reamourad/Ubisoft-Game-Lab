using UnityEngine;

public class COMP476CharacterController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;

    public bool isPlaying=false;

    void Update()
    {
        if (isPlaying)
        {
            // Get input from WASD or arrow keys
            float horizontalInput = Input.GetAxis("Horizontal"); // A, D or Left, Right
            float verticalInput = Input.GetAxis("Vertical");   // W, S or Up, Down

            // Rotate the character left and right
            transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);

            // Calculate movement direction relative to the character's forward direction
            Vector3 moveDirection = transform.forward * verticalInput;

            // Apply movement
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        }

 
    }
}