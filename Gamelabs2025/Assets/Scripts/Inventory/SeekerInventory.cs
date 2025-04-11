using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Player.Inventory
{
   public class SeekerInventory : NetworkBehaviour
   {
      [SerializeField] private GameObject inventoryGuiPrefab;
      [SerializeField] private List<InventoryItemContainer> items = new List<InventoryItemContainer>(3);
      
      private int selectedItem = -1;
      private ISeekerAttachable activeItem;

      public Action<NetworkObject> OnAttachableSpawned;
      private SeekerInventoryGui guiRef;

      private Coroutine delayedToggleRoutine;

      public bool HasStorage
      {
         get
         {
            return GetEmptySlot(out int id);
         }
      }

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
         
         var availableItems = items.FindAll(a => !a.IsSlotEmpty);
         if(availableItems.Count < 2)
            return;

         if (availableItems.Count >= items.Count)
         {
            var id = 1 + (selectedItem + 1) % items.Count;
            DelayedEquip(id);
            return;
         }

         var selectedSlot = items[selectedItem];
         var converted = availableItems.IndexOf(selectedSlot);
         
         converted = (converted + 1) % availableItems.Count;
         var convertedItem = availableItems[converted];

         DelayedEquip(items.IndexOf(convertedItem) + 1);
      }

      private void DelayedEquip(int id)
      {
         if(delayedToggleRoutine != null)
            StopCoroutine(delayedToggleRoutine);

         delayedToggleRoutine = StartCoroutine(DelayedEquipRoutine(id));
      }
      
      private IEnumerator DelayedEquipRoutine(int id)
      {
         yield return new WaitForSeconds(0.15f);
         Equip(id);
         delayedToggleRoutine = null;
      }
      
      private void OnDestroy()
      {
         OnAttachableSpawned += ClientOnAttachableSpawnedOnServer;
         InputReader.Instance.OnEquipInventoryItemEvent -= Equip;
         InputReader.Instance.OnToggleEquippedItemEvent -= ToggleEquippedItem;
      }
      
      private bool GetEmptySlot(out int availableSlotId)
      {
         for (int i = 0; i < items.Count; i++)
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
         if (GetEmptySlot(out int availableSlotId)) //availableSlotId is zero-indexed
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
         selectedItem = -1;
         
         //Equip other inventory item.
         for (int i = 0; i < items.Count; i++)
         {
            if (items[i] != null && !items[i].IsSlotEmpty)
            {
               Debug.Log($"SeekerInventory:::Equip-ing other item in inventory {i}");
               Equip(i+1);
               break;
            }
         }
      }

      /// <summary>
      /// Equips Item.
      /// </summary>
      /// <param name="id">Starts from 1</param>
      private void Equip(int id)
      {
         //don't go past the max limit
         if (id - 1 > items.Count)
            return;

         if(id - 1 == selectedItem)
            return;
         
         // Ignore if empty slot
         if (items[id - 1] == null || items[id - 1].IsSlotEmpty)
         {
            Debug.Log("SeekerInventory:::Equip Failed (ITEM NULL OR ITEM.SPAWN_ITEM NULL)");
            return;
         }

         if (activeItem != null)
            Detach(activeItem, false);

         selectedItem = (int)id - 1;
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
         
         if (IsOwner)
         {
            InScreenUI.Instance.ShowInputPrompt(InputReader.Instance.inputMap.Gameplay.UseItem, seekerAttachable.GetUsePromptText());
            InScreenUI.Instance.ShowInputPrompt(InputReader.Instance.inputMap.Gameplay.Drop, "Drop");
         }
      }

      private void Detach(ISeekerAttachable attachable, bool removed)
      {
         attachable?.OnDetach(this.transform, removed);
         if (IsOwner)
         {
            InScreenUI.Instance.RemoveInputPrompt(InputReader.Instance.inputMap.Gameplay.UseItem);
            InScreenUI.Instance.RemoveInputPrompt(InputReader.Instance.inputMap.Gameplay.Drop);
         }
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

