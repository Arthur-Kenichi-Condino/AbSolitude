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
using System.Linq;
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
           int simObjectTypeStringStart=line.IndexOf("type=")+5;
           int simObjectTypeStringEnd  =line.IndexOf(" , ",simObjectTypeStringStart);
           string simObjectTypeString=line.Substring(simObjectTypeStringStart,simObjectTypeStringEnd-simObjectTypeStringStart);
           Type simObjectType=Type.GetType(simObjectTypeString);
           if(simObjectType==null){continue;}
           SimObjectManager.singleton.releasedIds[simObjectType]=new HashSet<ulong>();
           int releasedIdsListStringStart=line.IndexOf("{ ",simObjectTypeStringEnd)+2;
           int releasedIdsListStringEnd  =line.IndexOf(", } , } , endOfLine",releasedIdsListStringStart);
           if(releasedIdsListStringEnd>=0){
            string releasedIdsListString=line.Substring(releasedIdsListStringStart,releasedIdsListStringEnd-releasedIdsListStringStart);
            string[]simObjectIdNumberStrings=releasedIdsListString.Split(',');
            foreach(string simObjectIdNumberString in simObjectIdNumberStrings){
             //Log.DebugMessage("adding releasedId of "+t+": "+idString+"...");
             ulong simObjectIdNumber=ulong.Parse(simObjectIdNumberString,NumberStyles.Any,CultureInfoUtil.en_US);
             SimObjectManager.singleton.releasedIds[simObjectType].Add(simObjectIdNumber);
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
           int simObjectTypeStringStart=line.IndexOf("type=")+5;
           int simObjectTypeStringEnd  =line.IndexOf(" , ",simObjectTypeStringStart);
           string simObjectTypeString=line.Substring(simObjectTypeStringStart,simObjectTypeStringEnd-simObjectTypeStringStart);
           Type simObjectType=Type.GetType(simObjectTypeString);
           if(simObjectType==null){continue;}
           int nextSimObjectIdNumberStringStart=line.IndexOf("nextId=",simObjectTypeStringEnd)+7;
           int nextSimObjectIdNumberStringEnd  =line.IndexOf(" } , endOfLine",nextSimObjectIdNumberStringStart);
           string nextSimObjectIdNumberString=line.Substring(nextSimObjectIdNumberStringStart,nextSimObjectIdNumberStringEnd-nextSimObjectIdNumberStringStart);
           ulong nextSimObjectIdNumber=ulong.Parse(nextSimObjectIdNumberString,NumberStyles.Any,CultureInfoUtil.en_US);
           SimObjectManager.singleton.ids[simObjectType]=nextSimObjectIdNumber;
          }
          idsFileStream.Position=0L;
          idsFileStreamReader.DiscardBufferedData();
         }
         foreach(var o in Resources.LoadAll("AKCondinoO/Prefabs/Network/Sims/",typeof(GameObject))){
          GameObject gameObject=(GameObject)o;
           SimObject  simObject=gameObject.GetComponent<SimObject>();
          if(simObject==null)continue;
          Type simObjectType=simObject.GetType();
          simObjectPrefabs.Add(simObjectType,gameObject);
          string simObjectSaveFile=null;
          if(Core.singleton.isServer){
           simObjectSaveFile=string.Format("{0}{1}{2}",SimObjectManager.simObjectDataSavePath,simObjectType,".txt");
           FileStream simObjectFileStream;
           SimObjectManager.singleton.persistentDataSavingBGThread.simObjectFileStream[simObjectType]=simObjectFileStream=new FileStream(simObjectSaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
           SimObjectManager.singleton.persistentDataSavingBGThread.simObjectFileStreamWriter[simObjectType]=new StreamWriter(simObjectFileStream);
           SimObjectManager.singleton.persistentDataSavingBGThread.simObjectFileStreamReader[simObjectType]=new StreamReader(simObjectFileStream);
          }
          SimObjectManager.singleton.persistentDataSavingBG.simObjectDataToSerializeToFile.Add(simObjectType,new Dictionary<ulong,SimObject.PersistentData>());
          string simActorSaveFile=null;
          if(SimObjectUtil.IsSimActor(simObjectType)){
           if(Core.singleton.isServer){
            simActorSaveFile=string.Format("{0}{1}{2}",SimObjectManager.simActorDataSavePath,simObjectType,".txt");
            FileStream simActorFileStream;
            SimObjectManager.singleton.persistentDataSavingBGThread.simActorFileStream[simObjectType]=simActorFileStream=new FileStream(simActorSaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
            SimObjectManager.singleton.persistentDataSavingBGThread.simActorFileStreamWriter[simObjectType]=new StreamWriter(simActorFileStream);
            SimObjectManager.singleton.persistentDataSavingBGThread.simActorFileStreamReader[simObjectType]=new StreamReader(simActorFileStream);
           }
           SimObjectManager.singleton.persistentDataSavingBG.simActorDataToSerializeToFile.Add(simObjectType,new Dictionary<ulong,SimActor.PersistentSimActorData>());
          }
          string simInventorySaveFile=null;
          if(Core.singleton.isServer){
           simInventorySaveFile=string.Format("{0}{1}{2}",SimInventoryManager.simInventoryDataSavePath,simObjectType,".txt");
           FileStream simInventoryFileStream;
           SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.simInventoryFileStream[simObjectType]=simInventoryFileStream=new FileStream(simInventorySaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
           SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.simInventoryFileStreamWriter[simObjectType]=new StreamWriter(simInventoryFileStream);
           SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.simInventoryFileStreamReader[simObjectType]=new StreamReader(simInventoryFileStream);
          }
          SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.releasedSimObjects.Add(simObjectType,new List<ulong>());
          SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.simInventoryDataToSerializeToFile.Add(simObjectType,new Dictionary<ulong,Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>>());
           #region output
               SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.simInventoryReleasedSimObjectsIdsToRelease.Add(simObjectType,new List<ulong>());
           #endregion
          SimObjectManager.singleton.persistentDataSavingBG.idsToRelease.Add(simObjectType,new HashSet<ulong>());
          SimObjectManager.singleton.persistentDataSavingBG.persistentIds.Add(simObjectType,0);
          SimObjectManager.singleton.persistentDataSavingBG.persistentReleasedIds.Add(simObjectType,new HashSet<ulong>());
          SimObjectManager.singleton.persistentDataSavingBG.onSavedReleasedIds.Add(simObjectType,new HashSet<ulong>());
          if(Core.singleton.isServer){
           FileStream loaderSimObjectFileStream;
           SimObjectManager.singleton.persistentDataLoadingBGThread.simObjectFileStream[simObjectType]=loaderSimObjectFileStream=new FileStream(simObjectSaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
           SimObjectManager.singleton.persistentDataLoadingBGThread.simObjectFileStreamReader[simObjectType]=new StreamReader(loaderSimObjectFileStream);
          }
          if(SimObjectUtil.IsSimActor(simObjectType)){
           if(Core.singleton.isServer){
            FileStream loaderSimActorFileStream;
            SimObjectManager.singleton.persistentDataLoadingBGThread.simActorFileStream[simObjectType]=loaderSimActorFileStream=new FileStream(simActorSaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
            SimObjectManager.singleton.persistentDataLoadingBGThread.simActorFileStreamReader[simObjectType]=new StreamReader(loaderSimActorFileStream);
           }
          }
          if(Core.singleton.isServer){
           FileStream loaderSimInventoryFileStream;
           SimInventoryManager.singleton.persistentSimInventoryDataLoadingBGThread.simInventoryFileStream[simObjectType]=loaderSimInventoryFileStream=new FileStream(simInventorySaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
           SimInventoryManager.singleton.persistentSimInventoryDataLoadingBGThread.simInventoryFileStreamReader[simObjectType]=new StreamReader(loaderSimInventoryFileStream);
          }
          if(!SimObjectManager.singleton.releasedIds.ContainsKey(simObjectType)){
              SimObjectManager.singleton.releasedIds.Add(simObjectType,new HashSet<ulong>());
          }
          SimObjectManager.singleton.pool.Add(simObjectType,new LinkedList<SimObject>());
         }
         //Log.DebugMessage("load thumbnails");
         foreach(var o in Resources.LoadAll("AKCondinoO/Prefabs/Network/Sims/",typeof(Texture))){
          //Log.DebugMessage("load Texture, o.name:"+o.name);
          string typeName=o.name.Substring(0,o.name.IndexOf("."));
          //Log.DebugMessage("load Texture, typeName:"+typeName);
          Type t=ReflectionUtil.GetTypeByName(typeName,typeof(SimObject));
          //Log.DebugMessage("load Texture, t:"+t);
          if(t!=null){
           Log.DebugMessage("register Texture for SimObject of Type t:"+t);
          }
         }
         simInventoryItemsInContainerSettings.Set();
         if(Core.singleton.isServer){
          spawnCoroutine=StartCoroutine(SpawnCoroutine());
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         //Log.DebugMessage("SimObjectSpawner:OnDestroyingCoreEvent");
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
     readonly Dictionary<(Type simObjectType,ulong idNumber),(Vector3 position,Vector3 eulerAngles,Vector3 localScale,(Type simObjectType,ulong idNumber)?asInventoryItemOwnerId)>specificSpawnRequests=new Dictionary<(Type,ulong),(Vector3,Vector3,Vector3,(Type,ulong)?)>();
        internal void OnSpecificSpawnRequestAt((Type simObjectType,ulong idNumber)id,Vector3 position,Vector3 eulerAngles,Vector3 localScale,(Type simObjectType,ulong idNumber)?asInventoryItemOwnerId=null){
         Log.DebugMessage("OnSpecificSpawnRequestAt:id:"+id);
         //  TO DO: move to destination if already spawned because it won't be spawned again
         specificSpawnRequests[id]=(position,eulerAngles,localScale,asInventoryItemOwnerId);
        }
        internal void OnSpecificSpawnRequestAt(SpawnData spawnData){
         Log.DebugMessage("OnSpecificSpawnRequestAt:spawnData");
         //  TO DO: move to destination if already spawned because it won't be spawned again
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
     float loaderCooldown=.5f;
     float loaderOnCooldownTimer;
     float reloadInterval=1f;
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
                (Vector3 position,Vector3 rotation,Vector3 scale,Type simObjectType,ulong?idNumber,SimObject.PersistentData persistentData)at=(DEBUG_CREATE_SIM_OBJECT_POSITION,DEBUG_CREATE_SIM_OBJECT_ROTATION,DEBUG_CREATE_SIM_OBJECT_SCALE,t,null,new SimObject.PersistentData());
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
               SimObject.PersistentStats?persistentStats=null;
               SimActor.PersistentSimActorData?persistentSimActorData=null;
               if(SimObjectUtil.IsSimActor(at.simObjectType)){
                //Log.DebugMessage("SimObjectUtil.IsSimActor(at.type)");
                if(toSpawn.actorData.TryGetValue(index,out var persistentSimActorDataValue)){
                 persistentSimActorData=persistentSimActorDataValue;
                }
               }
               (Type simObjectType,ulong idNumber)?masterId=null;
               if(toSpawn.masters.TryGetValue(index,out var masterIdValue)){
                masterId=masterIdValue;
               }
               if(masterId==null&&at.persistentData.masterId!=null){
                masterId=at.persistentData.masterId;
                //Log.DebugMessage("persistentData masterId:"+masterId);
               }
               (Type simObjectType,ulong idNumber)?asInventoryItemOwnerId=null;
               if(toSpawn.asInventoryItemOwnerIds.TryGetValue(index,out var asInventoryItemOwnerIdValue)){
                asInventoryItemOwnerId=asInventoryItemOwnerIdValue;
               }
               index++;
               _GetId:{}
               Type simObjectType=at.simObjectType;
               ulong number;
               (Type simObjectType,ulong idNumber)id;
               bool numberIsNew=false;
               bool numberIsRecycled=false;
               var releasedIds=SimObjectManager.singleton.releasedIds[simObjectType];
               if(at.idNumber==null){
                number=0;
                var ids=SimObjectManager.singleton.ids;
                if(!ids.ContainsKey(simObjectType)){
                 ids.Add(simObjectType,1);
                 numberIsNew=true;
                }else{
                 if(releasedIds.Count>0){
                  number=releasedIds.Last();
                  releasedIds.Remove(number);
                  numberIsRecycled=true;
                 }else{
                  number=ids[simObjectType]++;
                  numberIsNew=true;
                 }
                }
               }else{
                number=at.idNumber.Value;
                var ids=SimObjectManager.singleton.ids;
                if(!ids.ContainsKey(simObjectType)||number>=ids[simObjectType]){
                 ids[simObjectType]=number+1;
                }
                releasedIds.Remove(number);
               }
               id=(simObjectType,number);
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
               var pool=SimObjectManager.singleton.pool[at.simObjectType];
               if(pool.Count>0){
                 simObject=pool.First.Value;
                pool.RemoveFirst();
                 simObject.pooled=null;
                gameObject=simObject.gameObject;
               }else{
                gameObject=Instantiate(simObjectPrefabs[at.simObjectType]);
                 simObject=gameObject.GetComponent<SimObject>();
               }
               gameObject.transform.position=at.position;
               gameObject.transform.rotation=Quaternion.Euler(at.rotation);
               gameObject.transform.localScale=at.scale;
               SimObjectManager.singleton.spawned    .Add(id,simObject);
               SimObjectManager.singleton.active     .Add(id,simObject);
                simObject.id=id;
                simObject.OnSpawned();
                if(masterId!=null){
                 //Log.DebugMessage("simObject has master");
                 simObject.masterId=masterId;
                }else{
                 //Log.DebugMessage("simObject has no master");
                 simObject.masterId=null;
                }
                if(simObject is SimActor simActor){
                 if(persistentSimActorData!=null){
                  //Log.DebugMessage("set simActor.persistentSimActorData from loaded value");
                  simActor.persistentSimActorData=persistentSimActorData.Value;
                 }else{
                  //Log.DebugMessage("clear simActor.persistentSimActorData");
                  simActor.persistentSimActorData=new SimActor.PersistentSimActorData();
                 }
                 SimObjectManager.singleton.activeActor.Add(id,simActor);
                 SimsMachine.singleton.OnActorSpawn(simActor);
                }
                if(asInventoryItemOwner!=null){
                 Log.DebugMessage("add simObject asInventoryItem to Owner");
                 if(!asInventoryItemOwner.AddToInventory(simObject)){
                  Log.DebugMessage("couldn't add to inventory, set simObject as a released world object");
                 }
                }
                if(persistentStats!=null){
                 //Log.DebugMessage("simObject persistentStats loaded");
                 simObject.persistentStats=persistentStats.Value;
                 simObject.stats.InitFrom(simObject.persistentStats,simObject);
                }else{
                 //Log.DebugMessage("simObject persistentStats must be generated");
                 simObject.persistentStats=new SimObject.PersistentStats();
                 simObject.stats.Generate(simObject,true);
                }
                simObject.OnActivated();
              }
              toSpawn.Clear();
              toSpawn.dequeued=true;
             }
                 if(loadingPersistentData){
                     //Log.DebugMessage("loadingPersistentData");
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
                             //Log.DebugMessage("reloadTimer<=0f");
                             if(OnPersistentDataPullFromFile()){
                                 OnPersistentDataPullingFromFile();
                             }
                         }else{
                             if(loaderOnCooldownTimer<=0f&&(terraincnkIdxPhysMeshBaked.Count>0||specificSpawnRequests.Count>0)){
                                 //Log.DebugMessage("(terraincnkIdxPhysMeshBaked.Count>0||specificSpawnRequests.Count>0)");
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
         foreach(Type simInventoryType in SimInventoryManager.singleton.simInventoryTypesPendingRegistrationForDataSaving){
          SimInventoryManager.singleton.OnSaveSimInventoryTypeRegistration(simInventoryType);
         }
         SimInventoryManager.singleton.simInventoryTypesPendingRegistrationForDataSaving.Clear();
         foreach(var kvp in SimObjectManager.singleton.releasedIds){
          Type simObjectType=kvp.Key;
          SimObjectManager.singleton.persistentDataSavingBG.persistentReleasedIds[simObjectType].UnionWith(kvp.Value);
          SimObjectManager.singleton.ids.TryGetValue(simObjectType,out ulong nextId);
          SimObjectManager.singleton.persistentDataSavingBG.persistentIds[simObjectType]=nextId;
         }
          foreach(var kvp in SimInventoryManager.singleton.releasedIds){
           Type simInventoryType=kvp.Key;
           SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.persistentReleasedIds[simInventoryType].UnionWith(kvp.Value);
           SimInventoryManager.singleton.ids.TryGetValue(simInventoryType,out ulong nextId);
           SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.persistentIds[simInventoryType]=nextId;
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
          if(exitSave){
           simObject.OnExitSaveDataCollection();
          }
          SimObjectManager.singleton.persistentDataSavingBG.simObjectDataToSerializeToFile[simObject.id.Value.simObjectType].Add(simObject.id.Value.idNumber,simObject.persistentData);
          if(simObject is SimActor simActor){
           SimObjectManager.singleton.persistentDataSavingBG.simActorDataToSerializeToFile[simObject.id.Value.simObjectType].Add(simObject.id.Value.idNumber,simActor .persistentSimActorData);
          }
          if(!PersistentSimInventoryDataSavingBackgroundContainer.simInventoryDataBySimInventoryTypeDictionaryPool.TryDequeue(out var simInventoryDataBySimInventoryTypeDictionary)){
           simInventoryDataBySimInventoryTypeDictionary=new Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>();
          }
          //  TO DO: save and load all inventories represented by this sim object
          foreach(var simInventoryTypeSimInventoryByIdNumberDictionaryPair in simObject.inventory){
           Type simInventoryType=simInventoryTypeSimInventoryByIdNumberDictionaryPair.Key;
           if(!PersistentSimInventoryDataSavingBackgroundContainer.simInventoryDataByIdNumberDictionaryPool.TryDequeue(out var simInventoryDataByIdNumberDictionary)){
            simInventoryDataByIdNumberDictionary=new Dictionary<ulong,SimInventory.PersistentSimInventoryData>();
           }
           foreach(var simInventoryIdNumberSimInventoryPair in simInventoryTypeSimInventoryByIdNumberDictionaryPair.Value){
            //  TO DO: ignorar inventário vazio e dar release id somente se estiver dando despawn (ou saindo, porque é um despawn também)
            if(isDespawning||exitSave){
             if(simInventoryIdNumberSimInventoryPair.Value.maxItemsCount<=0){
              //  TO DO: sim inventory ids to release
              SetSimInventoryToBeUnassignedAndHaveIdReleased(simInventoryType,simInventoryIdNumberSimInventoryPair.Key,simInventoryIdNumberSimInventoryPair.Value);
              continue;
             }
            }
            simInventoryDataByIdNumberDictionary.Add(simInventoryIdNumberSimInventoryPair.Key,simInventoryIdNumberSimInventoryPair.Value.persistentSimInventoryData);
           }
           if(simInventoryDataByIdNumberDictionary.Count>0){
            simInventoryDataBySimInventoryTypeDictionary.Add(simInventoryType,simInventoryDataByIdNumberDictionary);
           }else{
            PersistentSimInventoryDataSavingBackgroundContainer.simInventoryDataByIdNumberDictionaryPool.Enqueue(simInventoryDataByIdNumberDictionary);
           }
          }
          SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.simInventoryDataToSerializeToFile[simObject.id.Value.simObjectType].Add(simObject.id.Value.idNumber,simInventoryDataBySimInventoryTypeDictionary);
         }
         while(despawnAndReleaseIdQueue.Count>0){var toDespawnAndReleaseId=despawnAndReleaseIdQueue.Dequeue();
          SetSimObjectIdToBeReleased(toDespawnAndReleaseId.id.Value.simObjectType,toDespawnAndReleaseId.id.Value.idNumber);
          //  TO DO: sim inventory ids to release; release inventory items' ids
          foreach(var typeIdInventoryDictionaryPair in toDespawnAndReleaseId.inventory){
           Type inventoryType=typeIdInventoryDictionaryPair.Key;
           foreach(var idInventoryPair in typeIdInventoryDictionaryPair.Value){
            SetSimInventoryToBeUnassignedAndHaveIdReleased(inventoryType,idInventoryPair.Key,idInventoryPair.Value);
           }
          }
          if(!exitSave){
           SimObjectManager.singleton.despawningAndReleasingId.Add(toDespawnAndReleaseId.id.Value,toDespawnAndReleaseId);
          }
         }
         void SetSimInventoryToBeUnassignedAndHaveIdReleased(Type simInventoryType,ulong simInventoryIdNumber,SimInventory simInventory){
          //  active: remove
          SimInventoryManager.singleton.active.Remove(simInventory.simInventoryId.Value);
          //  release inventory items
          List<(Type simObjectType,ulong idNumber)>simObjectIdsToRelease=simInventory.OnSetToBeUnassigned(exitSave);
          foreach((Type simObjectType,ulong idNumber)simObjectIdToRelease in simObjectIdsToRelease){
           SetSimObjectIdToBeReleased(simObjectIdToRelease.simObjectType,simObjectIdToRelease.idNumber);
          }
          simObjectIdsToRelease.Clear();
          SetSimInventoryIdToBeReleased(simInventoryType,simInventoryIdNumber);
          if(!exitSave){
           SimInventoryManager.singleton.unassigningAndReleasingId.Add((simInventoryType,simInventoryIdNumber),simInventory);
          }
         }
         void SetSimObjectIdToBeReleased(Type simObjectType,ulong simObjectIdNumber){
          if(!SimObjectManager.singleton.persistentDataSavingBG.onSavedReleasedIds[simObjectType].Contains(simObjectIdNumber)&&!SimObjectManager.singleton.persistentDataSavingBG.idsToRelease[simObjectType].Contains(simObjectIdNumber)){
           SimObjectManager.singleton.persistentDataSavingBG.idsToRelease[simObjectType].Add(simObjectIdNumber);
           Log.DebugMessage("release sim object id and remove all related save data during save:"+simObjectType+";idNumber:"+simObjectIdNumber);
          }else{
           Log.DebugMessage("sim object id was already released in the previous save, so don't duplicate the released id in the save file:"+simObjectType+";idNumber:"+simObjectIdNumber);
          }
         }
         void SetSimInventoryIdToBeReleased(Type simInventoryType,ulong simInventoryIdNumber){
          if(!SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.onSavedReleasedIds[simInventoryType].Contains(simInventoryIdNumber)&&!SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.idsToRelease[simInventoryType].Contains(simInventoryIdNumber)){
           //Log.DebugMessage("release sim inventory id and remove all related save data during save:"+simInventoryType+";idNumber:"+simInventoryIdNumber);
           SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.idsToRelease[simInventoryType].Add(simInventoryIdNumber);
          }else{
           Log.DebugMessage("sim inventory id was already released in the previous save, so don't duplicate the released id in the save file:"+simInventoryType+";idNumber:"+simInventoryIdNumber);
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
           if(despawn.Value is SimActor simActor){
            SimsMachine.singleton.OnActorDespawn(simActor);
           }
           SimObjectManager.singleton.spawned.Remove(despawn.Key);
           despawn.Value.OnDespawned();
           despawn.Value.pooled=SimObjectManager.singleton.pool[despawn.Value.id.Value.simObjectType].AddLast(despawn.Value);
           despawn.Value.id=null;
          }
          SimObjectManager.singleton.despawning.Clear();
          //  TO DO
          foreach(var simObjectTypeIdNumberListPair in SimObjectManager.singleton.persistentDataSavingBG.onSavedReleasedIds){
           Type simObjectType=simObjectTypeIdNumberListPair.Key;
           HashSet<ulong>onSavedReleasedIds=simObjectTypeIdNumberListPair.Value;
           foreach(ulong simObjectIdNumber in onSavedReleasedIds){
            (Type simObjectType,ulong idNumber)simObjectId=(simObjectType,simObjectIdNumber);
            if(SimObjectManager.singleton.active.TryGetValue(simObjectId,out SimObject simObject)){
             simObject.OnUnplaceRequest();
             Log.DebugMessage("unplace sim object released during save:"+simObjectType+";idNumber:"+simObjectIdNumber);
            }else if(!SimObjectManager.singleton.spawned.ContainsKey(simObjectId)){
             SimObjectManager.singleton.releasedIds[simObjectType].Add(simObjectIdNumber);
             Log.DebugMessage("sim object released during save isn't active and the id can be reused immediately:"+simObjectType+";idNumber:"+simObjectIdNumber);
            }
           }
          }
          foreach(var despawnAndReleaseId in SimObjectManager.singleton.despawningAndReleasingId){
           if(despawnAndReleaseId.Value is SimActor simActor){
            SimsMachine.singleton.OnActorDespawn(simActor);
           }
           SimObjectManager.singleton.spawned.Remove(despawnAndReleaseId.Key);
           SimObjectManager.singleton.releasedIds[despawnAndReleaseId.Key.simObjectType].Add(despawnAndReleaseId.Key.idNumber);
           despawnAndReleaseId.Value.OnDespawned();
           despawnAndReleaseId.Value.pooled=SimObjectManager.singleton.pool[despawnAndReleaseId.Value.id.Value.simObjectType].AddLast(despawnAndReleaseId.Value);
           despawnAndReleaseId.Value.id=null;
          }
          SimObjectManager.singleton.despawningAndReleasingId.Clear();
          foreach(var simInventoryTypeIdNumberListPair in SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.onSavedReleasedIds){
           Type simInventoryType=simInventoryTypeIdNumberListPair.Key;
           HashSet<ulong>onSavedReleasedIds=simInventoryTypeIdNumberListPair.Value;
           foreach(ulong simInventoryIdNumber in onSavedReleasedIds){
            (Type simInventoryType,ulong idNumber)simInventoryId=(simInventoryType,simInventoryIdNumber);
            if(SimInventoryManager.singleton.active.TryGetValue(simInventoryId,out SimInventory simInventory)){
             simInventory.asSimObject.OnUnplaceRequest();
             Log.DebugMessage("unplace sim inventory released during save:"+simInventoryType+";idNumber:"+simInventoryIdNumber);
            }else if(!SimInventoryManager.singleton.assigned.ContainsKey(simInventoryId)){
             SimInventoryManager.singleton.releasedIds[simInventoryType].Add(simInventoryIdNumber);
             Log.DebugMessage("sim inventory released during save isn't active and the id can be reused immediately:"+simInventoryType+";idNumber:"+simInventoryIdNumber);
            }
           }
          }
          foreach(var simInventoryReleaseId in SimInventoryManager.singleton.unassigningAndReleasingId){
           //  TO DO: remove id and add to pool
           SimInventoryManager.singleton.assigned.Remove(simInventoryReleaseId.Key);
           SimInventoryManager.singleton.releasedIds[simInventoryReleaseId.Key.simInventoryType].Add(simInventoryReleaseId.Key.idNumber);
           simInventoryReleaseId.Value.pooled=SimInventoryManager.singleton.pool[simInventoryReleaseId.Key.simInventoryType].AddLast(simInventoryReleaseId.Value);
           simInventoryReleaseId.Value.asSimObject.inventory[simInventoryReleaseId.Key.simInventoryType].Remove(simInventoryReleaseId.Key.idNumber);
           simInventoryReleaseId.Value.asSimObject  =null;
           simInventoryReleaseId.Value.asSimObjectId=null;
           simInventoryReleaseId.Value.simInventoryId=null;
          }
          SimInventoryManager.singleton.unassigningAndReleasingId.Clear();
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
           (Type simObjectType,ulong idNumber)id=specificSpawnRequest.Key;
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