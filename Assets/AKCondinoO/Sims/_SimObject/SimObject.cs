#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Gameplaying;
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Trees;
using AKCondinoO.Sims.Weapons;
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
using UnityEngine.Animations;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims{
    internal partial class SimObject:NetworkBehaviour,Interactable{
     [NonSerialized]internal static System.Random seedGenerator;
     [NonSerialized]protected System.Random math_random;
     #region networking
         [NonSerialized]internal Queue<NetworkObject>clientSidePooling=null;
         [NonSerialized]internal Gameplayer owner;
     #endregion
     [NonSerialized]protected bool zAxisIsUp=false;
     #region sim actor transforms
         [NonSerialized]internal Transform head;
         [NonSerialized]internal Transform  leftEye;
         [NonSerialized]internal Transform rightEye;
         [NonSerialized]internal Transform  leftHand;
         [NonSerialized]internal Transform rightHand;
         [NonSerialized]internal Transform  leftFoot;
         [NonSerialized]internal Transform rightFoot;
     #endregion
     [NonSerialized]internal LinkedListNode<SimObject>pooled; 
     [NonSerialized]internal(Type simObjectType,ulong idNumber)?id=null;
     [NonSerialized]internal(Type simObjectType,ulong idNumber)?masterId=null;
      [NonSerialized]protected SimObject masterSimObject;
     [NonSerialized]internal Rigidbody hasRigidbody;//  componente Rigidbody tem que ficar sempre no transform root
     [NonSerialized]internal Collider[]colliders;
     [NonSerialized]internal readonly List<Collider>volumeColliders=new List<Collider>();
      [SerializeField]internal SimCollisions simCollisionsPrefab;
       [NonSerialized]internal SimCollisions simCollisions;
     [NonSerialized]internal NavMeshObstacle[]navMeshObstacles;
      [NonSerialized]internal bool navMeshObstacleCarving;
     [NonSerialized]internal Bounds localBounds;
     [NonSerialized]protected readonly Vector3[]worldBoundsVertices=new Vector3[8];
     [NonSerialized]internal ParentConstraint parentConstraint;
        protected virtual void Awake(){
         math_random=new System.Random(seedGenerator.Next());
         netObj=GetComponent<NetworkObject>();
         hasRigidbody=transform.root.GetComponent<Rigidbody>();
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
         if(volumeColliders.Count>0&&simCollisionsPrefab){
          simCollisions=Instantiate(simCollisionsPrefab,transform).GetComponent<SimCollisions>();
          simCollisions.simObject=this;
          simCollisions.AddTriggers();
         }
         foreach(NavMeshObstacle navMeshObstacle in navMeshObstacles=GetComponentsInChildren<NavMeshObstacle>()){
          navMeshObstacleCarving|=navMeshObstacle.carving;
         }
         localBounds.center=transform.InverseTransformPoint(localBounds.center);
         TransformBoundsVertices();
         foreach(Renderer renderer in renderers=GetComponentsInChildren<Renderer>()){
          derivedMaterials.Add(renderer,new Dictionary<int,Dictionary<int,Material>>());
          derivedMaterialsBlendModeOpaque.Add(renderer,new List<Material>(renderer.materials.Length));
          derivedMaterialsBlendModeFade  .Add(renderer,new List<Material>(renderer.materials.Length));
          int i=0;
          foreach(Material material in renderer.materials){
           materialShader[material]=material.shader;
           derivedMaterials[renderer].Add(i,new Dictionary<int,Material>());
           Material derivedMaterial=new Material(material);
           derivedMaterials[renderer][i].Add(0,derivedMaterial);
           derivedMaterialsBlendModeOpaque[renderer].Add(derivedMaterial);
           derivedMaterial=new Material(material);
           derivedMaterial.shader=RenderingUtil.StandardShader;
           RenderingUtil.SetupStandardShaderMaterialBlendMode(derivedMaterial,RenderingUtil.BlendMode.Fade,true);
           derivedMaterials[renderer][i].Add(2,derivedMaterial);
           derivedMaterialsBlendModeFade  [renderer].Add(derivedMaterial);
           ++i;
          }
          if(useMultipleMaterials){
           renderer.SetMaterials(derivedMaterialsBlendModeOpaque[renderer]);
          }
          renderer.enabled=false;//  to prevent a "flashing" of the object when it's created
         }
         skillBuffs=gameObject.AddComponent<SkillBuffEffectsState>();
         skillBuffs.targetSimObject=this;
         waitForFixedUpdate=new WaitForFixedUpdate();
         parentConstraint=gameObject.GetComponent<ParentConstraint>();
         SetInteractionsList();
        }
        public override void OnDestroy(){
         foreach(var kvp1 in derivedMaterials){
          foreach(var kvp2 in kvp1.Value){
           foreach(var kvp3 in kvp2.Value){
            DestroyImmediate(kvp3.Value);
           }
          }
         }
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
         //Log.DebugMessage("SimObject:OnActivated:id:"+id);
         this.name=id.ToString();
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
           //Log.DebugMessage("SimObject:OnActivated:'netObj should be spawned now'");
           try{
            netObj.Spawn(destroyWithScene:false);
           }catch(Exception e){
            Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
           }
           netObj.DontDestroyWithOwner=true;
          }else if(IsOwner){
           //Log.DebugMessage("SimObject:OnActivated:'IsOwner, so set net variables'");
           netPosition.Value=persistentData.position  ;
           netRotation.Value=persistentData.rotation  ;
           netScale   .Value=persistentData.localScale;
          }
         }
         if(updateSafePositionCoroutine!=null){
          StopCoroutine(updateSafePositionCoroutine);updateSafePositionCoroutine=null;
         }
         updateSafePositionCoroutine=StartCoroutine(UpdateSafePosition());
        }
        internal virtual void OnDeactivated(){
         if(updateSafePositionCoroutine!=null){
          StopCoroutine(updateSafePositionCoroutine);updateSafePositionCoroutine=null;
         }
         skillBuffs.Clear();
         persistentStats.UpdateData(this);
         //Log.DebugMessage("SimObject:OnDeactivated:id:"+id);
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
         OnActAsNonInventorySimObject();
         foreach(Collider collider in colliders){
          collider.enabled=true;
         }
         interactionsEnabled=true;
         initManualUpdate=true;
         overlapper=false;
         foreach(var gameplayer in GameplayerManagement.singleton.all){
          ulong clientId=gameplayer.Key;
          if(colliders.Length>0){
           gameplayer.Value.OnSimObjectEnabled(this,colliders[0].gameObject.layer);
          }
         }
         if(simCollisions!=null){
          simCollisions.Activate();
         }
         EnableRenderers();
         if(actingAsInventoryItem){
          OnActAsInventoryItem();
         }
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
         if(simCollisions!=null){
          simCollisions.Deactivate();
         }
         DisableRenderers();
        }
        internal void OnUnplaceRequest(){
         unplaceRequested=true;
        }
        internal void OnPoolRequest(){
         poolRequested=true;
        }
     [SerializeField]bool DEBUG_UNPLACE=false;
     [NonSerialized]internal bool initManualUpdate;
     [NonSerialized]protected Vector3?safePosition;
     [NonSerialized]bool updateRenderersFlag;
     [NonSerialized]bool hadCollision;
     [NonSerialized]bool flaggedAsOverlapping;
     [NonSerialized](bool isOverlapping,bool isOverlapped)overlapState;
      [NonSerialized]internal bool overlapper;
     [NonSerialized]bool unplaceRequested;
     [NonSerialized]bool checkIfOutOfSight;
     [NonSerialized]bool poolRequested;
        internal virtual int ManualUpdate(bool doValidationChecks){
         if(initManualUpdate){
          hadCollision=false;
          flaggedAsOverlapping=false;
          //Log.DebugMessage("'initManualUpdate'");
          initManualUpdate=false;
          overlapState=IsOverlappingOrOverlapped(instantCheck:true);
          //Log.DebugMessage("'initManualUpdate':isOverlapping:"+isOverlapping);
          if(overlapState.isOverlapping){
           safePosition=null;
          }else{
           safePosition=transform.position;
          }
         }
         if(DEBUG_UNPLACE){
            DEBUG_UNPLACE=false;
          OnUnplaceRequest();
         }
         if(masterId!=null&&(masterSimObject==null||masterSimObject.id==null||masterSimObject.id.Value!=masterId.Value)){
          //Log.DebugMessage("'master sim (id:"+masterId+") validation failed: renew masterObject with GetMaster()':'my id':id:"+id);
          masterSimObject=GetMaster();
          SetAsSlaveOf(masterSimObject);
         }
         int result=0;
         skillBuffs.ManualUpdate(Time.deltaTime);
         if(asInventoryItem!=null){
          result=3;
          //Log.DebugMessage("ManualUpdate:result:"+result+":'asInventoryItem'");
          if(asInventoryItem.container==null){
              Log.DebugMessage("ManualUpdate:'asInventoryItem -> invalid container'");
          }else{
              if(asInventoryItem.container.asSimObject==null){
                  Log.DebugMessage("ManualUpdate:'asInventoryItem.container.asSimObject==null'");
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
            if(NetworkManager!=null){
             try{
              netPosition.Value=persistentData.position  ;
              netRotation.Value=persistentData.rotation  ;
              netScale   .Value=persistentData.localScale;
             }catch(Exception e){
              Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
             }
            }
           }
          }
            transform.hasChanged=false;
          overlapState=IsOverlappingOrOverlapped();
          foreach(var gameplayer in GameplayerManagement.singleton.all){
           if(colliders.Length>0){
            gameplayer.Value.OnSimObjectTransformHasChanged(this,colliders[0].gameObject.layer);
           }
          }
         }
         if(hadCollision){
          hadCollision=false;
          overlapState=IsOverlappingOrOverlapped();
          //Log.DebugMessage("ManualUpdate:'hadCollision':isOverlapping:"+isOverlapping);
         }
         if(flaggedAsOverlapping){
          flaggedAsOverlapping=false;
          overlapState=IsOverlappingOrOverlapped();
         }
         bool returnedToSafePos=false;
         GetCollidersTouchingNonAlloc(instantCheck:false);
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
             if(overlapState.isOverlapping){
                overlapState=(false,overlapState.isOverlapped);
                 if(Core.singleton.isServer){
                  if(safePosition!=null){
                   Log.DebugMessage("id:"+id+":'return to safe pos':safePosition:..."+safePosition+":'currently at':transform.position:"+transform.position);
                   transform.position=safePosition.Value;
                   returnedToSafePos=!IsOverlappingOrOverlapped(instantCheck:true).overlapping;
                  }
                  if(!returnedToSafePos){
                   Log.DebugMessage("'failed to return to safe pos:simObject is overlapping':id:"+id,this);
                   DisableInteractions();
                   if(netObj.IsSpawned){
                    netObj.DontDestroyWithOwner=true;
                    netObj.Despawn(destroy:false);
                   }
                   SimObjectManager.singleton.deactivateAndReleaseIdQueue.Enqueue(this);
                   result=2;
                  }else{
                   //Log.DebugMessage("'returned to safe pos':id:"+id);
                   overlapper=false;
                  }
                 }
             }else{
                 if(IsDead()){
                     if(Core.singleton.isServer){
                      if(IsMotionComplete()){
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
                                //Log.DebugMessage("'should the ownership be changed to client id?':clientId:"+clientId);
                                if(gameplayer.Value!=Gameplayer.main&&IsInPlayerWorldBounds(gameplayer.Value)){
                                 Log.DebugMessage("'change ownership to client id':clientId:"+clientId);
                                 netObj.ChangeOwnership(clientId);
                                 netObj.DontDestroyWithOwner=true;
                                 ownershipChanged=true;
                                 break;
                                }
                               }
                               if(!ownershipChanged){
                                //Log.DebugMessage("'sim object is out of sight (IsOutOfSight)':id:"+id);
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
         }
         if(result==0){
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
        internal virtual bool OnTeleportTo(Vector3 position,Quaternion rotation){
         return false;
        }
        internal void OnCollision(Collider collider,SimObject colliderSimObject){
         if(Core.singleton.isServer){
          //Log.DebugMessage("'"+this.transform.root.gameObject.name+" -> was hit in collision with -> "+collider.transform.root.gameObject.name+"'",this);
          hadCollision=true;
         }
        }
     [NonSerialized]readonly Dictionary<Collider,SimObject>overlapsColliders=new();
        protected virtual(bool overlapping,bool overlapped)IsOverlappingOrOverlapped(bool instantCheck=false){
         if(IgnoreOverlaps()){
          return(false,false);
         }
         (bool overlapping,bool overlapped)result=(false,false);
         overlapsColliders.Clear();
         if(simCollisions){
          if(!instantCheck){
           simCollisions.GetOverlapCollisions(overlapsColliders);
          }else{
           simCollisions.GetOverlapsNonAlloc(overlapsColliders);
          }
         }
         //Log.DebugMessage("IsOverlappingOrOverlapped:overlapsColliders.Count:"+overlapsColliders.Count);
         foreach(var overlapCollider in overlapsColliders){
          //Log.DebugMessage("overlap:"+this.transform.root.gameObject.name+"-> overlap with <-"+overlapCollider.Key.transform.root.gameObject.name,this);
          var overlapResults=GetOverlapResults(overlapCollider.Key,overlapCollider.Value);
          //Log.DebugMessage("overlap:"+this.transform.root.gameObject.name+(overlapResults.overlapping?"-> overlapping ->":(overlapResults.overlapped?"<- overlapped <-":"<- no overlap ->"))+overlapCollider.Key.transform.root.gameObject.name);
          result=(
           result.overlapping||overlapResults.overlapping,
           result.overlapped ||overlapResults.overlapped 
          );
         }
         overlapper=result.overlapping;
         return result;
        }
        (bool overlapping,bool overlapped)GetOverlapResults(Collider overlapCollider,SimObject overlapSimObject){
         if(overlapSimObject!=null){
          //Log.DebugMessage("overlapSimObject.overlapper:"+overlapSimObject.overlapper,overlapSimObject);
          if(overlapSimObject.initManualUpdate||overlapSimObject.overlapper){
           return(false,false);
          }
          if(this is SimConstruction&&overlapSimObject is SimTree){
           overlapSimObject.OnOverlapping(this);
           return(false,true);
          }
          if(!overlapSimObject.IgnoreOverlaps()){
           return(true,false);
          }
         }
         return(false,false);
        }
        internal void OnOverlapping(SimObject overlappedSimObject){
         if(Core.singleton.isServer){
          //Log.DebugMessage("OnOverlapping:'event':'"+this.transform.root.gameObject.name+"-> overlapping ->"+overlappedSimObject.transform.root.gameObject.name+"'",this);
          flaggedAsOverlapping=true;
         }
        }
        internal virtual bool IgnoreOverlaps(){
         return(
          this is SimActor||
          this is SimWeapon||
          (this.hasRigidbody!=null&&!this.hasRigidbody.isKinematic)
         );
        }
     [NonSerialized]protected bool gotCollidersTouchingFromInstantCheck;
     [NonSerialized]protected Collider[]collidersTouching=new Collider[8];
        protected virtual void GetCollidersTouchingNonAlloc(bool instantCheck){
         gotCollidersTouchingFromInstantCheck=false;
        }
        protected virtual bool IsOutOfSight(){
         //Log.DebugMessage("id:"+id+":'test if is out of sight (IsOutOfSight)'");
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
           //Log.DebugMessage("IsInPlayerActiveWorldBounds:'testing vertice':v:"+v);
           return gameplayer.activeWorldBounds.Contains(v);
          }
         );
        }
        protected virtual bool IsInPlayerWorldBounds(Gameplayer gameplayer){
         return worldBoundsVertices.Any(
          v=>{
           //Log.DebugMessage("IsInPlayerWorldBounds:'testing vertice':v:"+v);
           return gameplayer.worldBounds.Contains(v);
          }
         );
        }
     [NonSerialized]protected Coroutine updateSafePositionCoroutine;
     [NonSerialized]protected WaitForFixedUpdate waitForFixedUpdate;
        protected IEnumerator UpdateSafePosition(){
         Loop:{
          yield return null;
          yield return waitForFixedUpdate;
          if(Core.singleton.isServer){
           if(!initManualUpdate&&!overlapState.isOverlapping&&simCollisions.simObjectColliders.Count<=0){
            if(transform.hasChanged){
             //Log.DebugMessage("UpdateSafePosition:id:"+id+":safePosition:"+safePosition);
             safePosition=transform.position;
            }
           }
          }
         }
         goto Loop;
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