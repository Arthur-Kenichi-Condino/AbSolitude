#if UNITY_EDITOR
#define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims{
    internal class SimObject:MonoBehaviour{
     internal PersistentData persistentData;
        internal struct PersistentData{
         public Quaternion rotation;
         public Vector3    position;
         public Vector3    localScale;
            internal void UpdateData(SimObject simObject){
             rotation=simObject.transform.rotation;
             position=simObject.transform.position;
             localScale=simObject.transform.localScale;
            }
            public override string ToString(){
             return string.Format(CultureInfoUtil.en_US,"persistentData={{ position={0}, rotation={1}, localScale={2}, }}",position,rotation,localScale);
            }
            internal static PersistentData Parse(string s){
             PersistentData persistentData=new PersistentData();
             int positionStringStart=s.IndexOf("position=(");
             if(positionStringStart>=0){
                positionStringStart+=10;
              int positionStringEnd=s.IndexOf("), ",positionStringStart);
              string positionString=s.Substring(positionStringStart,positionStringEnd-positionStringStart);
              string[]xyzString=positionString.Split(',');
              float x=float.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float y=float.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float z=float.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              persistentData.position=new Vector3(x,y,z);
             }
             int rotationStringStart=s.IndexOf("rotation=(");
             if(rotationStringStart>=0){
                rotationStringStart+=10;
              int rotationStringEnd=s.IndexOf("), ",rotationStringStart);
              string rotationString=s.Substring(rotationStringStart,rotationStringEnd-rotationStringStart);
              string[]xyzwString=rotationString.Split(',');
              float x=float.Parse(xyzwString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float y=float.Parse(xyzwString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float z=float.Parse(xyzwString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float w=float.Parse(xyzwString[3].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              persistentData.rotation=new Quaternion(x,y,z,w);
             }
             int localScaleStringStart=s.IndexOf("localScale=(");
             if(localScaleStringStart>=0){
                localScaleStringStart+=12;
              int localScaleStringEnd=s.IndexOf("), ",localScaleStringStart);
              string localScaleString=s.Substring(localScaleStringStart,localScaleStringEnd-localScaleStringStart);
              string[]xyzString=localScaleString.Split(',');
              float x=float.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float y=float.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float z=float.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              persistentData.localScale=new Vector3(x,y,z);
             }
             if(persistentData.localScale.x<=0f||
                persistentData.localScale.y<=0f||
                persistentData.localScale.z<=0f){
              persistentData.localScale=Vector3.one;
             }
             return persistentData;
            }
        }
     internal LinkedListNode<SimObject>pooled; 
     internal(Type simType,ulong number)?id=null;
     internal Collider[]colliders;
     internal readonly List<Collider>volumeColliders=new List<Collider>();
     internal Bounds localBounds;
     protected readonly Vector3[]worldBoundsVertices=new Vector3[8];
        protected virtual void Awake(){
         foreach(Collider collider in colliders=GetComponentsInChildren<Collider>()){
          if(collider.CompareTag("SimObjectVolume")){
           if(localBounds.extents==Vector3.zero){
              localBounds=collider.bounds;
           }else{
              localBounds.Encapsulate(collider.bounds);
           }
           volumeColliders.Add(collider);
          }
         }
         localBounds.center=transform.InverseTransformPoint(localBounds.center);
         TransformBoundsVertices();
        }
        internal virtual void OnLoadingPool(){
         DisableInteractions();
        }
        internal virtual void OnActivated(){
         //Log.DebugMessage("OnActivated:id:"+id);
         TransformBoundsVertices();
         persistentData.UpdateData(this);
            transform.hasChanged=false;
         EnableInteractions();
         for(int i=0;i<Gameplayer.all.Count;++i){
          Gameplayer.all[i].OnSimObjectSpawned(this);
         }
        }
     public bool interactionsEnabled{get;protected set;}
        protected virtual void EnableInteractions(){
         foreach(Collider collider in colliders){
          collider.enabled=true;
         }
         interactionsEnabled=true;
         isOverlapping=IsOverlappingNonAlloc();
        }
        protected virtual void DisableInteractions(){
         foreach(Collider collider in colliders){
          collider.enabled=false;
         }
         interactionsEnabled=false;
        }
        internal void OnUnplaceRequest(){
         unplaceRequested=true;
        }
        internal void OnPoolRequest(){
         poolRequested=true;
        }
     [NonSerialized]bool isOverlapping;
     [NonSerialized]bool unplaceRequested;
     [NonSerialized]bool checkIfOutOfSight;
     [NonSerialized]bool poolRequested;
        internal virtual int ManualUpdate(bool doValidationChecks){
         int result=0;
         checkIfOutOfSight|=doValidationChecks;
         checkIfOutOfSight|=transform.hasChanged;
         if(transform.hasChanged){
          TransformBoundsVertices();
          persistentData.UpdateData(this);
            transform.hasChanged=false;
          isOverlapping=IsOverlappingNonAlloc();
         }
         if(unplaceRequested){
            unplaceRequested=false;
             DisableInteractions();
             SimObjectManager.singleton.DeactivateAndReleaseIdQueue.Enqueue(this);
             result=2;
         }else{
             if(isOverlapping){
                isOverlapping=false;
                 //Log.DebugMessage("simObject isOverlapping:id:"+id);
                 DisableInteractions();
                 SimObjectManager.singleton.DeactivateAndReleaseIdQueue.Enqueue(this);
                 result=2;
             }else{
                 if(checkIfOutOfSight){
                    checkIfOutOfSight=false;
                     if(IsOutOfSight()){
                         Log.DebugMessage("simObject IsOutOfSight:id:"+id);
                         DisableInteractions();
                         SimObjectManager.singleton.DeactivateQueue.Enqueue(this);
                         result=1;
                     }
                 }else{
                     if(poolRequested){
                        poolRequested=false;
                         DisableInteractions();
                         SimObjectManager.singleton.DeactivateQueue.Enqueue(this);
                         result=1;
                     }
                 }
             }
         }
         return result;
        }
        void TransformBoundsVertices(){
         worldBoundsVertices[0]=transform.TransformPoint(localBounds.min.x,localBounds.min.y,localBounds.min.z);
         worldBoundsVertices[1]=transform.TransformPoint(localBounds.max.x,localBounds.min.y,localBounds.min.z);
         worldBoundsVertices[2]=transform.TransformPoint(localBounds.max.x,localBounds.min.y,localBounds.max.z);
         worldBoundsVertices[3]=transform.TransformPoint(localBounds.min.x,localBounds.min.y,localBounds.max.z);
         worldBoundsVertices[4]=transform.TransformPoint(localBounds.min.x,localBounds.max.y,localBounds.min.z);
         worldBoundsVertices[5]=transform.TransformPoint(localBounds.max.x,localBounds.max.y,localBounds.min.z);
         worldBoundsVertices[6]=transform.TransformPoint(localBounds.max.x,localBounds.max.y,localBounds.max.z);
         worldBoundsVertices[7]=transform.TransformPoint(localBounds.min.x,localBounds.max.y,localBounds.max.z);
        }
        protected virtual bool IsOutOfSight(){
         //Log.DebugMessage("test if IsOutOfSight:id:"+id);
         return worldBoundsVertices.Any(
          v=>{
           Vector2Int cCoord=vecPosTocCoord(v);
           int cnkIdx=GetcnkIdx(cCoord.x,cCoord.y);
           return!VoxelSystem.singleton.terrainActive.TryGetValue(cnkIdx,out VoxelTerrainChunk cnk)||!cnk.hasPhysMeshBaked;
          }
         );
        }
     protected Collider[]overlappedColliders=new Collider[8];
        protected virtual bool IsOverlappingNonAlloc(){
         if(this is SimActor){
          return false;
         }
         bool result=false;
         for(int i=0;i<volumeColliders.Count;++i){
          int overlappingsLength=0;
          if(volumeColliders[i]is CapsuleCollider capsule){
           var direction=new Vector3{[capsule.direction]=1};
           direction=transform.rotation*direction;
           //Log.DebugMessage("capsule direction:"+direction);
           var offset=capsule.height/2f-capsule.radius-0.001f;
           var localPoint0=capsule.center-direction*offset;
           var localPoint1=capsule.center+direction*offset;
           var point0=transform.TransformPoint(localPoint0);
           var point1=transform.TransformPoint(localPoint1);
           _GetOverlappedColliders:{
            overlappingsLength=Physics.OverlapCapsuleNonAlloc(
             point0,
             point1,
             capsule.radius-0.001f,
             overlappedColliders
            );
           }
           if(overlappingsLength>0){
            if(overlappingsLength>=overlappedColliders.Length){
             Array.Resize(ref overlappedColliders,overlappingsLength*2);
             goto _GetOverlappedColliders;
            }
            ProcessOverlappings(capsule);
           }
          }
          void ProcessOverlappings(Collider volumeCollider){
           for(int j=0;j<overlappingsLength;++j){
            Collider overlappedCollider=overlappedColliders[j];
            if(overlappedCollider.transform.root!=transform.root){//  it's not myself
             SimObject overlappedSimObject=overlappedCollider.GetComponentInParent<SimObject>();
             if(overlappedSimObject!=null){
              if(!(overlappedSimObject is SimActor)){
               result=true;
              }
             }
            }
           }
          }
         }
         return result;
        }
        protected virtual void OnDrawGizmos(){
         #if UNITY_EDITOR
             Util.DrawRotatedBounds(worldBoundsVertices,Color.white);
         #endif
        }
    }
}