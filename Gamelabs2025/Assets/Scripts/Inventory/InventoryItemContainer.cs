using FishNet.Object;
using UnityEngine;

namespace Player.Inventory
{
    [System.Serializable]
    public class InventoryItemContainer
    {
        public NetworkObject ItemToSpawn;
        public Sprite ItemSprite;
        
        public bool IsSlotEmpty => ItemToSpawn == null;
    }
}