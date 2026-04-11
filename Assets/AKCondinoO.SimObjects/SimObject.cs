using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors;
using AKCondinoO.World;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.SimObjects{
    internal class SimObject:MonoBehaviour{
     internal Type simObjectType=>type??=GetType();private Type type;
     [SerializeField]internal string variant;
     [SerializeField]internal bool useInstancedRendering=false;
     [SerializeField]internal GameObject meshPrefab;
     [SerializeField]internal int[]useMeshObjectSubMeshesForCollider;
     [SerializeField]internal GameObject simObjectRendererComponents;
     [SerializeField]internal GameObject simObjectCollisionComponents;
     [SerializeField]internal Transform simObjectPartsRoot;
     internal List<SimObjectPart>simObjectParts=new();
     internal bool doInitialization=true;
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
        internal virtual void DynamicUpdate(){
         OnUpdate();
         JustUpdated();
        }
        internal virtual void LazyUpdate(){
         //Logs.Debug(()=>"'lazy update data'");
         if(id==0){
          return;
         }
         if(this==null){
          return;
         }
         OnUpdate();
         JustUpdated();
        }
        internal virtual void OnUpdate(){
         ValidateTransform();
        }
        internal virtual void JustUpdated(){
         doInitialization=false;
        }
     internal bool noGround;
        internal virtual void ValidateTransform(){
         if(transform.hasChanged){
          //Logs.Debug(()=>"'transform.hasChanged'");
          OnPositionChanged();
          transform.hasChanged=false;
         }
        }
        internal void OnPositionChanged(){
         var manager=SimObjectManager.singleton;
         noGround=false;
         if(!HasGroundBelow(transform.position,Height)){
          nextFindGroundTime=Time.time+1f;
          noGround=true;
         }
         UpdateGroundedState();
         if(instancedRenderingIndex>=0){
          manager.instancedRendering.UpdateInstance((simObjectType,variant),instancedRenderingIndex);
         }
        }
     internal float nextFindGroundTime;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void TryWakeUp(){
         if(noGround){
          if(Time.time>=nextFindGroundTime){
           nextFindGroundTime=Time.time+1f;
           Logs.Debug(()=>"'try to find ground'");
           if(HasGroundBelow(transform.position,Height)){
            noGround=false;
            UpdateGroundedState();
           }
          }
         }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual bool HasGroundBelow(Vector3 origin,float maxDistance){
         return Physics.Raycast(
          origin,
          Vector3.down,
          maxDistance,
          WorldChunkManager.singleton.terrainLayerMask,
          QueryTriggerInteraction.Ignore
         );
        }
        internal virtual void UpdateGroundedState(){
         if(IsSimObjectStatic()){
          isGrounded=true;
         }
        }
        internal bool IsSimObjectStatic(){
         return IsSimObjectStatic(simObjectType);
        }
        internal static bool IsSimObjectStatic(Type type){
         return typeof(SimObjectStatic).IsAssignableFrom(type);
        }
        internal bool IsSimActor(){
         return IsSimActor(simObjectType);
        }
        internal static bool IsSimActor(Type type){
         return typeof(SimActor).IsAssignableFrom(type);
        }
    }
}