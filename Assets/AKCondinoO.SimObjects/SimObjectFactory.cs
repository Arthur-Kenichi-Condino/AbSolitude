using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObjectFactory<T>where T:SimObject{
     private readonly SimObjectPool<T>pool;
        internal SimObjectFactory(T prefab,Transform parent=null){
         pool=new(prefab,parent);
        }
        internal void Destroy(bool destroy=false){
         pool.Destroy(destroy);
        }
        internal virtual SimObject Spawn(SimObjectSpawn item){
         SimObject simObject=pool.Rent();
         GameObject gameObject=simObject.gameObject;
         return simObject;
        }
        internal virtual void Despawn(T simObject){
         pool.Return(simObject);
        }
    }
}