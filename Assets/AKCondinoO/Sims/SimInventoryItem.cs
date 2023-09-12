#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventoryItem{
     internal SimInventory container;
     internal SimObject simObject;
     internal readonly HashSet<int>slotIdsUsed=new HashSet<int>();
     internal SimInventoryItemsInContainerSettings.SimObjectSettings settings;
        internal void SetAsInventoryItem(SimInventory inventory,SimObject simObject,SimInventoryItemsInContainerSettings.SimObjectSettings settings,int spaces){
         Log.DebugMessage("SetAsInventoryItem:"+simObject);
         this.settings=settings;
         if(simObject.asInventoryItem!=null&&simObject.asInventoryItem.container!=null){
          Log.DebugMessage("SetAsInventoryItem:Remove simObject from old inventory");
          UnsetAsInventoryItem(simObject.asInventoryItem.container);
         }
         container=inventory;
         slotIdsUsed.Clear();
         for(int i=0;i<spaces;++i){
          if(inventory.openSlotIds.TryTake(out int slotId)){
           inventory.itemIdsBySlotIds.Add(slotId,simObject.id.Value);
            inventory.itemsBySlotIds.Add(slotId,this);
           slotIdsUsed.Add(slotId);
          }
         }
         inventory.items.Add(this);
         this.simObject=simObject;
         simObject.asInventoryItem=this;
        }
        internal void UnsetAsInventoryItem(SimInventory inventory){
         simObject.asInventoryItem=null;
         simObject=null;
         inventory.items.Remove(this);
         foreach(int slotId in slotIdsUsed){
          inventory.itemIdsBySlotIds.Remove(slotId);
           inventory.itemsBySlotIds.Remove(slotId);
          inventory.openSlotIds.Add(slotId);
         }
         slotIdsUsed.Clear();
         container=null;
        }
    }
}