using System;
using Player;
using UnityEngine;

namespace StateManagement
{
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private GameObject seekerWin;
        [SerializeField] private GameObject hiderWin;

        private void Start()
        {
            if(GameLookupMemory.Winner == PlayerRole.RoleType.Hider)
                hiderWin.SetActive(true);
            else if(GameLookupMemory.Winner == PlayerRole.RoleType.Seeker)
                seekerWin.SetActive(true);
        }
    }
}