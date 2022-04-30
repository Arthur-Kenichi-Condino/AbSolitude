using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SpawnData{
     internal bool dequeued=true;
     internal readonly List<(Vector3 position,Vector3 rotation,Vector3 scale,Type type,ulong?id)>at;
        internal SpawnData(){
         at=new List<(Vector3,Vector3,Vector3,Type,ulong?)>(1);
        }
        internal SpawnData(int capacity){
         at=new List<(Vector3,Vector3,Vector3,Type,ulong?)>(capacity);
        }
    }
}