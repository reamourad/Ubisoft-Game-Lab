using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int speed; 
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CinemachinePanTilt panTilt;
    
    private Rigidbody rb;
    private Vector2 moveInput;

    public Transform grabPlacement; 
    public static InteractiveItem grabbedObject = null;
    
    //set up the input reader 
    private void OnEnable()
    {
        InputReader.Instance.OnMoveEvent += HandleMove;
        InputReader.Instance.OnLookEvent += HandleLook;
    }


    private void OnDisable()
    {
        InputReader.Instance.OnMoveEvent -= HandleMove;
        InputReader.Instance.OnLookEvent -= HandleLook;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        
        //Warnings for debugging 
        if (cameraTransform == null)
        {
            Debug.Log("Missing Camera Transform in Player Controller");
        }

        if (grabPlacement == null)
        {
            Debug.Log("Missing Grabbed Placement in Player Controller");
        }
    }

    private void FixedUpdate()
    {
        Vector3 move = cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x;
        move.y = 0f;
        rb.AddForce(move.normalized * speed, ForceMode.VelocityChange);
        if (panTilt)
        {
            // Get the current pan angle
            float panAngle = panTilt.PanAxis.Value;

            // Apply the pan angle to the character's Y-axis rotation
            transform.rotation = Quaternion.Euler(0, panAngle, 0);
        }
    }

    private void HandleLook(Vector2 obj)
    {
    }

    private void HandleMove(Vector2 moveVector)
    {
        //Debug.Log(moveVector);
        moveInput = moveVector; 
    }
}
