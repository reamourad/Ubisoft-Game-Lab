using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
     private PlayerController playerController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    //set up the input reader 
    private void OnEnable()
    {
        InputReader.Instance.OnMoveEvent += HandleMove;
        InputReader.Instance.OnLookEvent += HandleLook;
        InputReader.Instance.OnGrabEvent += HandleGrab; 
    }


    private void OnDisable()
    {
        InputReader.Instance.OnMoveEvent -= HandleMove;
        InputReader.Instance.OnLookEvent -= HandleLook;
        InputReader.Instance.OnGrabEvent -= HandleGrab;
    }
    
    private void HandleLook(Vector2 obj)
    {
    }

    private void HandleMove(Vector2 moveVector)
    {
        playerController.Move(moveVector);
    }

    private void HandleGrab()
    {
        playerController.OnGrab();
    }
}
