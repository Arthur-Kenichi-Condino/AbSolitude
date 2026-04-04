using AKCondinoO.Bootstrap;
using AKCondinoO.World;
using System;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObject:MonoBehaviour{
     internal Type simObjectType=>type??=GetType();private Type type;
     [SerializeField]internal string variant;
     [SerializeField]internal bool useInstancedRendering=false;
     [SerializeField]internal GameObject meshPrefab;
     [SerializeField]internal int[]useMeshObjectSubMeshesForCollider;
     internal ulong id;
     internal int instancedRenderingIndex=-1;
     internal MeshRenderer simObjectMeshRenderer;
     internal MeshFilter   simObjectMeshFilter;
     internal MeshCollider simObjectMeshCollider;
     internal bool isGrounded;
        internal virtual void Awake(){
        }
        internal void OnChunkPooled(){
        }
        internal virtual void ManualUpdate(){
         OnUpdate();
        }
        internal virtual void LazyUpdate(){
         //Logs.Debug(()=>"'lazy update data'");
         if(this==null){
          return;
         }
         if(id==0){
          return;
         }
         OnUpdate();
        }
        internal virtual void OnUpdate(){
         ValidateTransform();
        }
        internal virtual void ValidateTransform(){
         if(transform.hasChanged){
          //Logs.Debug(()=>"'transform.hasChanged'");
          OnPositionChanged(out bool outOfBounds);
          transform.hasChanged=false;
         }
        }
        internal void OnPositionChanged(out bool outOfBounds){
         var manager=SimObjectManager.singleton;
         outOfBounds=false;
         if(instancedRenderingIndex>=0){
          manager.instancedRendering.UpdateInstance((simObjectType,variant),instancedRenderingIndex);
         }
         UpdateGroundedState();
        }
        internal virtual void UpdateGroundedState(){
         isGrounded=true;
        }
    }
}