#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Animations;
using static AKCondinoO.Sims.Actors.BaseAI;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     [NonSerialized]internal readonly Dictionary<Type,Dictionary<ulong,SimInventory>>inventory=new Dictionary<Type,Dictionary<ulong,SimInventory>>();
     [NonSerialized]internal readonly HashSet<SimInventoryItem>inventoryItemsToSpawn=new HashSet<SimInventoryItem>();
     [NonSerialized]internal SpawnData inventoryItemsSpawnData;
        internal bool AddToInventory(SimObject simObject,bool addingList=false){
         //Log.DebugMessage("SimObject:AddToInventory");
         if(InventoryContains(simObject,out(SimInventory simInventory,SimInventoryItem asInventoryItem)?containerData)){
          Log.DebugMessage("SimObject:AddToInventory:'InventoryContains returned true'");
          Log.Warning("'TO DO: 'materialize and remove' (add to simInventoryItemPool) asInventoryItem SimObject on fail to add, if needed, or refresh settings and set properly as inventory item, or replace the item with another one to be added'");
         }else{
          //Log.DebugMessage("SimObject:AddToInventory:'InventoryContains returned false'");
          if(inventory.TryGetValue(typeof(SimHands),out Dictionary<ulong,SimInventory>simHandsInventories)){
           foreach(var simInventory in simHandsInventories){
            if(simInventory.Value is SimHands simHandsInventory){
             //Log.DebugMessage("'simHandsInventory: try to add simObject'");
             if(simHandsInventory.Add(simObject,out SimInventoryItemsInContainerSettings.InContainerSettings settings,!addingList)){
              //Log.DebugMessage("'simObject added to simHandsInventory: mark simObject to be saved as an inventory item'");
              OnAddedToInventory(simHandsInventory,settings);//  change simObject to act as an inventory item
              return true;
             }
            }
           }
          }
         }
         return false;
         void OnAddedToInventory(SimInventory simInventory,SimInventoryItemsInContainerSettings.InContainerSettings settings){
          simObject.ChangeInteractionsToActAsInventoryItem(simInventory,settings);
          OnInventoryItemAdded(simInventory,settings,simObject);
         }
        }
        protected virtual void OnInventoryItemAdded(SimInventory simInventory,SimInventoryItemsInContainerSettings.InContainerSettings settings,SimObject simObjectAdded){
         //Log.DebugMessage(this+":OnInventoryItemAdded:simInventory:"+simInventory);
         if(simInventory is SimHands simHands){
          //Log.DebugMessage(this+":OnInventoryItemAdded:simObjectAdded:"+simObjectAdded);
          if(simObjectAdded is SimWeapon simWeapon){
           if(settings.handsUsage==BaseAI.HandsUsage.TwoHanded){
            SetCurrentToolsOrWeapons(simWeapon,simWeapon);
           }else{
            SetCurrentToolsOrWeapons(simWeapon,null);
           }
          }
         }
        }
        internal void RemoveFromInventory(SimObject simObject,SimInventory simInventory,bool delete=true,bool removingList=false){
         simInventory.Remove(simObject.asInventoryItem,delete,!removingList);
         if(!delete){
          simObject.ChangeInteractionsToActAsNonInventorySimObject(simInventory);
         }
         OnInventoryItemRemoved(simInventory,simObject);
        }
        protected virtual void OnInventoryItemRemoved(SimInventory simInventory,SimObject simObjectRemoved){
         RemoveFromCurrentToolsOrWeapons(simObjectRemoved);
        }
        internal bool InventoryContains(SimObject simObject,out(SimInventory simInventory,SimInventoryItem asInventoryItem)?containerData){
         containerData=null;
         if(simObject.asInventoryItem==null){
          return false;
         }
         foreach(var inventoryTypeDictionary in inventory){
          foreach(var simInventory in inventoryTypeDictionary.Value){
           if(simInventory.Value.Contains(simObject)){
            containerData=(simInventory.Value,simObject.asInventoryItem);
            return true;
           }
          }
         }
         return false;
        }
     [NonSerialized]internal readonly HashSet<(SimObject forAction1,SimObject forAction2)>itemsInToolbar=new HashSet<(SimObject,SimObject)>();
      [NonSerialized]internal readonly Dictionary<int,(SimObject forAction1,SimObject forAction2)>itemsByToolbarSlot=new Dictionary<int,(SimObject,SimObject)>();
       [NonSerialized]internal(SimObject forAction1,SimObject forAction2)?itemsEquipped=null;
        protected virtual void SetCurrentToolsOrWeapons(SimObject forAction1,SimObject forAction2){
         Log.Warning("'TO DO: put in toolbar slots (only if available); and only equip one item pair, selected by its toolbar slot'");
         itemsInToolbar.Add((forAction1,forAction2));
         itemsEquipped=(forAction1,forAction2);
         //Log.DebugMessage(this+":itemsEquipped:"+itemsEquipped);
        }
        protected virtual void RemoveFromCurrentToolsOrWeapons(SimObject simObject){
         itemsInToolbar.RemoveWhere(
          (itemPair)=>{
           return(
            (itemPair.forAction1==null&&itemPair.forAction2==null)||
            (itemPair.forAction1!=null&&itemPair.forAction1==simObject)||
            (itemPair.forAction2!=null&&itemPair.forAction2==simObject)
           );
          }
         );
         if(itemsEquipped!=null){
          if(
           (itemsEquipped.Value.forAction1==null&&itemsEquipped.Value.forAction2==null)||
           (itemsEquipped.Value.forAction1!=null&&itemsEquipped.Value.forAction1==simObject)||
           (itemsEquipped.Value.forAction2!=null&&itemsEquipped.Value.forAction2==simObject)
          ){
           itemsEquipped=null;
          }
         }
        }
    }
}