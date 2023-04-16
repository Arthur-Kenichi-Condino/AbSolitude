#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventory{
     internal PersistentSimInventoryData persistentSimInventoryData;
        internal struct PersistentSimInventoryData{
         public(Type simType,ulong number)asSimObjectId;
         public(Type simInventoryType,ulong number)simInventoryId;
         public ListWrapper<SimInventoryItemData>inventoryItems;
            public struct SimInventoryItemData{
             public Type simType;public ulong number;public int id;
            }
            internal void UpdateData(SimInventory simInventory){
             simInventoryId=simInventory.simInventoryId.Value;
              asSimObjectId=simInventory. asSimObjectId.Value;
             inventoryItems=new ListWrapper<SimInventoryItemData>(simInventory.idsItems.Where(kvp=>kvp.Value.simObject!=null&&kvp.Value.simObject.id!=null).Select(kvp=>{return new SimInventoryItemData{simType=kvp.Value.simObject.id.Value.simType,number=kvp.Value.simObject.id.Value.number,id=kvp.Key};}).ToList());
            }
         private static readonly ConcurrentQueue<StringBuilder>stringBuilderPool=new ConcurrentQueue<StringBuilder>();
            public override string ToString(){
             if(!stringBuilderPool.TryDequeue(out StringBuilder stringBuilder)){
              stringBuilder=new StringBuilder();
             }
             stringBuilder.Clear();
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"simInventoryId={0} , ",simInventoryId);
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"asSimObjectId={0} , ",asSimObjectId);
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"inventoryItems={{ ");
             inventoryItems.Reset();
             while(inventoryItems.MoveNext()){
              SimInventoryItemData inventoryItem=inventoryItems.Current;
              stringBuilder.AppendFormat(CultureInfoUtil.en_US,"[{0},{1},{2}], ",inventoryItem.simType,inventoryItem.number,inventoryItem.id);
             }
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} , ");
             string result=string.Format(CultureInfoUtil.en_US,"persistentSimInventoryData={{ {0}, }}",stringBuilder.ToString());
             stringBuilderPool.Enqueue(stringBuilder);
             return result;
            }
        }
     internal LinkedListNode<SimInventory>pooled; 
     internal(Type simInventoryType,ulong number)?simInventoryId;
     internal SimObject asSimObject;
     internal(Type simType,ulong number)?asSimObjectId;
     internal readonly int maxItemsCount;
     internal readonly ConcurrentBag<int>openIds;
     internal readonly Dictionary<int,SimInventoryItem>idsItems;
     internal readonly HashSet<SimInventoryItem>items;
        internal SimInventory(int maxItemsCount){
         openIds=new ConcurrentBag<int>();
         idsItems=new Dictionary<int,SimInventoryItem>(maxItemsCount);
         items=new HashSet<SimInventoryItem>(maxItemsCount);
         simInventoryItemPool=new Queue<SimInventoryItem>(maxItemsCount);
         for(int id=0;id<maxItemsCount;id++){
          openIds.Add(id);
         }
         this.maxItemsCount=maxItemsCount;
        }
        internal void OnAssign((Type simInventoryType,ulong number)simInventoryId,SimObject asSimObject){
         asSimObject.inventory[simInventoryId.simInventoryType].Add(simInventoryId.number,this);
         this.asSimObject  =asSimObject;
         this.asSimObjectId=asSimObject.id.Value;
         SimInventoryManager.singleton.spawned.Add(simInventoryId,this);
         SimInventoryManager.singleton.active .Add(simInventoryId,this);
         this.simInventoryId=simInventoryId;
         Log.DebugMessage("assigned SimInventory of size:"+maxItemsCount+" for asSimObject:"+asSimObject);
         persistentSimInventoryData.UpdateData(this);
        }
        internal virtual void Reset(bool clear=false){
         Log.DebugMessage("SimInventory Reset");
         if(asSimObjectId!=asSimObject.id.Value){
          Log.DebugMessage("asSimObjectId!=asSimObject.id.Value");
          this.asSimObjectId=asSimObject.id.Value;
         }
         if(clear){
          Clear();
         }
         persistentSimInventoryData.UpdateData(this);
        }
     internal readonly Queue<SimInventoryItem>simInventoryItemPool;
        internal virtual void Clear(){
        }
        internal virtual void Remove(SimObject simObject){
        }
        internal virtual bool Add(SimObject simObject,out SimInventoryItemsInContainerSettings.SimObjectSettings settings,bool updatePersistentData=true){
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
          OnAddedSimInventoryItem();
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
         OnAddedSimInventoryItem();
         return(true);//  added successfully
         void OnAddedSimInventoryItem(){
          if(updatePersistentData){persistentSimInventoryData.UpdateData(this);}
         }
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