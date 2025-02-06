using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector2 mouseInput; 
    [SerializeField] private float sensitivity;

    private float pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //set up input reader
    private void OnEnable()
    {
        InputReader.Instance.OnLookEvent += OnLookEvent;
    }
    
    private void OnDisable()
    {
        InputReader.Instance.OnLookEvent -= OnLookEvent;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, mouseInput.x * sensitivity * Time.deltaTime );
        pitch = mouseInput.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        transform.localEulerAngles = new Vector3(pitch, transform.localEulerAngles.y, 0f);
        
    }

    private void OnLookEvent(Vector2 mouseMove)
    {
        mouseInput = mouseMove; 
    }
}
