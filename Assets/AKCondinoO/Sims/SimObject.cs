#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Gameplaying;
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims{
    internal partial class SimObject:NetworkBehaviour{
     internal PersistentData persistentData;
        internal struct PersistentData{
         public Quaternion rotation;
         public Vector3    position;
         public Vector3    localScale;
         public bool isInventoryItem;
         public(Type simInventoryType,ulong idNumber)?containerSimInventoryId;
         //  Não salvar lista de inventários aqui; ela é salva em uma pasta própria, por tipo e id de sim object
            internal void UpdateData(SimObject simObject){
             rotation=simObject.transform.rotation;
             position=simObject.transform.position;
             localScale=simObject.transform.localScale;
             isInventoryItem=(simObject.asInventoryItem!=null);
            }
            public override string ToString(){
             return string.Format(CultureInfoUtil.en_US,"persistentData={{ position={0} , rotation={1} , localScale={2} , isInventoryItem={3} , }}",position,rotation,localScale,isInventoryItem);
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
             int isInventoryItemStringStart=s.IndexOf("isInventoryItem=");
             if(isInventoryItemStringStart>=0){
                isInventoryItemStringStart+=16;
              int isInventoryItemStringEnd=s.IndexOf(" , ",isInventoryItemStringStart);
              string isInventoryItemString=s.Substring(isInventoryItemStringStart,isInventoryItemStringEnd-isInventoryItemStringStart);
              bool isInventoryItem=bool.Parse(isInventoryItemString);
              persistentData.isInventoryItem=isInventoryItem;
             }
             return persistentData;
            }
        }
     internal NetworkObject netObj;
     internal Queue<NetworkObject>clientSidePooling=null;
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
      private readonly NetworkVariable<Quaternion>netRotation=new NetworkVariable<Quaternion>(default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Owner
      );
       private void OnClientSideNetRotationValueChanged(Quaternion previous,Quaternion current){
        if(Core.singleton.isClient){
         if(!IsOwner){
          transform.rotation=current;
         }
        }
       }
       private void OnServerSideNetRotationValueChanged(Quaternion previous,Quaternion current){
        if(Core.singleton.isServer){
         if(!IsOwner){
          transform.rotation=current;
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
     [SerializeField]internal bool ZAxisIsUp=false;
     internal Transform head;
     internal Transform  leftEye;
     internal Transform rightEye;
     internal Transform  leftHand;
     internal Transform rightHand;
     internal LinkedListNode<SimObject>pooled; 
     internal(Type simObjectType,ulong idNumber)?id=null;
     internal(Type simObjectType,ulong idNumber)?masterId=null;
      protected SimObject masterSimObject;
     //  TO DO: componente Rigidbody tem que ficar sempre no transform root
     internal Rigidbody hasRigidbody;
     internal Collider[]colliders;
     internal readonly List<Collider>volumeColliders=new List<Collider>();
     internal NavMeshObstacle[]navMeshObstacles;
      internal bool navMeshObstacleCarving;
     internal Bounds localBounds;
     protected readonly Vector3[]worldBoundsVertices=new Vector3[8];
        protected virtual void Awake(){
         netObj=GetComponent<NetworkObject>();
         hasRigidbody=transform.root.GetComponent<Rigidbody>();
         //Log.DebugMessage(id+" hasRigidbody:"+(hasRigidbody!=null));
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
         foreach(NavMeshObstacle navMeshObstacle in navMeshObstacles=GetComponentsInChildren<NavMeshObstacle>()){
          navMeshObstacleCarving|=navMeshObstacle.carving;
         }
         localBounds.center=transform.InverseTransformPoint(localBounds.center);
         TransformBoundsVertices();
         foreach(Renderer renderer in renderers=GetComponentsInChildren<Renderer>()){
          foreach(Material material in renderer.materials){
           materialShader[material]=material.shader;
          }
          if(autoChangeMaterialsToFade){
           foreach(Material material in renderer.materials){
            if(material.shader!=RenderingUtil.StandardShader){
             material.shader=RenderingUtil.StandardShader;
             RenderingUtil.SetupStandardShaderMaterialBlendMode(material,RenderingUtil.BlendMode.Fade);
            }
            Color c=material.GetColor("_Color");
            c.a=0f;
            material.SetColor("_Color",c);
           }
          }
          renderer.enabled=false;//  to prevent a "flashing" of the object when it's created
         }
         skillBuffs=gameObject.AddComponent<SkillBuffEffectsState>();
         skillBuffs.targetSimObject=this;
        }
        public override void OnDestroy(){
         base.OnDestroy();
        }
        internal virtual void OnLoadingPool(){
         DisableInteractions();
        }
        internal virtual void OnSpawned(){
         RenewStats();
        }
        internal virtual void OnDespawned(){
         ReleaseStats();
        }
        internal virtual void OnActivated(){
         //Log.DebugMessage("OnActivated:id:"+id);
         inventoryItemsToSpawn.Clear();
         if(Core.singleton.isServer){
          masterSimObject=GetMaster();
          SetAsSlaveOf(masterSimObject);
          if(!inventory.ContainsKey(typeof(SimInventory))||inventory[typeof(SimInventory)].Count<=0){
           SimInventoryManager.singleton.AddInventoryTo(this,typeof(SimInventory));
          }
          int totalInventorySpaces=0;
          foreach(var typeInventoryListPair in inventory){
           foreach(var simInventory in typeInventoryListPair.Value){
            totalInventorySpaces+=simInventory.Value.maxItemsCount;
           }
          }
          inventoryItemsSpawnData=new SpawnData(totalInventorySpaces);
         }
         TransformBoundsVertices();
         persistentData.UpdateData(this);
            transform.hasChanged=false;
         EnableInteractions();
         if(Core.singleton.isServer){
          if(!netObj.IsSpawned){
           //Log.DebugMessage("netObj should be spawned now");
           netObj.Spawn(destroyWithScene:false);
           netObj.DontDestroyWithOwner=true;
          }else if(IsOwner){
           Log.DebugMessage("set net variables");
           netPosition.Value=persistentData.position  ;
           netRotation.Value=persistentData.rotation  ;
           netScale   .Value=persistentData.localScale;
          }
         }
        }
        internal virtual void OnDeactivated(){
         skillBuffs.Clear();
         persistentStats.UpdateData(this);
         //Log.DebugMessage("OnDeactivated:id:"+id);
        }
        public override void OnNetworkSpawn(){
         base.OnNetworkSpawn();
         if(Core.singleton.isServer){
          //Log.DebugMessage("SimObject:OnNetworkSpawn:isServer");
          if(IsOwner){
           //Log.DebugMessage("init net variables");
           netPosition.Value=persistentData.position  ;
           netRotation.Value=persistentData.rotation  ;
           netScale   .Value=persistentData.localScale;
          }
          OnServerSideNetPositionValueChanged(transform.position  ,netPosition.Value);//  update on spawn
          netPosition.OnValueChanged+=OnServerSideNetPositionValueChanged;
          OnServerSideNetRotationValueChanged(transform.rotation  ,netRotation.Value);//  update on spawn
          netRotation.OnValueChanged+=OnServerSideNetRotationValueChanged;
          OnServerSideNetScaleValueChanged   (transform.localScale,netScale   .Value);//  update on spawn
          netScale   .OnValueChanged+=OnServerSideNetScaleValueChanged   ;
         }
         if(Core.singleton.isClient){
          Log.DebugMessage("SimObject:OnNetworkSpawn:isClient");
          OnClientSideNetPositionValueChanged(transform.position  ,netPosition.Value);//  update on spawn
          netPosition.OnValueChanged+=OnClientSideNetPositionValueChanged;
          OnClientSideNetRotationValueChanged(transform.rotation  ,netRotation.Value);//  update on spawn
          netRotation.OnValueChanged+=OnClientSideNetRotationValueChanged;
          OnClientSideNetScaleValueChanged   (transform.localScale,netScale   .Value);//  update on spawn
          netScale   .OnValueChanged+=OnClientSideNetScaleValueChanged   ;
         }
        }
        public override void OnNetworkDespawn(){
         if(Core.singleton.isServer){
          //Log.DebugMessage("SimObject:OnNetworkDespawn:isServer");
          netPosition.OnValueChanged-=OnServerSideNetPositionValueChanged;
          netRotation.OnValueChanged-=OnServerSideNetRotationValueChanged;
          netScale   .OnValueChanged-=OnServerSideNetScaleValueChanged   ;
         }
         if(Core.singleton.isClient){
          //Log.DebugMessage("SimObject:OnNetworkDespawn:isClient");
          netPosition.OnValueChanged-=OnClientSideNetPositionValueChanged;
          netRotation.OnValueChanged-=OnClientSideNetRotationValueChanged;
          netScale   .OnValueChanged-=OnClientSideNetScaleValueChanged   ;
         }
         base.OnNetworkDespawn();
        }
        internal SimObject GetMaster(){
         if(masterId!=null&&SimObjectManager.singleton.active.TryGetValue(masterId.Value,out SimObject masterSimObject)){
          return masterSimObject;
         }
         return null;
        }
        protected virtual void SetAsSlaveOf(SimObject masterSimObject){
         if(masterSimObject!=null){
          masterSimObject.SetSlave(this);
         }
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
         if(isOverlapping){
          safePosition=null;
         }else{
          safePosition=transform.position;
         }
         foreach(var gameplayer in GameplayerManagement.singleton.all){
          ulong clientId=gameplayer.Key;
          if(colliders.Length>0){
           gameplayer.Value.OnSimObjectEnabled(this,colliders[0].gameObject.layer);
          }
         }
         EnableRenderers();
        }
        protected virtual void DisableInteractions(){
         foreach(Collider collider in colliders){
          collider.enabled=false;
         }
         interactionsEnabled=false;
         foreach(var gameplayer in GameplayerManagement.singleton.all){
          ulong clientId=gameplayer.Key;
          if(colliders.Length>0){
           gameplayer.Value.OnSimObjectDisabled(this,colliders[0].gameObject.layer);
          }
         }
         DisableRenderers();
        }
        internal void OnUnplaceRequest(){
         unplaceRequested=true;
        }
        internal void OnPoolRequest(){
         poolRequested=true;
        }
     Vector3?safePosition;
     [NonSerialized]bool updateRenderersFlag;
     [NonSerialized]bool isOverlapping;
     [NonSerialized]bool unplaceRequested;
     [NonSerialized]bool checkIfOutOfSight;
     [NonSerialized]bool poolRequested;
        internal virtual int ManualUpdate(bool doValidationChecks){
         if(masterId!=null&&(masterSimObject==null||masterSimObject.id==null||masterSimObject.id.Value!=masterId.Value)){
          Log.DebugMessage("master sim [id:"+masterId+"] validation failed: renew masterObject with GetMaster(); myid:"+id);
          masterSimObject=GetMaster();
          SetAsSlaveOf(masterSimObject);
         }
         int result=0;
         skillBuffs.ManualUpdate(Time.deltaTime);
         if(asInventoryItem!=null){
          result=3;
          //Log.DebugMessage("ManualUpdate asInventoryItem");
          if(asInventoryItem.container==null){
              Log.DebugMessage("asInventoryItem invalid container");
          }else{
              if(asInventoryItem.container.asSimObject==null){
                  Log.DebugMessage("asInventoryItem.container.asSimObject==null");
              }
          }
          SetAsInventoryItemTransform();
          return result;
         }
         updateRenderersFlag|=doValidationChecks;
         updateRenderersFlag|=transform.hasChanged;
         updateRenderersFlag|=Core.singleton.currentRenderingTargetCameraChanged;
         updateRenderersFlag|=Core.singleton.currentRenderingTargetCameraHasTransformChanges;
         checkIfOutOfSight|=doValidationChecks;
         checkIfOutOfSight|=transform.hasChanged;
         if(transform.hasChanged){
          TransformBoundsVertices();
          persistentData.UpdateData(this);
          if(Core.singleton.isServer){
           if(IsOwner){
            netPosition.Value=persistentData.position  ;
            netRotation.Value=persistentData.rotation  ;
            netScale   .Value=persistentData.localScale;
           }
          }
            transform.hasChanged=false;
          isOverlapping=IsOverlappingNonAlloc();
          foreach(var gameplayer in GameplayerManagement.singleton.all){
           if(colliders.Length>0){
            gameplayer.Value.OnSimObjectTransformHasChanged(this,colliders[0].gameObject.layer);
           }
          }
         }
         bool returnedToSafePos=false;
         GetCollidersTouchingNonAlloc();
         if(unplaceRequested){
            unplaceRequested=false;
             if(Core.singleton.isServer){
              DisableInteractions();
              if(netObj.IsSpawned){
               netObj.DontDestroyWithOwner=true;
               netObj.Despawn(destroy:false);
              }
              SimObjectManager.singleton.deactivateAndReleaseIdQueue.Enqueue(this);
              result=2;
             }
         }else{
             if(isOverlapping){
                isOverlapping=false;
                 if(Core.singleton.isServer){
                  if(safePosition!=null){
                   transform.position=safePosition.Value;
                   returnedToSafePos=!IsOverlappingNonAlloc();
                  }
                  if(!returnedToSafePos){
                   Log.DebugMessage("simObject isOverlapping:id:"+id);
                   DisableInteractions();
                   if(netObj.IsSpawned){
                    netObj.DontDestroyWithOwner=true;
                    netObj.Despawn(destroy:false);
                   }
                   SimObjectManager.singleton.deactivateAndReleaseIdQueue.Enqueue(this);
                   result=2;
                  }
                 }
             }else{
                 if(checkIfOutOfSight){
                    checkIfOutOfSight=false;
                     if(IsOutOfSight()){
                         if(Core.singleton.isServer){
                          if(IsOwner){
                           bool ownershipChanged=false;
                           foreach(var gameplayer in GameplayerManagement.singleton.all){
                            ulong clientId=gameplayer.Key;
                            //Log.DebugMessage("should the ownership be changed to clientId:"+clientId);
                            if(gameplayer.Value!=Gameplayer.main&&IsInPlayerWorldBounds(gameplayer.Value)){
                             Log.DebugMessage("change ownership to clientId:"+clientId);
                             netObj.ChangeOwnership(clientId);
                             netObj.DontDestroyWithOwner=true;
                             ownershipChanged=true;
                             break;
                            }
                           }
                           if(!ownershipChanged){
                            //Log.DebugMessage("simObject IsOutOfSight:id:"+id);
                            DisableInteractions();
                            if(netObj.IsSpawned){
                             netObj.DontDestroyWithOwner=true;
                             netObj.Despawn(destroy:false);
                            }
                            SimObjectManager.singleton.deactivateQueue.Enqueue(this);
                            result=1;
                           }
                          }
                         }
                     }
                 }else{
                     if(poolRequested){
                        poolRequested=false;
                         if(Core.singleton.isServer){
                          DisableInteractions();
                          if(netObj.IsSpawned){
                           netObj.DontDestroyWithOwner=true;
                           netObj.Despawn(destroy:false);
                          }
                          SimObjectManager.singleton.deactivateQueue.Enqueue(this);
                          result=1;
                         }
                     }
                 }
             }
         }
         if(result==0){
          safePosition=transform.position;
         }
         return result;
        }
        internal virtual int ManualUpdateAsClient(bool doValidationChecks){
         int result=0;
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
        protected virtual bool IsInPlayerActiveWorldBounds(Gameplayer gameplayer){
         return worldBoundsVertices.Any(
          v=>{
           //Log.DebugMessage("IsInPlayerActiveWorldBounds:testing v");
           return gameplayer.activeWorldBounds.Contains(v);
          }
         );
        }
        protected virtual bool IsInPlayerWorldBounds(Gameplayer gameplayer){
         return worldBoundsVertices.Any(
          v=>{
           //Log.DebugMessage("IsInPlayerWorldBounds:testing v");
           return gameplayer.worldBounds.Contains(v);
          }
         );
        }
     protected Collider[]overlappedColliders=new Collider[8];
        protected virtual bool IsOverlappingNonAlloc(){
         if(hasRigidbody!=null){
          return false;
         }
         if(this is SimActor){
          return false;
         }
         bool result=false;
         for(int i=0;i<volumeColliders.Count;++i){
          int overlappingsLength=0;
          if(volumeColliders[i]is CapsuleCollider capsule){
           var direction=new Vector3{[capsule.direction]=1};
           //Log.DebugMessage("capsule direction:"+direction);
           var offset=capsule.height/2f-capsule.radius;
           var localPoint0=capsule.center-direction*offset;
           var localPoint1=capsule.center+direction*offset;
           var point0=transform.TransformPoint(localPoint0);
           var point1=transform.TransformPoint(localPoint1);
           Vector3 r=transform.TransformVector(capsule.radius,capsule.radius,capsule.radius);
           float radius=Enumerable.Range(0,3).Select(xyz=>xyz==capsule.direction?0:r[xyz]).Select(Mathf.Abs).Max();
           _GetOverlappedColliders:{
            overlappingsLength=Physics.OverlapCapsuleNonAlloc(
             point0,
             point1,
             radius-0.001f,
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
              if(!(overlappedSimObject is SimActor||overlappedSimObject.hasRigidbody!=null)){
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
        internal virtual void ManualLateUpdate(){
         UpdateRenderers();
        }
        internal void OnExitSaveDataCollection(){
         skillBuffs.Clear(true);
         persistentStats.UpdateData(this);
        }
        internal bool OnExitSaveRecursion(){
         if(unplaceRequested){
          SimObjectSpawner.singleton.despawnAndReleaseIdQueue.Enqueue(this);
          return true;
         }
         return false;
        }
        protected virtual void OnDrawGizmos(){
         #if UNITY_EDITOR
             //Util.DrawRotatedBounds(worldBoundsVertices,Color.white);
         #endif
        }
    }
}