#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Gameplaying;
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Trees;
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
     internal static System.Random seedGenerator;
     internal System.Random math_random;
     internal Queue<NetworkObject>clientSidePooling=null;
     internal Gameplayer owner;
     [SerializeField]internal bool ZAxisIsUp=false;
     internal Transform head;
     internal Transform  leftEye;
     internal Transform rightEye;
     internal Transform  leftHand;
     internal Transform rightHand;
     internal Transform  leftFoot;
     internal Transform rightFoot;
     internal LinkedListNode<SimObject>pooled; 
     internal(Type simObjectType,ulong idNumber)?id=null;
     internal(Type simObjectType,ulong idNumber)?masterId=null;
      protected SimObject masterSimObject;
     //  TO DO: componente Rigidbody tem que ficar sempre no transform root
     internal Rigidbody hasRigidbody;
     internal Collider[]colliders;
     internal readonly List<Collider>volumeColliders=new List<Collider>();
      [SerializeField]internal SimCollisions simCollisionsPrefab;
      internal SimCollisions simCollisions;
     internal NavMeshObstacle[]navMeshObstacles;
      internal bool navMeshObstacleCarving;
     internal Bounds localBounds;
     protected readonly Vector3[]worldBoundsVertices=new Vector3[8];
     internal ParentConstraint parentConstraint;
        protected virtual void Awake(){
         math_random=new System.Random(seedGenerator.Next());
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
           try{
            netObj.Spawn(destroyWithScene:false);
           }catch(Exception e){
            Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
           }
           netObj.DontDestroyWithOwner=true;
          }else if(IsOwner){
           Log.DebugMessage("set net variables");
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
         //Log.DebugMessage("OnDeactivated:id:"+id);
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
         isOverlapping=IsOverlappingNonAlloc(instantCheck:true);
         //Log.DebugMessage("isOverlappingReversal:"+isOverlappingReversal);
         isOverlapping|=isOverlappingReversal;
         isOverlappingReversal=false;
         //Log.DebugMessage("EnableInteractions:isOverlapping:"+isOverlapping);
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
     Vector3?safePosition;
     [NonSerialized]bool updateRenderersFlag;
     [NonSerialized]bool isOverlapping;
      [NonSerialized]bool isOverlappingReversal;
     [NonSerialized]bool unplaceRequested;
     [NonSerialized]bool checkIfOutOfSight;
     [NonSerialized]bool poolRequested;
        internal virtual int ManualUpdate(bool doValidationChecks){
         if(DEBUG_UNPLACE){
            DEBUG_UNPLACE=false;
          OnUnplaceRequest();
         }
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
          isOverlapping|=IsOverlappingNonAlloc(instantCheck:false);
          foreach(var gameplayer in GameplayerManagement.singleton.all){
           if(colliders.Length>0){
            gameplayer.Value.OnSimObjectTransformHasChanged(this,colliders[0].gameObject.layer);
           }
          }
         }
         //Log.DebugMessage("isOverlappingReversal:"+isOverlappingReversal);
         isOverlapping|=isOverlappingReversal;
         isOverlappingReversal=false;
         //Log.DebugMessage("isOverlapping:"+isOverlapping);
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
             if(isOverlapping){
                isOverlapping=false;
                 if(Core.singleton.isServer){
                  if(safePosition!=null){
                   Log.DebugMessage(id+":return to safe pos;"+"safePosition:"+safePosition+";transform.position:"+transform.position);
                   transform.position=safePosition.Value;
                   returnedToSafePos=!IsOverlappingNonAlloc(instantCheck:true);
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
                  }else{
                   Log.DebugMessage("returnedToSafePos:id:"+id);
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
     [NonSerialized]private Collider[]overlappedColliders=new Collider[8];
        protected virtual bool IsOverlappingNonAlloc(bool instantCheck){
         if(hasRigidbody!=null&&!hasRigidbody.isKinematic){
          return false;
         }
         if(this is SimActor){
          return false;
         }
         bool result=false;
         if(!instantCheck){
          if(simCollisions){
           foreach(Collider overlappedCollider in simCollisions.simObjectColliders){
            result|=SetOverlapResult(overlappedCollider);
           }
           return result;
          }
         }
         for(int i=0;i<volumeColliders.Count;++i){
          int overlappingsLength=0;
          if(volumeColliders[i]is CapsuleCollider capsule){
           var values=simCollisions.GetCapsuleValuesForCollisionTesting(capsule,transform.root);
           _GetOverlappedColliders:{
            overlappingsLength=Physics.OverlapCapsuleNonAlloc(
             values.point0,
             values.point1,
             values.radius,
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
            if(overlappedCollider.transform.root!=this.transform.root){//  it's not myself
             result|=SetOverlapResult(overlappedCollider);
            }
           }
          }
         }
         return result;
        }
        internal void OnOverlapping(Collider overlappedCollider){
         Log.DebugMessage("OnOverlapping:"+this.transform.root.gameObject.name+"-> overlapping <-"+overlappedCollider.transform.root.gameObject.name);
         if(Core.singleton.isServer){
          if(!overlappedCollider.transform.root.hasChanged){
           isOverlapping|=SetOverlapResult(overlappedCollider);
          }
         }
        }
        bool SetOverlapResult(Collider overlappedCollider){
         SimObject overlappedSimObject=overlappedCollider.GetComponentInParent<SimObject>();
         if(overlappedSimObject!=null){
          if(this is SimConstruction&&overlappedSimObject is SimTree){
           overlappedSimObject.OnOverlappingReversal(this);
           return false;
          }
          if(!(overlappedSimObject is SimActor||(overlappedSimObject.hasRigidbody!=null&&!overlappedSimObject.hasRigidbody.isKinematic))){
           return true;
          }
         }
         return false;
        }
        internal void OnOverlappingReversal(SimObject overlappedSimObject){
         Log.DebugMessage(this+":OnOverlappingReversal");
         isOverlappingReversal=true;
         safePosition=null;
         //Log.DebugMessage("isOverlappingReversal:"+isOverlappingReversal);
        }
     protected bool gotCollidersTouchingFromInstantCheck;
     protected Collider[]collidersTouching=new Collider[8];
        protected virtual void GetCollidersTouchingNonAlloc(bool instantCheck){
         gotCollidersTouchingFromInstantCheck=false;
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
     protected Coroutine updateSafePositionCoroutine;
     protected WaitForFixedUpdate waitForFixedUpdate;
        protected IEnumerator UpdateSafePosition(){
         Loop:{
          yield return null;
          yield return waitForFixedUpdate;
          if(Core.singleton.isServer){
           if(!isOverlapping&&simCollisions.simObjectColliders.Count<=0){
            if(transform.hasChanged){
             //Log.DebugMessage("UpdateSafePosition:id:"+id+";safePosition:"+safePosition);
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