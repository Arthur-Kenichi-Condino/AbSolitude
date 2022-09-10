using AKCondinoO.Sims.Actors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SpawnData{
     internal bool dequeued=true;
     internal readonly List<(Vector3 position,Vector3 rotation,Vector3 scale,Type type,ulong?id,SimObject.PersistentData persistentData)>at;
      internal readonly Dictionary<int,SimActor.PersistentSimActorData>actorData;
        internal SpawnData(){
         at=new List<(Vector3,Vector3,Vector3,Type,ulong?,SimObject.PersistentData)>(1);
         actorData=new Dictionary<int,SimActor.PersistentSimActorData>(1);
        }
        internal SpawnData(int capacity){
         at=new List<(Vector3,Vector3,Vector3,Type,ulong?,SimObject.PersistentData)>(capacity);
         actorData=new Dictionary<int,SimActor.PersistentSimActorData>(capacity);
        }
        internal void Clear(){
         at.Clear();
         actorData.Clear();
        }
    }
}