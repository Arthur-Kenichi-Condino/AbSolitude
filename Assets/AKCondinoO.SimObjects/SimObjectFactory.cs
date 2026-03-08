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
         T simObject=pool.Rent();
         simObject.simObjectType=item.simObjectType;
         SimObjectManager.singleton.instancedRendering.AddInstance(simObject.simObjectType,simObject);
         simObject.OnPositionChanged(out bool outOfBounds);
         if(outOfBounds){
          simObject=null;
         }
         return simObject;
        }
        internal virtual void Despawn(T simObject){
         SimObjectManager.singleton.instancedRendering.RemoveInstance(simObject.simObjectType,simObject.instancedRenderingIndex);
         pool.Return(simObject);
        }
    }
}