using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.World.Spawning{
    internal class ByChanceObjectSpawnEntry<T>where T:class{
     internal T prefab;
     internal float chance;
     internal int priority;
    }
    internal class ByChancePicker<T>where T:class{
     internal static readonly Utilities.ObjectPool<ByChanceObjectSpawnEntry<T>>entryPool=
      Pool.GetPool<ByChanceObjectSpawnEntry<T>>(
       "",
       ()=>new(),
       (ByChanceObjectSpawnEntry<T>item)=>{
        item.prefab=null;
       }
      );
     internal readonly List<ByChanceObjectSpawnEntry<T>>items=new();
     private readonly List<ByChanceObjectSpawnEntry<T>>filteredItems=new();
     private ByChanceObjectSpawnEntry<T>[]lookupTable;
     private float maxWeight;
     private int resolution;
        internal void Build(int resolution=100_000,bool fill=false){
         this.resolution=resolution;
         if(items.Count<=0){
          lookupTable=Pool.RentArray<ByChanceObjectSpawnEntry<T>>(0);
          maxWeight=0f;
          return;
         }
         filteredItems.Clear();
         foreach(var item in items){
          if(item.chance>0f)filteredItems.Add(item);
         }
         if(filteredItems.Count<=0){
          lookupTable=Pool.RentArray<ByChanceObjectSpawnEntry<T>>(0);
          maxWeight=0f;
          return;
         }
         filteredItems.Sort((a,b)=>{
          int cmp=b.chance.CompareTo(a.chance);
          return cmp!=0?cmp:b.priority.CompareTo(a.priority);
         });
         float total=0f;
         if(fill){foreach(var item in filteredItems)total+=item.chance;}else{total=1f;}
         maxWeight=filteredItems[0].chance;
         lookupTable=Pool.RentArray<ByChanceObjectSpawnEntry<T>>(resolution);
         float cumulative=0f;
         for(int i=0;i<filteredItems.Count;i++){
          var obj=filteredItems[i];
          int startIdx=(int)(cumulative/total*resolution);
          cumulative+=obj.chance;
          int endIdx=(int)(Math.Min(cumulative/total,1f)*resolution);
          for(int idx=startIdx;idx<endIdx;idx++){
           lookupTable[idx]=obj;
          }
         }
        }
        internal void Clear(){
         Pool.ReturnArray(lookupTable,true);
         lookupTable=null;
         foreach(var item in items){
          entryPool.Return(item);
         }
         items.Clear();
         filteredItems.Clear();
        }
        internal bool Get(float value,out ByChanceObjectSpawnEntry<T>result){
         result=null;
         if(value<0f||value>1f)return false;
         int idx=(int)(value*resolution);
         if(idx>=lookupTable.Length)return false;
         if(idx>=resolution)idx=resolution-1;
         result=lookupTable[idx];
         if(result==null)return false;
         return true;
        }
    }
}