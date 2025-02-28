using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int speed; 
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CinemachinePanTilt panTilt;
    [SerializeField] public Camera playerCamera;

    private Rigidbody rb;
    private Vector2 moveInput;
    
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
    }

    public void FixedUpdate()
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

    public void Look(Vector2 obj)
    {
    }

    public void Move(Vector2 moveVector)
    {
        //Debug.Log(moveVector);
        moveInput = moveVector; 
    }

    //to be overridden in hider controller
    public virtual void OnGrab(){}
    public virtual void OnGrabRelease(){}
    public virtual void OnConnection(){}
}
