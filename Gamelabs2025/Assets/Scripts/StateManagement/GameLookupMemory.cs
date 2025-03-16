using Player;

namespace StateManagement
{
    public class GameLookupMemory
    {
        public static PlayerRole.RoleType Winner { get; set; }
        public static PlayerRole.RoleType MyLocalPlayerRole { get; set; } = PlayerRole.RoleType.None;
    }
}