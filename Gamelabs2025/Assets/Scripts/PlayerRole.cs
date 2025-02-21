using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerRole : MonoBehaviour
    {
        public enum RoleType
        {
            None = 0,
            Seeker,
            Hider
        }
        
        [SerializeField] private RoleType roleType;
        public RoleType Role => roleType;
    }
}