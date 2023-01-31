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
         for(int id=0;id<maxItemsCount;id++){
          openIds.Add(id);
         }
         this.maxItemsCount=maxItemsCount;
         Log.DebugMessage("created SimInventory of size:"+maxItemsCount+"and owner:"+owner);
        }
     internal readonly Queue<SimInventoryItem>simInventoryItemPool;
        internal virtual void Add(SimObject simObject){
         SimInventoryItem simInventoryItem=null;
         if(simInventoryItemPool.Count>0){
          Log.DebugMessage("use simInventoryItemPool");
          simInventoryItem=simInventoryItemPool.Dequeue();
         }else{
          Log.DebugMessage("simInventoryItemPool is empty");
          simInventoryItem=new SimInventoryItem();
         }
         simInventoryItem.SetAsInventoryItem(simObject);
        }
        internal virtual void Clear(){
         Log.DebugMessage("SimInventory Clear");
        }
    }
}