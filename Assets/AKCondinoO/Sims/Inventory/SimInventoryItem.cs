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
     internal readonly HashSet<int>ids=new HashSet<int>();
     internal SimInventoryItemsInContainerSettings.SimObjectSettings settings;
        internal void SetAsInventoryItem(SimInventory inventory,SimObject simObject,SimInventoryItemsInContainerSettings.SimObjectSettings settings,int spaces){
         Log.DebugMessage("SetAsInventoryItem:"+simObject);
         this.settings=settings;
         if(simObject.asInventoryItem!=null&&simObject.asInventoryItem.container!=null){
          Log.DebugMessage("SetAsInventoryItem:Remove simObject from old inventory");
          UnsetAsInventoryItem(simObject.asInventoryItem.container);
         }
         container=inventory;
         ids.Clear();
         for(int i=0;i<spaces;++i){
          if(inventory.openIds.TryTake(out int id)){
           inventory.idsItemIds.Add(id,simObject.id.Value);
            inventory.idsItems.Add(id,this);
           ids.Add(id);
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
         foreach(int id in ids){
          inventory.idsItemIds.Remove(id);
           inventory.idsItems.Remove(id);
          inventory.openIds.Add(id);
         }
         ids.Clear();
         container=null;
        }
    }
}