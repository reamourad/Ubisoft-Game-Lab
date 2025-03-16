using System;
using System.Collections;
using Networking;
using UnityEngine;

namespace StateManagement
{
    public class CutsceneController : MonoBehaviour
    {
        private IEnumerator Start()
        {
            if (!NetworkUtility.IsServer)
            {
               yield break;
            }
            
            Debug.Log("Waiting for cutscene...");
            yield return new WaitForSeconds(1f);
            Debug.Log("Cutscene complete");
            GameStateController.Instance?.ServerChangeState(GameStates.Game);
        }
    }
}