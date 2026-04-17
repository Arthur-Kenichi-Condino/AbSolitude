using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors.SimInteractions;
using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObjectFactory<T>where T:SimObject{
     private readonly SimObjectPool<T>pool;
     private GameObject   prefabMeshObject  ;
     private MeshRenderer prefabMeshRenderer;
     private MeshFilter   prefabMeshFilter  ;
     private Mesh colliderMesh;
     private SimObjectPartData[]partsData;
     struct SimObjectPartData{
      public SimObjectPart prefab        ;
      public MeshRenderer  prefabRenderer;
      public MeshFilter    prefabFilter  ;
      public Mesh colliderMesh;
     }
     private InteractionSlot[]slotPrefabs;
        internal SimObjectFactory(T prefab,Transform parent=null){
         var manager=SimObjectManager.singleton;
         pool=new(prefab,parent);
         var type=prefab.simObjectType;
         var variant=prefab.variant;
         if(prefab.meshPrefab!=null){
          prefabMeshObject  =prefab.meshPrefab;
          prefabMeshRenderer=prefab.meshPrefab.GetComponent<MeshRenderer>();
          prefabMeshFilter  =prefab.meshPrefab.GetComponent<MeshFilter  >();
          if(prefabMeshRenderer!=null&&TryGetMesh(prefabMeshFilter,out Mesh mesh)){
           Logs.Debug(()=>"mesh.name:"+mesh.name);
           if(prefab.useInstancedRendering){
            manager.instancedRendering.RegisterType((type,variant),mesh,prefabMeshRenderer.sharedMaterials,prefab.meshPrefab.layer);
           }
           colliderMesh=BuildColliderIfNeeded(
            mesh,
            prefab.useMeshObjectSubMeshesForCollider
           );
          }
          var partPrefabs=prefab.meshPrefab.GetComponentsInChildren<SimObjectPart>();
          partsData=new SimObjectPartData[partPrefabs.Length];
          for(int i=0;i<partsData.Length;i++){
           var partPrefab=partPrefabs[i];
           var partData=new SimObjectPartData(){
            prefab=        partPrefab,
            prefabRenderer=partPrefab.GetComponent<MeshRenderer>(),
            prefabFilter=  partPrefab.GetComponent<MeshFilter  >(),
           };
           if(TryGetMesh(partData.prefabFilter,out Mesh partMesh)){
            partData.colliderMesh=BuildColliderIfNeeded(
             partMesh,
             partPrefab.usePartMeshSubMeshesForCollider
            );
           }
           partsData[i]=partData;
          }
          slotPrefabs=prefab.meshPrefab.GetComponentsInChildren<InteractionSlot>();
         }
        }
        private static bool TryGetMesh(MeshFilter filter,out Mesh mesh){
         mesh=filter!=null?filter.sharedMesh:null;
         return mesh!=null;
        }
        private static Mesh BuildColliderIfNeeded(
         Mesh mesh,
         int[]subMeshes
        ){
         if(mesh==null||subMeshes==null||subMeshes.Length<=0)return null;
         return MeshHelper.BuildColliderMeshFromSubMeshes(mesh,subMeshes);
        }
        internal void Destroy(bool destroy=false){
         pool.Destroy(destroy);
         prefabMeshObject  =null;
         prefabMeshRenderer=null;
         prefabMeshFilter  =null;
         if(colliderMesh!=null){
          GameObject.Destroy(colliderMesh);colliderMesh=null;
         }
         if(partsData!=null){
          for(int i=0;i<partsData.Length;i++){
           var partData=partsData[i];
           if(partData.colliderMesh!=null){
            GameObject.Destroy(partData.colliderMesh);
           }
          }
         }
        }
        internal virtual SimObject Spawn(SimObjectSpawn item){
         var manager=SimObjectManager.singleton;
         T simObject=pool.Rent();
         simObject.doInitialization=true;
         simObject.transform.position=item.position;
         AssignId(simObject);
         SetupSimObject(simObject);
         manager.OnSpawn(simObject);
         return simObject;
        }
        private void AssignId(T simObject){
         var manager=SimObjectManager.singleton;
         simObject.id=manager.simIds.Next((simObject.simObjectType,simObject.variant));
        }
        internal void SetupSimObject(T simObject){
         if(!RegisterForInstancedRendering(simObject)){
          SetupRenderer(simObject);
         }
         SetupCollider(simObject);
         SetupAllParts(simObject);
         SetupAllSlots(simObject);
        }
        private bool RegisterForInstancedRendering(T simObject){
         var manager=SimObjectManager.singleton;
         if(simObject.useInstancedRendering){
          var index=manager.instancedRendering.AddInstance((simObject.simObjectType,simObject.variant),simObject);
          return index>=0;
         }
         return false;
        }
        private void SetupRenderer(T simObject){
         if(prefabMeshObject==null)return;
         CopyLocalTransform(
          prefabMeshObject.transform,
          simObject.simObjectRendererComponents.transform
         );
         SetupMeshRenderer(
          simObject.simObjectRendererComponents.transform,
          ref simObject.simObjectMeshRenderer,
          ref simObject.simObjectMeshFilter,
          prefabMeshRenderer,
          prefabMeshFilter
         );
        }
        private static void SetupMeshRenderer(
         Transform root,
         ref MeshRenderer renderer,
         ref MeshFilter   filter,
         MeshRenderer sourceRenderer,
         MeshFilter   sourceFilter
        ){
         if(renderer!=null)return;
         renderer=root.gameObject.AddComponent<MeshRenderer>();
         filter  =root.gameObject.AddComponent<MeshFilter  >();
         if(sourceRenderer!=null&&TryGetMesh(sourceFilter,out var mesh)){
          filter.sharedMesh=mesh;
          CopyRendererProperties(sourceRenderer,renderer);
         }
        }
        private static void CopyRendererProperties(MeshRenderer source,MeshRenderer target){
         target.sharedMaterials     =source.sharedMaterials     ;
         target.shadowCastingMode   =source.shadowCastingMode   ;
         target.receiveShadows      =source.receiveShadows      ;
         target.renderingLayerMask  =source.renderingLayerMask  ;
         target.lightProbeUsage     =source.lightProbeUsage     ;
         target.reflectionProbeUsage=source.reflectionProbeUsage;
         target.probeAnchor         =source.probeAnchor         ;
         target.motionVectorGenerationMode=source.motionVectorGenerationMode;
         target.allowOcclusionWhenDynamic =source.allowOcclusionWhenDynamic ;
        }
        private void SetupCollider(T simObject){
         if(colliderMesh==null)return;
         CopyLocalTransform(
          prefabMeshObject.transform,
          simObject.simObjectCollisionComponents.transform
         );
         SetupMeshCollider(
          simObject.simObjectCollisionComponents.transform,
          colliderMesh,
          ref simObject.simObjectMeshCollider
         );
        }
        private static MeshCollider SetupMeshCollider(
         Transform root,
         Mesh mesh,
         ref MeshCollider collider
        ){
         if(mesh==null||collider!=null)return collider;
         collider=root.gameObject.AddComponent<MeshCollider>();
         collider.sharedMesh=mesh;
         return collider;
        }
        private void SetupAllParts(T simObject){
         if(prefabMeshObject==null||simObject.simObjectParts.Count>0)return;
         CopyLocalTransform(
          prefabMeshObject.transform,
          simObject.simObjectPartsRoot
         );
         for(int i=0;i<partsData.Length;i++){
          var partData=partsData[i];
          var part=GameObject.Instantiate(simObject.simObjectPartBase);
          part.transform.SetParent(simObject.simObjectPartsRoot,false);
          part.holder=simObject;
          CopyLocalTransform(partData.prefab.transform,part.transform);
          SetupMeshRenderer(
           part.simObjectPartRendererComponents.transform,
           ref part.simObjectPartMeshRenderer,
           ref part.simObjectPartMeshFilter,
           partData.prefabRenderer,
           partData.prefabFilter
          );
          SetupMeshCollider(
           part.simObjectPartCollisionComponents.transform,
           partData.colliderMesh,
           ref part.simObjectPartMeshCollider
          );
          simObject.simObjectParts.Add(part);
         }
        }
        private void SetupAllSlots(T simObject){
         if(prefabMeshObject==null||simObject.simObjectSlots.Count>0)return;
         CopyLocalTransform(
          prefabMeshObject.transform,
          simObject.simObjectSlotsRoot
         );
         for(int i=0;i<slotPrefabs.Length;i++){
          var slotPrefab=slotPrefabs[i];
          var slot=GameObject.Instantiate(slotPrefab);
          slot.transform.SetParent(simObject.simObjectSlotsRoot,false);
          CopyLocalTransform(slotPrefab.transform,slot.transform);
          simObject.simObjectSlots.Add(slot);
         }
        }
        private static void CopyLocalTransform(Transform source,Transform target){
         target.localPosition=source.localPosition;
         target.localRotation=source.localRotation;
         target.localScale   =source.localScale;
        }
        internal virtual void Despawn(T simObject){
         var manager=SimObjectManager.singleton;
         simObject.id=0;
         manager.instancedRendering.RemoveInstance((simObject.simObjectType,simObject.variant),simObject.instancedRenderingIndex);
         simObject.instancedRenderingIndex=-1;
         pool.Return(simObject);
        }
    }
}