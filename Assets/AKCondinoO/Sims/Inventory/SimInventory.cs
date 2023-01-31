#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventory{
     internal readonly int maxItemsCount;
     internal readonly ConcurrentBag<int>openIds=new ConcurrentBag<int>();
     internal readonly Dictionary<int,SimInventoryItem>idsItems=new Dictionary<int,SimInventoryItem>();
     internal readonly HashSet<SimInventoryItem>items=new HashSet<SimInventoryItem>();
        internal SimInventory(int maxItemsCount){
         for(int id=0;id<maxItemsCount;id++){
          openIds.Add(id);
         }
         this.maxItemsCount=maxItemsCount;
        }
     internal readonly Queue<SimInventoryItem>simInventoryItemPool=new Queue<SimInventoryItem>();
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
    }
}