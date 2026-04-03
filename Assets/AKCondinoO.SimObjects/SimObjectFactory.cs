using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObjectFactory<T>where T:SimObject{
     private readonly SimObjectPool<T>pool;
     private GameObject   prefabMeshObject  ;
     private MeshRenderer prefabMeshRenderer;
     private MeshFilter   prefabMeshFilter  ;
     private Mesh colliderMesh;
        internal SimObjectFactory(T prefab,Transform parent=null){
         var manager=SimObjectManager.singleton;
         pool=new(prefab,parent);
         var type=prefab.simObjectType;
         var variant=prefab.variant;
         if(prefab.meshPrefab!=null){
          prefabMeshObject=prefab.meshPrefab;
          prefabMeshRenderer=prefab.meshPrefab.GetComponentInChildren<MeshRenderer>();
          prefabMeshFilter  =prefab.meshPrefab.GetComponentInChildren<MeshFilter  >();
          Mesh mesh;
          if(prefabMeshRenderer!=null&&prefabMeshFilter!=null&&(mesh=prefabMeshFilter.sharedMesh)!=null){
           Logs.Debug(()=>"mesh.name:"+mesh.name);
           if(prefab.useInstancedRendering){
            manager.instancedRendering.RegisterType((type,variant),mesh,prefabMeshRenderer.sharedMaterials,prefab.meshPrefab.layer);
           }
           if(prefab.useMeshObjectSubMeshesForCollider.Length>0){
            colliderMesh=MeshHelper.BuildColliderMeshFromSubMeshes(mesh,prefab.useMeshObjectSubMeshesForCollider);
           }
          }
         }
        }
        internal void Destroy(bool destroy=false){
         pool.Destroy(destroy);
         prefabMeshObject  =null;
         prefabMeshRenderer=null;
         prefabMeshFilter  =null;
         if(colliderMesh!=null){
          GameObject.Destroy(colliderMesh);colliderMesh=null;
         }
        }
        internal virtual SimObject Spawn(SimObjectSpawn item){
         var manager=SimObjectManager.singleton;
         T simObject=pool.Rent();
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
         if(prefabMeshObject!=null){
          if(simObject.simObjectMeshRenderer==null){
           simObject.simObjectMeshRenderer=simObject.gameObject.AddComponent<MeshRenderer>();
           simObject.simObjectMeshFilter  =simObject.gameObject.AddComponent<MeshFilter  >();
           Mesh mesh;
           if(prefabMeshRenderer!=null&&prefabMeshFilter!=null&&(mesh=prefabMeshFilter.sharedMesh)!=null){
            simObject.simObjectMeshFilter.sharedMesh=mesh;
            var source=prefabMeshRenderer;
            var target=simObject.simObjectMeshRenderer;
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
          }
         }
        }
        private void SetupCollider(T simObject){
         if(colliderMesh!=null&&simObject.simObjectMeshCollider==null){
          simObject.simObjectMeshCollider=simObject.gameObject.AddComponent<MeshCollider>();
          simObject.simObjectMeshCollider.sharedMesh=colliderMesh;
         }
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