#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Gameplaying;
using AKCondinoO.Sims.Actors;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims{
    internal class SimObject:NetworkBehaviour{
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
             return string.Format(CultureInfoUtil.en_US,"persistentData={{ position={0} , rotation={1} , localScale={2} , }}",position,rotation,localScale);
            }
            internal static PersistentData Parse(string s){
             PersistentData persistentData=new PersistentData();
             int positionStringStart=s.IndexOf("position=(");
             if(positionStringStart>=0){
                positionStringStart+=10;
              int positionStringEnd=s.IndexOf(") , ",positionStringStart);
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
              int rotationStringEnd=s.IndexOf(") , ",rotationStringStart);
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
              int localScaleStringEnd=s.IndexOf(") , ",localScaleStringStart);
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
     internal NetworkObject netObj;
     internal Gameplayer owner;
      private readonly NetworkVariable<Vector3>netPosition=new NetworkVariable<Vector3>(default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Owner
      );
       private void OnClientSideNetPositionValueChanged(Vector3 previous,Vector3 current){
        if(Core.singleton.isClient){
         if(!IsOwner){
          transform.position=current;
         }
        }
       }
       private void OnServerSideNetPositionValueChanged(Vector3 previous,Vector3 current){
        if(Core.singleton.isServer){
         if(!IsOwner){
          transform.position=current;
         }
        }
       }
      private readonly NetworkVariable<Vector3>netScale=new NetworkVariable<Vector3>(default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Owner
      );
       private void OnClientSideNetScaleValueChanged(Vector3 previous,Vector3 current){
        if(Core.singleton.isClient){
         if(!IsOwner){
          transform.localScale=current;
         }
        }
       }
       private void OnServerSideNetScaleValueChanged(Vector3 previous,Vector3 current){
        if(Core.singleton.isServer){
         if(!IsOwner){
          transform.localScale=current;
         }
        }
       }
     internal LinkedListNode<SimObject>pooled; 
     internal(Type simType,ulong number)?id=null;
     internal(Type simType,ulong number)?master=null;
      protected SimObject masterObject;
     internal Collider[]colliders;
     internal readonly List<Collider>volumeColliders=new List<Collider>();
     internal Bounds localBounds;
     protected readonly Vector3[]worldBoundsVertices=new Vector3[8];
        protected virtual void Awake(){
         netObj=GetComponent<NetworkObject>();
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
         if(Core.singleton.isServer){
          masterObject=GetMaster();
          if(masterObject!=null&&masterObject is SimActor masterActor){
           masterActor.SetSlave(this);
          }
         }
         TransformBoundsVertices();
         persistentData.UpdateData(this);
            transform.hasChanged=false;
         EnableInteractions();
         if(Core.singleton.isServer){
          if(!netObj.IsSpawned){
           netObj.Spawn(destroyWithScene:false);
          }else{
           Log.Warning("netObj should have been despawned");
          }
          foreach(var gameplayer in GameplayerManagement.singleton.all){
           gameplayer.Value.OnSimObjectSpawned(this);
          }
         }
        }
        public override void OnNetworkSpawn(){
         base.OnNetworkSpawn();
         if(Core.singleton.isServer){
          Log.DebugMessage("SimObject:OnNetworkSpawn:isServer");
          if(IsOwner){
           netPosition.Value=persistentData.position  ;
           netScale   .Value=persistentData.localScale;
          }else{
           Log.Warning("SimObject OnNetworkSpawn should always start with the server as owner");
          }
          OnServerSideNetPositionValueChanged(transform.position  ,netPosition.Value);//  update on spawn
          netPosition.OnValueChanged+=OnServerSideNetPositionValueChanged;
          OnServerSideNetScaleValueChanged   (transform.localScale,netScale   .Value);//  update on spawn
          netScale   .OnValueChanged+=OnServerSideNetScaleValueChanged   ;
         }
         if(Core.singleton.isClient){
          Log.DebugMessage("SimObject:OnNetworkSpawn:isClient");
          OnClientSideNetPositionValueChanged(transform.position  ,netPosition.Value);//  update on spawn
          netPosition.OnValueChanged+=OnClientSideNetPositionValueChanged;
          OnClientSideNetScaleValueChanged   (transform.localScale,netScale   .Value);//  update on spawn
          netScale   .OnValueChanged+=OnClientSideNetScaleValueChanged   ;
         }
        }
        public override void OnNetworkDespawn(){
         if(Core.singleton.isServer){
          Log.DebugMessage("SimObject:OnNetworkDespawn:isServer");
          netPosition.OnValueChanged-=OnServerSideNetPositionValueChanged;
          netScale   .OnValueChanged-=OnServerSideNetScaleValueChanged   ;
         }
         if(Core.singleton.isClient){
          Log.DebugMessage("SimObject:OnNetworkDespawn:isClient");
          netPosition.OnValueChanged-=OnClientSideNetPositionValueChanged;
          netScale   .OnValueChanged-=OnClientSideNetScaleValueChanged   ;
         }
         base.OnNetworkDespawn();
        }
        internal SimObject GetMaster(){
         if(master!=null&&SimObjectManager.singleton.active.TryGetValue(master.Value,out SimObject masterObject)){
          return masterObject;
         }
         return null;
        }
        protected virtual void SetSlave(SimObject slave){
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
          if(Core.singleton.isServer){
           if(IsOwner){
            netPosition.Value=persistentData.position  ;
            netScale   .Value=persistentData.localScale;
           }
          }
            transform.hasChanged=false;
          isOverlapping=IsOverlappingNonAlloc();
          foreach(var gameplayer in GameplayerManagement.singleton.all){
           gameplayer.Value.OnSimObjectTransformHasChanged(this,colliders[0].gameObject.layer);
          }
         }
         GetCollidersTouchingNonAlloc();
         if(unplaceRequested){
            unplaceRequested=false;
             DisableInteractions();
             if(Core.singleton.isServer){
              if(netObj.IsSpawned){
               netObj.Despawn(destroy:false);
              }
             }
             SimObjectManager.singleton.DeactivateAndReleaseIdQueue.Enqueue(this);
             result=2;
         }else{
             if(isOverlapping){
                isOverlapping=false;
                 //Log.DebugMessage("simObject isOverlapping:id:"+id);
                 DisableInteractions();
                 if(Core.singleton.isServer){
                  if(netObj.IsSpawned){
                   netObj.Despawn(destroy:false);
                  }
                 }
                 SimObjectManager.singleton.DeactivateAndReleaseIdQueue.Enqueue(this);
                 result=2;
             }else{
                 if(checkIfOutOfSight){
                    checkIfOutOfSight=false;
                     if(IsOutOfSight()){
                         Log.DebugMessage("simObject IsOutOfSight:id:"+id);
                         DisableInteractions();
                         if(Core.singleton.isServer){
                          if(netObj.IsSpawned){
                           netObj.Despawn(destroy:false);
                          }
                         }
                         SimObjectManager.singleton.DeactivateQueue.Enqueue(this);
                         result=1;
                     }
                 }else{
                     if(poolRequested){
                        poolRequested=false;
                         DisableInteractions();
                         if(Core.singleton.isServer){
                          if(netObj.IsSpawned){
                           netObj.Despawn(destroy:false);
                          }
                         }
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
     protected Collider[]collidersTouching=new Collider[8];
        protected virtual void GetCollidersTouchingNonAlloc(){
        }
        protected virtual void OnDrawGizmos(){
         #if UNITY_EDITOR
             Util.DrawRotatedBounds(worldBoundsVertices,Color.white);
         #endif
        }
    }
}