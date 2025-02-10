using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int speed; 
    [SerializeField] private Transform cameraTransform;
    
    private Rigidbody rb;
    private Vector2 moveInput;
    
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
        if (cameraTransform == null)
        {
            Debug.Log("Missing Camera Transform in Player Controller");
        }
    }

    private void FixedUpdate()
    {
        Vector3 move = cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x;
        move.y = 0f;
        rb.AddForce(move.normalized * speed, ForceMode.VelocityChange);
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
