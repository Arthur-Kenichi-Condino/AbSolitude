using AKCondinoO.Bootstrap;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObjectPool<T>where T:SimObject{
     private readonly MonoPool<T>pool;
     private readonly T prefab;
     private readonly Transform parent;
        internal SimObjectPool(T prefab,Transform parent=null){
         this.prefab=prefab;
         this.parent=parent;
         pool=new(prefab,parent);
        }
        internal T Rent(){
         T item=pool.Rent();
         return item;
        }
        internal void Return(T item){
         pool.Return(item);
        }
        internal void Destroy(bool destroy=false){
         pool.Destroy(destroy);
        }
    }
}