#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
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
          simObject.asInventoryItem.container.Remove(simObject);
         }
         container=inventory;
         ids.Clear();
         for(int i=0;i<spaces;++i){
          if(inventory.openIds.TryTake(out int id)){
           inventory.idsItems.Add(id,this);
           ids.Add(id);
          }
         }
         inventory.items.Add(this);
         this.simObject=simObject;
         simObject.asInventoryItem=this;
        }
    }
}