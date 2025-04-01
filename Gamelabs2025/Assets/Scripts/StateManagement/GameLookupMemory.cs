using Player;
using UnityEngine;

namespace StateManagement
{
    public class GameLookupMemory
    {
        public static PlayerRole.RoleType Winner { get; set; }
        public static PlayerRole.RoleType MyLocalPlayerRole { get; set; } = PlayerRole.RoleType.None;

        public static GameObject LocalPlayer { get; set; }
    }
}