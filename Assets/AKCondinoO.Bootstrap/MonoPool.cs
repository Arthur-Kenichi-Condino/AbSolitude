using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal class MonoPool<T>where T:PoolableMonoBehaviour{
     private readonly Queue<T>pool=new();
     private readonly T prefab;
     private readonly Transform parent;
        internal MonoPool(T prefab,Transform parent=null){
         this.prefab=prefab;
         this.parent=parent;
        }
        internal T Rent(){
         if(pool.Count>0){
          T item=pool.Dequeue();
          item.OnRent();
          return item;
         }
         T instance=GameObject.Instantiate(prefab,parent);
         instance.OnRent();
         return instance;
        }
        internal void Return(T item){
         item.OnPool();
         pool.Enqueue(item);
        }
        internal void Destroy(bool destroy=false){
         if(destroy){
          while(pool.Count>0){
           T item=pool.Dequeue();
           GameObject.Destroy(item.gameObject);
          }
         }
         pool.Clear();
        }
    }
    internal abstract class PoolableMonoBehaviour:MonoBehaviour{
     internal virtual void OnRent(){}
     internal virtual void OnPool(){}
    }
}