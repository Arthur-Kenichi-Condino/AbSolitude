#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventoryItem{
     internal SimInventory container;
        internal void SetAsInventoryItem(SimInventory inventory,SimObject simObject){
         Log.DebugMessage("SetAsInventoryItem:"+simObject);
         if(simObject.asInventoryItem!=null&&simObject.asInventoryItem.container!=null){
          Log.DebugMessage("SetAsInventoryItem:Remove simObject from old inventory");
          simObject.asInventoryItem.container.Remove(simObject);
         }
         container=inventory;
         simObject.asInventoryItem=this;
        }
    }
}