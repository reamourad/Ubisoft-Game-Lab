using System;
using System.Collections;
using FishNet;
using FishNet.Object;
using Networking;
using Player.Cutscene;
using UnityEditor;
using UnityEngine;

namespace StateManagement
{
    public class CutsceneController : NetworkBehaviour
    {
        private int count=0;
        
        [SerializeField] CutsceneSystem cutscene;
        [SerializeField] GameObject waitScreen;

        public override void OnStartClient()
        {
            base.OnStartClient();
            cutscene?.StartDialogue(OnDialogComplete);
        }

        [Client]
        private void OnDialogComplete()
        {
            RPC_DialogComplete();
            waitScreen.SetActive(true);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RPC_DialogComplete()
        {
            count += 1;
            if (count >= InstanceFinder.ServerManager.Clients.Count)
            {
                Debug.Log("Cutscene complete");
                GameStateController.Instance?.ServerChangeState(GameStates.Game);
            }
        }
    }
    

}