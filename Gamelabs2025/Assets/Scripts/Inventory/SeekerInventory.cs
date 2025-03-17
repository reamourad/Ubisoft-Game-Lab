using System;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Player.Inventory
{
   public class SeekerInventory : NetworkBehaviour
   {
      [SerializeField] private GameObject inventoryGuiPrefab;
      [SerializeField] private InventoryItemContainer[] items = new InventoryItemContainer[2];
      
      private uint selectedItem = 0;
      private ISeekerAttachable activeItem;

      public Action<NetworkObject> OnAttachableSpawned;

      [Header("Testing...")] [SerializeField]
      private NetworkObject testVacuumObject;

      [SerializeField] private NetworkObject testTabletObject;
      [SerializeField] private NetworkObject testThermometerObject;
      
      private SeekerInventoryGui guiRef;

      public bool HasItemEquipped => activeItem != null;
      
      public override void OnStartNetwork()
      {
         base.OnStartNetwork();
         OnAttachableSpawned += ClientOnAttachableSpawnedOnServer;
      }

      public override void OnStartClient()
      {
         base.OnStartClient();
         
         if (IsOwner)
         {
            InputReader.Instance.OnEquipInventoryItemEvent += Equip;
            InputReader.Instance.OnToggleEquippedItemEvent += ToggleEquippedItem;
            guiRef = Instantiate(inventoryGuiPrefab).GetComponent<SeekerInventoryGui>();
         }
      }

      private void ToggleEquippedItem()
      {
         var newId = 1 + (selectedItem + 1) % items.Length;
         Equip((uint)newId);
      }

      private void OnDestroy()
      {
         InputReader.Instance.OnEquipInventoryItemEvent -= Equip;
      }
      
      private bool GetEmptySlot(out uint availableSlotId)
      {
         for (uint i = 0; i < items.Length; i++)
         {
            if (items[i] != null && items[i].IsSlotEmpty)
            {
               availableSlotId = i;
               return true;
            }
         }

         availableSlotId = 0;
         return false;
      }

      /// <summary>
      /// Add item to inventory and equip it.
      /// </summary>
      /// <param name="item"></param>
      public void AddToInventory(NetworkObject item, Sprite icon)
      {
         Debug.Log("SeekerInventory::::AddToInventory");
         if (GetEmptySlot(out uint availableSlotId)) //availableSlotId is zero-indexed
         {
            Debug.Log($"SeekerInventory::::AddToInventory At Slot {availableSlotId}");
            items[availableSlotId] = new InventoryItemContainer()
            {
               ItemToSpawn = item,
               ItemSprite = icon
            };

            guiRef.SetItemData((int)availableSlotId, icon);
            //just to hide the object, once added to inventory
            Equip(availableSlotId+1); //+1 since I went and made it a 1-indexed thing like an idiot
         }
         else
         {
            Debug.Log("SeekerInventory:::No Empty Slots");
         }
      }

      /// <summary>
      /// Removes item from inventory
      /// </summary>
      /// <param name="id">Starts from 1</param>
      public void RemoveActiveItem()
      {
         if(activeItem == null)
            return;
         
         Debug.Log($"SeekerInventory:::Removing Item {selectedItem}");
         guiRef.RemoveItemData((int)selectedItem);
         //Remove data
         items[selectedItem].ItemToSpawn = null;
         items[selectedItem].ItemSprite = null;
         
         Detach(activeItem, true); 
         activeItem = null;
         selectedItem = 0;
         
         //Equip other inventory item.
         for (int i = 0; i < items.Length; i++)
         {
            if (items[i] != null && !items[i].IsSlotEmpty)
            {
               Debug.Log("SeekerInventory:::Equip-ing other item in inventory");
               Equip((uint)i);
               break;
            }
         }
      }

      /// <summary>
      /// Equips Item.
      /// </summary>
      /// <param name="id">Starts from 1</param>
      private void Equip(uint id)
      {
         //don't go past the max limit
         if (id - 1 > items.Length)
            return;

         // Ignore if empty slot
         if (items[id - 1] == null || items[id - 1].IsSlotEmpty)
         {
            Debug.Log("SeekerInventory:::Equip Failed (ITEM NULL OR ITEM.SPAWN_ITEM NULL)");
            return;
         }

         if (activeItem != null)
            Detach(activeItem, false);

         selectedItem = id - 1;
         guiRef.Equip((int)selectedItem); 
         RPC_RequestSpawnAttachable(items[id - 1].ItemToSpawn, OwnerId);
         //of-course my stupid brain made this zero-indexed, I shouldn't code at 3AM
      }

      private void Attach(NetworkObject networkObject)
      {
         //parent the object within, later move this designated locators.
         var seekerAttachable = networkObject.GetComponentInChildren<ISeekerAttachable>();
         if (seekerAttachable == null)
         {
            Debug.Log("SeekerInventory (Attach)::::Seeker Attachable NULL!!!");
            return;
         }
         activeItem = seekerAttachable;
         activeItem.OnAttach(this.transform);
      }

      private void Detach(ISeekerAttachable attachable, bool removed)
      {
         attachable?.OnDetach(this.transform, removed);
      }
      
      [ServerRpc]
      private void RPC_RequestSpawnAttachable(NetworkObject networkObject, int connectionId)
      {
         var netManager = InstanceFinder.NetworkManager;
         var connection = InstanceFinder.ServerManager.Clients[connectionId];
         NetworkObject nob = netManager.GetPooledInstantiated(networkObject, Vector3.zero, Quaternion.identity, this.transform, true);
         netManager.ServerManager.Spawn(nob, connection);
         var seekerAttachable = nob.GetComponentInChildren<ISeekerAttachable>();
         if (seekerAttachable != null)
         {
            seekerAttachable.OnAttach(this.transform);
            //forward spawn complete message to clients.
            RPC_InformClientsOfAttachableSpawn(nob.ObjectId);
         }
      }
      
      [ObserversRpc]
      private void RPC_InformClientsOfAttachableSpawn(int objectId)
      {
         var spawned = InstanceFinder.ClientManager.Objects.Spawned[objectId];
         OnAttachableSpawned?.Invoke(spawned);
      }
      
      [Client]
      private void ClientOnAttachableSpawnedOnServer(NetworkObject spawned)
      {
         Attach(spawned);
      }
   }
}

