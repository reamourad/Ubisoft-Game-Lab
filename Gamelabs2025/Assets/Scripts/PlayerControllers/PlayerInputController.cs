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
        InputReader.Instance.OnGrabActivateEvent += HandleGrab; 
        InputReader.Instance.OnGrabReleaseEvent += HandleGrabRelease; 
        InputReader.Instance.OnConnectItemsEvent += HandleConnection;
    }


    private void OnDisable()
    {
        InputReader.Instance.OnMoveEvent -= HandleMove;
        InputReader.Instance.OnLookEvent -= HandleLook;
        InputReader.Instance.OnGrabActivateEvent -= HandleGrab;
        InputReader.Instance.OnGrabReleaseEvent -= HandleGrabRelease;
        InputReader.Instance.OnConnectItemsEvent -= HandleConnection;
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
    
    private void HandleGrabRelease()
    {
        Debug.Log("grab release");
        playerController.OnGrabRelease();
    }

    private void HandleConnection()
    {
        playerController.OnConnection();
    }
}
