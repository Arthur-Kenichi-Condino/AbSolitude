#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     internal SimInventoryItem asInventoryItem=null;
     internal readonly Dictionary<Type,List<SimInventory>>inventory=new Dictionary<Type,List<SimInventory>>();
     internal readonly HashSet<SimInventoryItem>inventoryItemsToSpawn=new HashSet<SimInventoryItem>();
     internal SpawnData inventoryItemsSpawnData;
        internal bool AddToInventory(SimObject simObject){
         Log.DebugMessage("AddToInventory");
         return false;
        }
        internal bool InventoryContains(SimObject simObject){
         if(simObject.asInventoryItem==null){
          return false;
         }
         foreach(var inventoryTypeList in inventory){
          foreach(SimInventory simInventory in inventoryTypeList.Value){
           if(simInventory.Contains(simObject)){
            return true;
           }
          }
         }
         return false;
        }
    }
}