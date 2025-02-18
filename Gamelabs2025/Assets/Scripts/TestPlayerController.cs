using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Networking;
using UnityEngine;

public class TestPlayerController : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject playerVisuals;
    [SerializeField] private Transform playerItemHolderRight;
    
    private Vector2 moveInput;
    private NetworkPlayerMotor.PlayerMoveData moveData = default;
    private NetworkPlayerMotor playerMotor;
    
    private void OnDestroy()
    {
        if(!IsOwner)
            return;
        
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
        
        inputReader.OnMoveEvent += ClientHandleMove;
        inputReader.OnLookEvent += ClientHandleLook;
        
        playerMotor = GetComponent<NetworkPlayerMotor>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerVisuals.SetActive(false);
    }

    private void Update()
    {
        if(!IsOwner)
            return;
        
        playerMotor.UpdatePlayerInputs(moveData);
    }

    private void ClientHandleLook(Vector2 lookVector)
    {
        moveData.LookInputVector = lookVector;
    }
    
    private void ClientHandleMove(Vector2 moveVector)
    {
        //Debug.Log(moveVector);
        moveData.MoveInputVector = moveVector; 
    }
}
