using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObjectPool<T>where T:SimObject{
     private readonly Stack<T>pool=new();
     private readonly T prefab;
     private readonly Transform parent;
        internal SimObjectPool(T prefab,Transform parent=null){
         this.prefab=prefab;
         this.parent=parent;
        }
        internal T Rent(){
         if(pool.Count>0){
          T item=pool.Pop();
          return item;
         }
         return GameObject.Instantiate(prefab,parent);
        }
        internal void Return(T item){
         pool.Push(item);
        }
        internal void Destroy(bool destroy=false){
         if(destroy){
          while(pool.Count>0){
           T item=pool.Pop();
           GameObject.Destroy(item.gameObject);
          }
         }
         pool.Clear();
        }
    }
}