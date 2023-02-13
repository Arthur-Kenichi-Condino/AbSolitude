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
         if(InventoryContains(simObject,out(SimInventory simInventory,SimInventoryItem asInventoryItem)?containerData)){
          Log.DebugMessage("AddToInventory InventoryContains True");
         }else{
          Log.DebugMessage("AddToInventory InventoryContains False");
         }
         return false;
        }
        internal bool InventoryContains(SimObject simObject,out(SimInventory simInventory,SimInventoryItem asInventoryItem)?containerData){
         containerData=null;
         if(simObject.asInventoryItem==null){
          return false;
         }
         foreach(var inventoryTypeList in inventory){
          foreach(SimInventory simInventory in inventoryTypeList.Value){
           if(simInventory.Contains(simObject)){
            containerData=(simInventory,simObject.asInventoryItem);
            return true;
           }
          }
         }
         return false;
        }
    }
}