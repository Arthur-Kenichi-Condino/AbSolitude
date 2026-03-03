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
         Matrix4x4 matrix=gameObject.transform.localToWorldMatrix;
         SimObjectManager.singleton.instancedRendering.AddInstance(item.simObjectType,matrix);
         return simObject;
        }
        internal virtual void Despawn(T simObject){
         pool.Return(simObject);
        }
    }
}