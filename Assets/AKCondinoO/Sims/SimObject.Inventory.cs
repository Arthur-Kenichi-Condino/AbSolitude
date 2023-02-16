#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
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
          //  TO DO: "materialize" if needed
         }else{
          Log.DebugMessage("AddToInventory InventoryContains False");
          if(inventory.TryGetValue(typeof(SimHands),out List<SimInventory>simHandsInventories)){
           foreach(SimInventory simInventory in simHandsInventories){
            if(simInventory is SimHands simHandsInventory){
             Log.DebugMessage("simHandsInventory try Add");
             if(simHandsInventory.Add(simObject)){
              Log.DebugMessage("added to simHandsInventory:mark simObject to be saved as an inventory item");
              //  TO DO: create local function to save simObjects as an inventory item
              OnAddedToInventory(simHandsInventory);
              return true;
             }
            }
           }
          }
         }
         return false;
         void OnAddedToInventory(SimInventory simInventory){
          simObject.ChangeInteractionsToActAsInventoryItem(simInventory);
         }
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
        internal void ChangeInteractionsToActAsInventoryItem(SimInventory simInventory){
         Log.DebugMessage("ChangeInteractionsToActAsInventoryItem:"+id);
         //
        }
        internal void ChangeInteractionsToActAsNonInventorySimObject(SimInventory simInventory){
        }
    }
}