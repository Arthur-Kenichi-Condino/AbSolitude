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
    }
}