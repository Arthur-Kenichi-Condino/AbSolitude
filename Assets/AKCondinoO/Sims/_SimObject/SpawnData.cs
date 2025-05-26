using AKCondinoO.Sims.Actors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SpawnData{
     internal bool dequeued=true;
     internal readonly List<(Vector3 position,Vector3 rotation,Vector3 scale,Type simObjectType,ulong?idNumber,SimObject.PersistentData persistentData)>at;
      internal readonly Dictionary<int,SimActor.PersistentSimActorData>actorData;
       internal readonly Dictionary<int,(Type simObjectType,ulong idNumber)>masters;
        internal readonly Dictionary<int,(Type simObjectType,ulong idNumber)?>asInventoryItemOwnerIds;
         internal readonly Dictionary<int,SimObject.PersistentStats>statsData;
        internal SpawnData(){
         at=new List<(Vector3,Vector3,Vector3,Type,ulong?,SimObject.PersistentData)>(1);
         actorData=new Dictionary<int,SimActor.PersistentSimActorData>(1);
         masters=new Dictionary<int,(Type simObjectType,ulong idNumber)>(1);
         asInventoryItemOwnerIds=new Dictionary<int,(Type,ulong)?>(1);
         statsData=new Dictionary<int,SimObject.PersistentStats>(1);
        }
        internal SpawnData(int capacity){
         at=new List<(Vector3,Vector3,Vector3,Type,ulong?,SimObject.PersistentData)>(capacity);
         actorData=new Dictionary<int,SimActor.PersistentSimActorData>(capacity);
         masters=new Dictionary<int,(Type simObjectType,ulong idNumber)>(capacity);
         asInventoryItemOwnerIds=new Dictionary<int,(Type,ulong)?>(capacity);
         statsData=new Dictionary<int,SimObject.PersistentStats>(capacity);
        }
        internal void Clear(){
         at.Clear();
         actorData              .Clear();
         masters                .Clear();
         asInventoryItemOwnerIds.Clear();
         statsData              .Clear();
        }
        internal void RemoveAt(int index){
         if(index>=at.Count){
          Log.Error("SpawnData:RemoveAt:index out of range");
          return;
         }
         at.RemoveAt(index);
         for(int i=index;i<at.Count;++i){
          if(actorData              .TryGetValue(i+1,out var               actorData_next)){actorData              [i]=              actorData_next;}else{actorData              .Remove(i);}
          if(masters                .TryGetValue(i+1,out var                 masters_next)){masters                [i]=                masters_next;}else{masters                .Remove(i);}
          if(asInventoryItemOwnerIds.TryGetValue(i+1,out var asInventoryItemOwnerIds_next)){asInventoryItemOwnerIds[i]=asInventoryItemOwnerIds_next;}else{asInventoryItemOwnerIds.Remove(i);}
          if(statsData              .TryGetValue(i+1,out var               statsData_next)){statsData              [i]=              statsData_next;}else{statsData              .Remove(i);}
         }
         actorData              .Remove(at.Count);
         masters                .Remove(at.Count);
         asInventoryItemOwnerIds.Remove(at.Count);
         statsData              .Remove(at.Count);
        }
    }
}
          /*  Código para testagem
          SpawnData s=new SpawnData(3);
          for(int i=0;i<3;++i){
           s.at.Add((Vector3.zero,Vector3.zero,Vector3.one,null,(ulong)i,new SimObject.PersistentData()));
           SimObject.PersistentStats stats=new SimObject.PersistentStats();
           List<SimObject.PersistentStats.StatsBoolData>bools=new List<SimObject.PersistentStats.StatsBoolData>();
           for(int j=0;j<i;++j){
            bools.Add(new SimObject.PersistentStats.StatsBoolData());
           }
           stats.statsBools=new ListWrapper<SimObject.PersistentStats.StatsBoolData>(bools);
           s.statsData[i]=stats;
          }
          for(int i=2;i>=1;--i){
           Log.DebugMessage("traversing:remove at i:"+i+":idNumber:"+s.at[i].idNumber+":s.at.Count:"+s.at.Count+":s.statsData.Count:"+s.statsData.Count+":s.statsData[i]:"+(s.statsData.ContainsKey(i)?s.statsData[i].statsBools.Count:"False"));
           s.RemoveAt(i);
          }
          //s.RemoveAt(0);
          //s.RemoveAt(1);
          //s.RemoveAt(2);
          for(int i=0;i<s.at.Count;++i){
           Log.DebugMessage("traversing:i:"+i+":idNumber:"+s.at[i].idNumber+":s.at.Count:"+s.at.Count+":s.statsData.Count:"+s.statsData.Count+":s.statsData[i]:"+(s.statsData.ContainsKey(i)?s.statsData[i].statsBools.Count:"False"));
          }
          */