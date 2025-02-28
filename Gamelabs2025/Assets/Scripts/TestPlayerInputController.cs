using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Networking;
using Unity.Cinemachine;
using UnityEngine;

public class TestPlayerInputController : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject playerVisuals;
    [SerializeField] private Transform playerItemHolderRight;
    
    [SerializeField] private float pitchSensitivity=1;
    [SerializeField] private float yawSensitivity=1;
    
    private Vector2 moveInput;
    private NetworkPlayerController.PlayerInputData inputData = default;
    private NetworkPlayerController playerController;
    
    private void OnDestroy()
    {
        if(!IsOwner)
            return;

        inputReader.OnUseEvent -= ClientHandleItemUsage;
        inputReader.OnMoveEvent -= ClientHandleMove;
        inputReader.OnLookEvent -= ClientHandleLook;
    }

    public override void OnStartClient()
    {
        if (!IsOwner)
        {
            playerItemHolderRight.SetParent(playerVisuals.transform, true);    
            return;
        }
        
        inputReader.OnUseEvent += ClientHandleItemUsage;
        inputReader.OnMoveEvent += ClientHandleMove;
        inputReader.OnLookEvent += ClientHandleLook;
        
        playerController = GetComponent<NetworkPlayerController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //playerVisuals.SetActive(false);
    }

    private void Update()
    {
        if(!IsOwner)
            return;
        
        inputData.PitchSensitivity = pitchSensitivity;
        inputData.YawSensitivity = yawSensitivity;
        playerController.UpdatePlayerInputs(inputData);
    }

    private void ClientHandleLook(Vector2 lookVector)
    {
        inputData.LookInputVector = lookVector;
    }
    
    private void ClientHandleMove(Vector2 moveVector)
    {
        //Debug.Log(moveVector);
        inputData.MoveInputVector = moveVector; 
    }

    private void ClientHandleItemUsage(bool use)
    {
        Debug.Log($"Item Use {use}");
        playerController.UseItem(use);
    }
}
