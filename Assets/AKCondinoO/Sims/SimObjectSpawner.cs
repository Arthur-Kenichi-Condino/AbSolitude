#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObjectSpawner:MonoBehaviour,ISingletonInitialization{
     internal static SimObjectSpawner singleton{get;set;}
     internal readonly SimInventoryItemsInContainerSettings simInventoryItemsInContainerSettings=new SimInventoryItemsInContainerSettings();
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
     internal readonly Dictionary<Type,GameObject>simObjectPrefabs=new Dictionary<Type,GameObject>();
        public void Init(){
         if(Core.singleton.isServer){
          FileStream releasedIdsFileStream=SimObjectManager.singleton.persistentDataSavingBGThread.releasedIdsFileStream=new FileStream(SimObjectManager.releasedIdsFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
          StreamWriter releasedIdsFileStreamWriter=SimObjectManager.singleton.persistentDataSavingBGThread.releasedIdsFileStreamWriter=new StreamWriter(releasedIdsFileStream);
          StreamReader releasedIdsFileStreamReader=SimObjectManager.singleton.persistentDataSavingBGThread.releasedIdsFileStreamReader=new StreamReader(releasedIdsFileStream);
          releasedIdsFileStream.Position=0L;
          releasedIdsFileStreamReader.DiscardBufferedData();
          string line;
          while((line=releasedIdsFileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int typeStringStart=line.IndexOf("type=")+5;
           int typeStringEnd  =line.IndexOf(" , ",typeStringStart);
           string typeString=line.Substring(typeStringStart,typeStringEnd-typeStringStart);
           Type t=Type.GetType(typeString);
           if(t==null){continue;}
           SimObjectManager.singleton.releasedIds[t]=new List<ulong>();
           int releasedIdsListStringStart=line.IndexOf("{ ",typeStringEnd)+2;
           int releasedIdsListStringEnd  =line.IndexOf(", } , } , endOfLine",releasedIdsListStringStart);
           if(releasedIdsListStringEnd>=0){
            string releasedIdsListString=line.Substring(releasedIdsListStringStart,releasedIdsListStringEnd-releasedIdsListStringStart);
            string[]idStrings=releasedIdsListString.Split(',');
            foreach(string idString in idStrings){
             //Log.DebugMessage("adding releasedId of "+t+": "+idString+"...");
             ulong id=ulong.Parse(idString,NumberStyles.Any,CultureInfoUtil.en_US);
             SimObjectManager.singleton.releasedIds[t].Add(id);
            }
           }
          }
          releasedIdsFileStream.Position=0L;
          releasedIdsFileStreamReader.DiscardBufferedData();
          FileStream idsFileStream=SimObjectManager.singleton.persistentDataSavingBGThread.idsFileStream=new FileStream(SimObjectManager.idsFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
          StreamWriter idsFileStreamWriter=SimObjectManager.singleton.persistentDataSavingBGThread.idsFileStreamWriter=new StreamWriter(idsFileStream);
          StreamReader idsFileStreamReader=SimObjectManager.singleton.persistentDataSavingBGThread.idsFileStreamReader=new StreamReader(idsFileStream);
          idsFileStream.Position=0L;
          idsFileStreamReader.DiscardBufferedData();
          while((line=idsFileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int typeStringStart=line.IndexOf("type=")+5;
           int typeStringEnd  =line.IndexOf(" , ",typeStringStart);
           string typeString=line.Substring(typeStringStart,typeStringEnd-typeStringStart);
           Type t=Type.GetType(typeString);
           if(t==null){continue;}
           int nextIdStringStart=line.IndexOf("nextId=",typeStringEnd)+7;
           int nextIdStringEnd  =line.IndexOf(" } , endOfLine",nextIdStringStart);
           string nextIdString=line.Substring(nextIdStringStart,nextIdStringEnd-nextIdStringStart);
           ulong nextId=ulong.Parse(nextIdString,NumberStyles.Any,CultureInfoUtil.en_US);
           SimObjectManager.singleton.ids[t]=nextId;
          }
          idsFileStream.Position=0L;
          idsFileStreamReader.DiscardBufferedData();
         }
         foreach(var o in Resources.LoadAll("AKCondinoO/Sims/",typeof(GameObject))){
          GameObject gameObject=(GameObject)o;
           SimObject  simObject=gameObject.GetComponent<SimObject>();
          if(simObject==null)continue;
          Type t=simObject.GetType();
          simObjectPrefabs.Add(t,gameObject);
          string simObjectSaveFile=null;
          if(Core.singleton.isServer){
           simObjectSaveFile=string.Format("{0}{1}{2}",SimObjectManager.simObjectDataSavePath,t,".txt");
           FileStream simObjectFileStream;
           SimObjectManager.singleton.persistentDataSavingBGThread.simObjectFileStream[t]=simObjectFileStream=new FileStream(simObjectSaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
           SimObjectManager.singleton.persistentDataSavingBGThread.simObjectFileStreamWriter[t]=new StreamWriter(simObjectFileStream);
           SimObjectManager.singleton.persistentDataSavingBGThread.simObjectFileStreamReader[t]=new StreamReader(simObjectFileStream);
          }
          SimObjectManager.singleton.persistentDataSavingBG.simObjectDataToSerializeToFile.Add(t,new Dictionary<ulong,SimObject.PersistentData>());
          string simActorSaveFile=null;
          if(SimObjectUtil.IsSimActor(t)){
           if(Core.singleton.isServer){
            simActorSaveFile=string.Format("{0}{1}{2}",SimObjectManager.simActorDataSavePath,t,".txt");
            FileStream simActorFileStream;
            SimObjectManager.singleton.persistentDataSavingBGThread.simActorFileStream[t]=simActorFileStream=new FileStream(simActorSaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
            SimObjectManager.singleton.persistentDataSavingBGThread.simActorFileStreamWriter[t]=new StreamWriter(simActorFileStream);
            SimObjectManager.singleton.persistentDataSavingBGThread.simActorFileStreamReader[t]=new StreamReader(simActorFileStream);
           }
           SimObjectManager.singleton.persistentDataSavingBG.simActorDataToSerializeToFile.Add(t,new Dictionary<ulong,SimActor.PersistentSimActorData>());
          }
          string simInventorySaveFile=null;
          if(Core.singleton.isServer){
           simInventorySaveFile=string.Format("{0}{1}{2}",SimInventoryManager.simInventoryDataSavePath,t,".txt");
           FileStream simInventoryFileStream;
           SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.simInventoryFileStream[t]=simInventoryFileStream=new FileStream(simInventorySaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
           SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.simInventoryFileStreamWriter[t]=new StreamWriter(simInventoryFileStream);
           SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.simInventoryFileStreamReader[t]=new StreamReader(simInventoryFileStream);
          }
          SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.releasedSimObjects.Add(t,new List<ulong>());
          SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.simInventoryDataToSerializeToFile.Add(t,new Dictionary<ulong,Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>>());
           #region output
               SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.simInventoryReleasedSimObjectsIdsToRelease.Add(t,new List<ulong>());
           #endregion
          SimObjectManager.singleton.persistentDataSavingBG.idsToRelease.Add(t,new List<ulong>());
          SimObjectManager.singleton.persistentDataSavingBG.persistentIds.Add(t,0);
          SimObjectManager.singleton.persistentDataSavingBG.persistentReleasedIds.Add(t,new List<ulong>());
          if(Core.singleton.isServer){
           FileStream loaderSimObjectFileStream;
           SimObjectManager.singleton.persistentDataLoadingBGThread.simObjectFileStream[t]=loaderSimObjectFileStream=new FileStream(simObjectSaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
           SimObjectManager.singleton.persistentDataLoadingBGThread.simObjectFileStreamReader[t]=new StreamReader(loaderSimObjectFileStream);
          }
          if(SimObjectUtil.IsSimActor(t)){
           if(Core.singleton.isServer){
            FileStream loaderSimActorFileStream;
            SimObjectManager.singleton.persistentDataLoadingBGThread.simActorFileStream[t]=loaderSimActorFileStream=new FileStream(simActorSaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
            SimObjectManager.singleton.persistentDataLoadingBGThread.simActorFileStreamReader[t]=new StreamReader(loaderSimActorFileStream);
           }
          }
          if(Core.singleton.isServer){
           FileStream loaderSimInventoryFileStream;
           SimInventoryManager.singleton.persistentSimInventoryDataLoadingBGThread.simInventoryFileStream[t]=loaderSimInventoryFileStream=new FileStream(simInventorySaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
           SimInventoryManager.singleton.persistentSimInventoryDataLoadingBGThread.simInventoryFileStreamReader[t]=new StreamReader(loaderSimInventoryFileStream);
          }
          if(!SimObjectManager.singleton.releasedIds.ContainsKey(t)){
              SimObjectManager.singleton.releasedIds.Add(t,new List<ulong>());
          }
          SimObjectManager.singleton.pool.Add(t,new LinkedList<SimObject>());
         }
         Log.DebugMessage("load thumbnails");
         foreach(var o in Resources.LoadAll("AKCondinoO/Sims/",typeof(Texture))){
          Log.DebugMessage("load Texture, o.name:"+o.name);
          string typeName=o.name.Substring(0,o.name.IndexOf("."));
          Log.DebugMessage("load Texture, typeName:"+typeName);
          Type t=ReflectionUtil.GetTypeByName(typeName);
          Log.DebugMessage("load Texture, t:"+t);
          if(t!=null&&ReflectionUtil.IsTypeDerivedFrom(t,typeof(SimObject))){
           Log.DebugMessage("register Texture for SimObject of Type t:"+t);
          }
         }
         simInventoryItemsInContainerSettings.Set();
         if(Core.singleton.isServer){
          spawnCoroutine=StartCoroutine(SpawnCoroutine());
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimObjectSpawner:OnDestroyingCoreEvent");
         if(this!=null&&spawnCoroutine!=null){
          StopCoroutine(spawnCoroutine);
         }
        }
        void OnDestroy(){
        }
     readonly HashSet<int>terraincnkIdxPhysMeshBaked=new HashSet<int>();
        internal void OnVoxelTerrainChunkPhysMeshBaked(VoxelTerrainChunk cnk){
         terraincnkIdxPhysMeshBaked.Add(cnk.id.Value.cnkIdx);
        }
     readonly Dictionary<(Type simType,ulong number),(Vector3 position,Vector3 eulerAngles,Vector3 localScale,(Type simType,ulong number)?asInventoryItemOwnerId)>specificSpawnRequests=new Dictionary<(Type,ulong),(Vector3,Vector3,Vector3,(Type,ulong)?)>();
        internal void OnSpecificSpawnRequestAt((Type simType,ulong number)id,Vector3 position,Vector3 eulerAngles,Vector3 localScale,(Type simType,ulong number)?asInventoryItemOwnerId=null){
         Log.DebugMessage("OnSpecificSpawnRequestAt:id:"+id);
         specificSpawnRequests[id]=(position,eulerAngles,localScale,asInventoryItemOwnerId);
        }
        internal void OnSpecificSpawnRequestAt(SpawnData spawnData){
         Log.DebugMessage("OnSpecificSpawnRequestAt:spawnData");
         spawnQueue.Enqueue(spawnData);
        }
     Coroutine spawnCoroutine;
     [SerializeField]int       DEBUG_CREATE_SIM_OBJECT_AMOUNT=1;
     [SerializeField]Vector3   DEBUG_CREATE_SIM_OBJECT_ROTATION;
     [SerializeField]Vector3   DEBUG_CREATE_SIM_OBJECT_POSITION;
     [SerializeField]Vector3   DEBUG_CREATE_SIM_OBJECT_SCALE=Vector3.one;
     [SerializeField]SimObject DEBUG_CREATE_SIM_OBJECT=null;
     internal readonly Queue<SpawnData>              spawnQueue=new Queue<SpawnData>();
     internal readonly Queue<SimObject>            despawnQueue=new Queue<SimObject>();
     internal readonly Queue<SimObject>despawnAndReleaseIdQueue=new Queue<SimObject>();
     readonly SpawnData spawnData=new SpawnData();
     bool savingPersistentData;
     bool loadingPersistentData;
     float loaderCooldown=1f;
     float loaderOnCooldownTimer;
     float reloadInterval=5f;
     float reloadTimer;
        void Update(){
         if(loaderOnCooldownTimer>0f){
            loaderOnCooldownTimer-=Time.deltaTime;
         }
         if(reloadTimer>0f){
            reloadTimer-=Time.deltaTime;
         }
        }
        private IEnumerator SpawnCoroutine(){
            Loop:{
             yield return null;
             if(DEBUG_CREATE_SIM_OBJECT!=null){
              if(spawnData.dequeued){
               Log.DebugMessage("DEBUG_CREATE_SIM_OBJECT:"+DEBUG_CREATE_SIM_OBJECT+";amount:"+DEBUG_CREATE_SIM_OBJECT_AMOUNT);
               Type t=DEBUG_CREATE_SIM_OBJECT.GetType();
               for(int i=0;i<DEBUG_CREATE_SIM_OBJECT_AMOUNT;++i){
                (Vector3 position,Vector3 rotation,Vector3 scale,Type type,ulong?id,SimObject.PersistentData persistentData)at=(DEBUG_CREATE_SIM_OBJECT_POSITION,DEBUG_CREATE_SIM_OBJECT_ROTATION,DEBUG_CREATE_SIM_OBJECT_SCALE,t,null,new SimObject.PersistentData());
                spawnData.at.Add(at);
               }
               spawnData.dequeued=false;
               spawnQueue.Enqueue(spawnData);
                DEBUG_CREATE_SIM_OBJECT=null;
              }
             }
             while(spawnQueue.Count>0){SpawnData toSpawn=spawnQueue.Dequeue();
              int index=0;
              foreach(var at in toSpawn.at){
               SimActor.PersistentSimActorData?persistentSimActorData=null;
               if(SimObjectUtil.IsSimActor(at.type)){
                //Log.DebugMessage("SimObjectUtil.IsSimActor(at.type)");
                if(toSpawn.actorData.TryGetValue(index,out var data)){
                 persistentSimActorData=data;
                }
               }
               (Type simType,ulong number)?masterId=null;
               if(toSpawn.masters.TryGetValue(index,out var master)){
                masterId=master;
               }
               (Type simType,ulong number)?asInventoryItemOwnerId=null;
               if(toSpawn.asInventoryItemOwnerIds.TryGetValue(index,out var asInventoryItemOwnerIdValue)){
                asInventoryItemOwnerId=asInventoryItemOwnerIdValue;
               }
               index++;
               _GetId:{}
               Type simType=at.type;
               ulong number;
               (Type simType,ulong number)id;
               bool numberIsNew=false;
               bool numberIsRecycled=false;
               var releasedIds=SimObjectManager.singleton.releasedIds[simType];
               if(at.id==null){
                number=0;
                var ids=SimObjectManager.singleton.ids;
                if(!ids.ContainsKey(simType)){
                 ids.Add(simType,1);
                 numberIsNew=true;
                }else{
                 if(releasedIds.Count>0){
                  number=releasedIds[releasedIds.Count-1];
                  releasedIds.RemoveAt(releasedIds.Count-1);
                  numberIsRecycled=true;
                 }else{
                  number=ids[simType]++;
                  numberIsNew=true;
                 }
                }
               }else{
                number=at.id.Value;
                var ids=SimObjectManager.singleton.ids;
                if(!ids.ContainsKey(simType)||number>=ids[simType]){
                 ids[simType]=number+1;
                }
                releasedIds.Remove(number);
               }
               id=(simType,number);
               if(SimObjectManager.singleton.spawned.ContainsKey(id)){
                //Log.DebugMessage("SpawnCoroutine:id already spawned:"+id);
                if(numberIsNew||numberIsRecycled){
                 goto _GetId;
                }
                continue;
               }
               SimObject asInventoryItemOwner=null;
               if(asInventoryItemOwnerId!=null&&!SimObjectManager.singleton.spawned.TryGetValue(asInventoryItemOwnerId.Value,out asInventoryItemOwner)){
                Log.DebugMessage("owner id for this inventory item is not spawned;asInventoryItemOwnerId.Value:"+asInventoryItemOwnerId.Value);
                continue;
               }
               GameObject gameObject;
                SimObject  simObject;
               var pool=SimObjectManager.singleton.pool[at.type];
               if(pool.Count>0){
                 simObject=pool.First.Value;
                pool.RemoveFirst();
                 simObject.pooled=null;
                gameObject=simObject.gameObject;
               }else{
                gameObject=Instantiate(simObjectPrefabs[at.type]);
                 simObject=gameObject.GetComponent<SimObject>();
               }
               gameObject.transform.position=at.position;
               gameObject.transform.rotation=Quaternion.Euler(at.rotation);
               gameObject.transform.localScale=at.scale;
               SimObjectManager.singleton.spawned.Add(id,simObject);
               SimObjectManager.singleton.active .Add(id,simObject);
                simObject.id=id;
                if(masterId!=null){
                 Log.DebugMessage("simObject has master");
                 simObject.master=masterId;
                }else{
                 //Log.DebugMessage("simObject has no master");
                 simObject.master=null;
                }
                if(simObject is SimActor simActor){
                 if(persistentSimActorData!=null){
                  Log.DebugMessage("set simActor.persistentSimActorData from loaded value");
                  simActor.persistentSimActorData=persistentSimActorData.Value;
                 }else{
                  //Log.DebugMessage("clear simActor.persistentSimActorData");
                  simActor.persistentSimActorData=new SimActor.PersistentSimActorData();
                 }
                }
                if(asInventoryItemOwner!=null){
                 Log.DebugMessage("add simObject asInventoryItem to Owner");
                 if(!asInventoryItemOwner.AddToInventory(simObject)){
                  Log.DebugMessage("couldn't add to inventory, set simObject as a released world object");
                 }
                }
                simObject.OnActivated();
              }
              toSpawn.Clear();
              toSpawn.dequeued=true;
             }
                 if(loadingPersistentData){
                     Log.DebugMessage("loadingPersistentData");
                     if(OnPersistentDataLoaded()){
                         loadingPersistentData=false;
                     }
                 }else{
                     if(savingPersistentData){
                         Log.DebugMessage("savingPersistentData");
                         if(OnPersistentDataSaved()){
                             savingPersistentData=false;
                         }
                     }else{
                         if(reloadTimer<=0f){
                             Log.DebugMessage("reloadTimer<=0f");
                             if(OnPersistentDataPullFromFile()){
                                 OnPersistentDataPullingFromFile();
                             }
                         }else{
                             if(loaderOnCooldownTimer<=0f&&(terraincnkIdxPhysMeshBaked.Count>0||specificSpawnRequests.Count>0)){
                                 Log.DebugMessage("(terraincnkIdxPhysMeshBaked.Count>0||specificSpawnRequests.Count>0)");
                                 if(OnPersistentDataPullFromFile()){
                                     loaderOnCooldownTimer=loaderCooldown;
                                     OnPersistentDataPullingFromFile();
                                 }
                             }else{
                                 if(despawnQueue.Count>0||despawnAndReleaseIdQueue.Count>0){
                                     Log.DebugMessage("despawnQueue.Count>0||despawnAndReleaseIdQueue.Count>0");
                                     if(OnPersistentDataPushToFile()){
                                         OnPersistentDataPushedToFile();
                                     }
                                 }else{
                                 }
                             }
                         }
                     }
                 }
            }
            goto Loop;
        }
        bool OnPersistentDataPushToFile(){
         if(SimObjectManager.singleton.persistentDataSavingBG.IsCompleted(SimObjectManager.singleton.persistentDataSavingBGThread.IsRunning)&&
             SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.IsCompleted(SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.IsRunning)
         ){
          CollectSavingData();
          SimObjectManager.singleton.SchedulePersistentDataSaving();
          return true;
         }
         return false;
        }
        internal void CollectSavingData(bool exitSave=false){
         foreach(Type t in SimInventoryManager.singleton.simInventoryTypesPendingRegistrationForDataSaving){
          SimInventoryManager.singleton.OnSaveSimInventoryTypeRegistration(t);
         }
         SimInventoryManager.singleton.simInventoryTypesPendingRegistrationForDataSaving.Clear();
         foreach(var kvp in SimObjectManager.singleton.releasedIds){
          Type t=kvp.Key;
          SimObjectManager.singleton.persistentDataSavingBG.persistentReleasedIds[t].AddRange(kvp.Value);
          SimObjectManager.singleton.ids.TryGetValue(t,out ulong nextId);
          SimObjectManager.singleton.persistentDataSavingBG.persistentIds[t]=nextId;
         }
          foreach(var kvp in SimInventoryManager.singleton.releasedIds){
           Type t=kvp.Key;
           SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.persistentReleasedIds[t].AddRange(kvp.Value);
           SimInventoryManager.singleton.ids.TryGetValue(t,out ulong nextId);
           SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.persistentIds[t]=nextId;
          }
         foreach(var a in SimObjectManager.singleton.active){
          GetSimObjectPersistentData(a.Value);
         }
         while(despawnQueue.Count>0){var toDespawn=despawnQueue.Dequeue();
          GetSimObjectPersistentData(toDespawn,true);
          if(!exitSave){
           SimObjectManager.singleton.despawning.Add(toDespawn.id.Value,toDespawn);
          }
         }
         void GetSimObjectPersistentData(SimObject simObject,bool isDespawning=false){
          SimObjectManager.singleton.persistentDataSavingBG.simObjectDataToSerializeToFile[simObject.id.Value.simType].Add(simObject.id.Value.number,simObject.persistentData);
          if(simObject is SimActor simActor){
           SimObjectManager.singleton.persistentDataSavingBG.simActorDataToSerializeToFile[simObject.id.Value.simType].Add(simObject.id.Value.number,simActor .persistentSimActorData);
          }
          if(!PersistentSimInventoryDataSavingBackgroundContainer.simInventoryDataTypeDictionaryPool.TryDequeue(out var simInventoryDataTypeDictionary)){
           simInventoryDataTypeDictionary=new Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>();
          }
          //  TO DO: save and load all inventories represented by this sim object
          foreach(var typeIdInventoryDictionaryPair in simObject.inventory){
           Type inventoryType=typeIdInventoryDictionaryPair.Key;
           if(!PersistentSimInventoryDataSavingBackgroundContainer.simInventoryDataIdDictionaryPool.TryDequeue(out var simInventoryDataIdDictionary)){
            simInventoryDataIdDictionary=new Dictionary<ulong,SimInventory.PersistentSimInventoryData>();
           }
           foreach(var idInventoryPair in typeIdInventoryDictionaryPair.Value){
            //  TO DO: ignorar inventário vazio e dar release id somente se estiver dando despawn (ou saindo, porque é um despawn também)
            if(isDespawning||exitSave){
             if(idInventoryPair.Value.maxItemsCount<=0){
              //  TO DO: sim inventory ids to release
              SetSimInventoryToReleaseId(inventoryType,idInventoryPair.Key,idInventoryPair.Value);
              continue;
             }
            }
            simInventoryDataIdDictionary.Add(idInventoryPair.Key,idInventoryPair.Value.persistentSimInventoryData);
           }
           if(simInventoryDataIdDictionary.Count>0){
            simInventoryDataTypeDictionary.Add(inventoryType,simInventoryDataIdDictionary);
           }else{
            PersistentSimInventoryDataSavingBackgroundContainer.simInventoryDataIdDictionaryPool.Enqueue(simInventoryDataIdDictionary);
           }
          }
          SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.simInventoryDataToSerializeToFile[simObject.id.Value.simType].Add(simObject.id.Value.number,simInventoryDataTypeDictionary);
         }
         while(despawnAndReleaseIdQueue.Count>0){var toDespawnAndReleaseId=despawnAndReleaseIdQueue.Dequeue();
          SimObjectManager.singleton.persistentDataSavingBG.idsToRelease[toDespawnAndReleaseId.id.Value.simType].Add(toDespawnAndReleaseId.id.Value.number);
          //  TO DO: sim inventory ids to release; release inventory items' ids
          foreach(var typeIdInventoryDictionaryPair in toDespawnAndReleaseId.inventory){
           Type inventoryType=typeIdInventoryDictionaryPair.Key;
           foreach(var idInventoryPair in typeIdInventoryDictionaryPair.Value){
            SetSimInventoryToReleaseId(inventoryType,idInventoryPair.Key,idInventoryPair.Value);
           }
          }
          if(!exitSave){
           SimObjectManager.singleton.despawningAndReleasingId.Add(toDespawnAndReleaseId.id.Value,toDespawnAndReleaseId);
          }
         }
         void SetSimInventoryToReleaseId(Type simInventoryType,ulong number,SimInventory simInventory){
          //  active: remove
          SimInventoryManager.singleton.active.Remove(simInventory.simInventoryId.Value);
          //  release inventory items
          List<(Type simType,ulong number)>simObjectIdsToRelease=simInventory.OnUnassign(exitSave);
          foreach((Type simType,ulong number)simObjectIdToRelease in simObjectIdsToRelease){
           SimObjectManager.singleton.persistentDataSavingBG.idsToRelease[simObjectIdToRelease.simType].Add(simObjectIdToRelease.number);
          }
          simObjectIdsToRelease.Clear();
          if(!SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.onSavedReleasedIds[simInventoryType].Contains(number)){
           SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.idsToRelease[simInventoryType].Add(number);
          }
          if(!exitSave){
           SimInventoryManager.singleton.unassigningAndReleasingId.Add((simInventoryType,number),simInventory);
          }
         }
        }
        void OnPersistentDataPushedToFile(){
         savingPersistentData=true;
        }
        bool OnPersistentDataSaved(){
         if(SimObjectManager.singleton.persistentDataSavingBG.IsCompleted(SimObjectManager.singleton.persistentDataSavingBGThread.IsRunning)&&
             SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.IsCompleted(SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.IsRunning)
         ){
          //  TO DO: remove sim inventories
          foreach(var despawn in SimObjectManager.singleton.despawning){
           SimObjectManager.singleton.spawned.Remove(despawn.Key);
           despawn.Value.pooled=SimObjectManager.singleton.pool[despawn.Value.id.Value.simType].AddLast(despawn.Value);
           despawn.Value.id=null;
          }
          SimObjectManager.singleton.despawning.Clear();
          foreach(var despawnAndReleaseId in SimObjectManager.singleton.despawningAndReleasingId){
           SimObjectManager.singleton.spawned.Remove(despawnAndReleaseId.Key);
           SimObjectManager.singleton.releasedIds[despawnAndReleaseId.Key.simType].Add(despawnAndReleaseId.Key.number);
           despawnAndReleaseId.Value.pooled=SimObjectManager.singleton.pool[despawnAndReleaseId.Value.id.Value.simType].AddLast(despawnAndReleaseId.Value);
           despawnAndReleaseId.Value.id=null;
          }
          SimObjectManager.singleton.despawningAndReleasingId.Clear();
          foreach(var simInventoryReleaseId in SimInventoryManager.singleton.unassigningAndReleasingId){
           //  TO DO: remove id and add to pool
           SimInventoryManager.singleton.assigned.Remove(simInventoryReleaseId.Key);
           SimInventoryManager.singleton.releasedIds[simInventoryReleaseId.Key.simInventoryType].Add(simInventoryReleaseId.Key.number);
           simInventoryReleaseId.Value.pooled=SimInventoryManager.singleton.pool[simInventoryReleaseId.Key.simInventoryType].AddLast(simInventoryReleaseId.Value);
           simInventoryReleaseId.Value.asSimObject.inventory[simInventoryReleaseId.Key.simInventoryType].Remove(simInventoryReleaseId.Key.number);
           simInventoryReleaseId.Value.asSimObject  =null;
           simInventoryReleaseId.Value.asSimObjectId=null;
           simInventoryReleaseId.Value.simInventoryId=null;
          }
          SimInventoryManager.singleton.unassigningAndReleasingId.Clear();
          foreach(var simInventoryTypeIdNumberListPair in SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.onSavedReleasedIds){
           Type simInventoryType=simInventoryTypeIdNumberListPair.Key;
           List<ulong>onSavedReleasedIds=simInventoryTypeIdNumberListPair.Value;
           for(int i=0;i<onSavedReleasedIds.Count;++i){ulong idNumber=onSavedReleasedIds[i];
            (Type simInventoryType,ulong number)simInventoryId=(simInventoryType,idNumber);
            if(SimInventoryManager.singleton.active.TryGetValue(simInventoryId,out SimInventory simInventory)){
             simInventory.asSimObject.OnUnplaceRequest();
            }else{
             SimInventoryManager.singleton.releasedIds[simInventoryType].Add(idNumber);
            }
           }
          }
          return true;
         }
         return false;
        }
        bool OnPersistentDataPullFromFile(){
         if(SimObjectManager.singleton.persistentDataLoadingBG.IsCompleted(SimObjectManager.singleton.persistentDataLoadingBGThread.IsRunning)&&
             SimInventoryManager.singleton.persistentSimInventoryDataLoadingBG.IsCompleted(SimInventoryManager.singleton.persistentSimInventoryDataLoadingBGThread.IsRunning)
         ){
          foreach(int cnkIdx in terraincnkIdxPhysMeshBaked){
           SimObjectManager.singleton.persistentDataLoadingBG.terraincnkIdxToLoad.Add(cnkIdx);
          }
          terraincnkIdxPhysMeshBaked.Clear();
          SimObjectManager.singleton.persistentDataLoadingBG.terraincnkIdxToLoad.RemoveWhere(
           cnkIdx=>{
            if(!VoxelSystem.singleton.terrainActive.TryGetValue(cnkIdx,out VoxelTerrainChunk cnk)){
             return true;
            }
            if(!cnk.hasPhysMeshBaked){
             return true;
            }
            //  cnk is ready for sim objects
            return false;
           }
          );
          foreach(var specificSpawnRequest in specificSpawnRequests){
           (Type simType,ulong number)id=specificSpawnRequest.Key;
           var spawnRequestData=specificSpawnRequest.Value;
           SimObjectManager.singleton.persistentDataLoadingBG.specificIdsToLoad.Add(id,spawnRequestData);
          }
          specificSpawnRequests.Clear();
          SimObjectManager.singleton.SchedulePersistentDataLoading();
          return true;
         }
         return false;
        }
        void OnPersistentDataPullingFromFile(){
         reloadTimer=reloadInterval;
         loadingPersistentData=true;
        }
        bool OnPersistentDataLoaded(){
         if(SimObjectManager.singleton.persistentDataLoadingBG.IsCompleted(SimObjectManager.singleton.persistentDataLoadingBGThread.IsRunning)&&
             SimInventoryManager.singleton.persistentSimInventoryDataLoadingBG.IsCompleted(SimInventoryManager.singleton.persistentSimInventoryDataLoadingBGThread.IsRunning)
         ){
          spawnQueue.Enqueue(SimObjectManager.singleton.persistentDataLoadingBG.spawnDataFromFiles);
          return true;
         }
         return false;
        }
    }
}