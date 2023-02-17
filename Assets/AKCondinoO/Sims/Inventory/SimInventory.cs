#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventory{
        internal struct PersistentSimInventoryData{
         public ListWrapper<SimInventoryItemData>inventoryItems;
            public struct SimInventoryItemData{
             public Type simType;public ulong number;
            }
            internal void UpdateData(SimInventory simInventory){
            }
        }
     internal readonly SimObject owner;
     internal(Type simType,ulong number)ownerId;
     internal readonly int maxItemsCount;
     internal readonly ConcurrentBag<int>openIds;
     internal readonly Dictionary<int,SimInventoryItem>idsItems;
     internal readonly HashSet<SimInventoryItem>items;
        internal SimInventory(SimObject owner,int maxItemsCount){
         openIds=new ConcurrentBag<int>();
         idsItems=new Dictionary<int,SimInventoryItem>(maxItemsCount);
         items=new HashSet<SimInventoryItem>(maxItemsCount);
         simInventoryItemPool=new Queue<SimInventoryItem>(maxItemsCount);
         this.owner=owner;
         this.ownerId=owner.id.Value;
         for(int id=0;id<maxItemsCount;id++){
          openIds.Add(id);
         }
         this.maxItemsCount=maxItemsCount;
         Log.DebugMessage("created SimInventory of size:"+maxItemsCount+"and owner:"+owner);
        }
        internal virtual void Reset(bool clear=false){
         Log.DebugMessage("SimInventory Reset");
         if(ownerId!=owner.id.Value){
          Log.DebugMessage("ownerId!=owner.id.Value");
          this.ownerId=owner.id.Value;
         }
         if(clear){
          Clear();
         }
        }
     internal readonly Queue<SimInventoryItem>simInventoryItemPool;
        internal virtual void Clear(){
        }
        internal virtual void Remove(SimObject simObject){
        }
        internal virtual bool Add(SimObject simObject,out SimInventoryItemsInContainerSettings.SimObjectSettings settings){
         int spaces=0;
         if(SimObjectSpawner.singleton.simInventoryItemsInContainerSettings.allSettings.TryGetValue(simObject.GetType(),out settings)){
          if(!settings.inventorySpaces.TryGetValue(this.GetType(),out spaces)){
           Log.DebugMessage("SimObject doesn't have a valid SimInventoryItemsSettings.SimObjectSettings for this SimInventory:"+this.GetType());
           return(false);
          }
         }
         if(spaces<=0){
          Log.DebugMessage("SimObject uses 0 spaces in this SimInventory:"+this.GetType()+", which is not allowed");
          return(false);
         }
         if(openIds.Count<spaces){
          Log.DebugMessage("not enough space in the inventory");
          return(false);
         }
         if(simObject.asInventoryItem!=null){
          Log.DebugMessage("simObject is already an inventory item");
          simObject.asInventoryItem.SetAsInventoryItem(this,simObject,settings,spaces);
          return(true);//  moved to this inventory
         }
         SimInventoryItem simInventoryItem=null;
         if(simInventoryItemPool.Count>0){
          Log.DebugMessage("use simInventoryItemPool");
          simInventoryItem=simInventoryItemPool.Dequeue();
         }else{
          Log.DebugMessage("simInventoryItemPool is empty");
          simInventoryItem=new SimInventoryItem();
         }
         simInventoryItem.SetAsInventoryItem(this,simObject,settings,spaces);
         return(true);//  added successfully
        }
        internal bool Contains(SimObject simObject){
         if(simObject.asInventoryItem!=null){
          if(simObject.asInventoryItem.container==this){
           return true;
          }
         }else{
         }
         return false;
        }
        internal void Materialize((Type simType,ulong number)id){
         Log.DebugMessage("Materialize inventory item as simObject");
        }
    }
}