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
using System.IO;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SimObjectSpawner:MonoBehaviour,ISingletonInitialization{
     internal static SimObjectSpawner singleton{get;set;}
     internal static string simActorSavePath;
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
     internal readonly Dictionary<Type,GameObject>simObjectPrefabs=new Dictionary<Type,GameObject>();
        public void Init(){
         simActorSavePath=string.Format("{0}{1}",Core.savePath,"SimActor/");
         Directory.CreateDirectory(simActorSavePath);
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
         foreach(var o in Resources.LoadAll("AKCondinoO/Sims/",typeof(GameObject))){
          GameObject gameObject=(GameObject)o;
           SimObject  simObject=gameObject.GetComponent<SimObject>();
          if(simObject==null)continue;
          Type t=simObject.GetType();
          simObjectPrefabs.Add(t,gameObject);
          string saveFile=string.Format("{0}{1}{2}",Core.savePath,t,".txt");
          FileStream fileStream;
          SimObjectManager.singleton.persistentDataSavingBGThread.simObjectFileStream[t]=fileStream=new FileStream(saveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
          SimObjectManager.singleton.persistentDataSavingBGThread.simObjectFileStreamWriter[t]=new StreamWriter(fileStream);
          SimObjectManager.singleton.persistentDataSavingBGThread.simObjectFileStreamReader[t]=new StreamReader(fileStream);
          SimObjectManager.singleton.persistentDataSavingBG.simObjectDataToSerializeToFile.Add(t,new Dictionary<ulong,SimObject.PersistentData>());
          fileStream=null;
          string simActorSaveFile=string.Format("{0}{1}{2}",simActorSavePath,t,".txt");
          if(SimObjectUtil.IsSimActor(t)){
           FileStream simActorFileStream;
           SimObjectManager.singleton.persistentDataSavingBGThread.simActorFileStream[t]=simActorFileStream=new FileStream(simActorSaveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
           SimObjectManager.singleton.persistentDataSavingBGThread.simActorFileStreamWriter[t]=new StreamWriter(simActorFileStream);
           SimObjectManager.singleton.persistentDataSavingBGThread.simActorFileStreamReader[t]=new StreamReader(simActorFileStream);
           SimObjectManager.singleton.persistentDataSavingBG.simActorDataToSerializeToFile.Add(t,new Dictionary<ulong,SimActor.PersistentSimActorData>());
          }
          SimObjectManager.singleton.persistentDataSavingBG.idsToRelease.Add(t,new List<ulong>());
          SimObjectManager.singleton.persistentDataSavingBG.persistentIds.Add(t,0);
          SimObjectManager.singleton.persistentDataSavingBG.persistentReleasedIds.Add(t,new List<ulong>());
          FileStream loaderFileStream;
          SimObjectManager.singleton.persistentDataLoadingBGThread.fileStream[t]=loaderFileStream=new FileStream(saveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
          SimObjectManager.singleton.persistentDataLoadingBGThread.fileStreamReader[t]=new StreamReader(loaderFileStream);
          if(!SimObjectManager.singleton.releasedIds.ContainsKey(t)){
              SimObjectManager.singleton.releasedIds.Add(t,new List<ulong>());
          }
          SimObjectManager.singleton.pool.Add(t,new LinkedList<SimObject>());
         }
         spawnCoroutine=StartCoroutine(SpawnCoroutine());
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimObjectSpawner:OnDestroyingCoreEvent");
         if(this!=null){
          StopCoroutine(spawnCoroutine);
         }
        }
        void OnDestroy(){
        }
     readonly HashSet<int>terraincnkIdxPhysMeshBaked=new HashSet<int>();
        internal void OnVoxelTerrainChunkPhysMeshBaked(VoxelTerrainChunk cnk){
         terraincnkIdxPhysMeshBaked.Add(cnk.id.Value.cnkIdx);
        }
     readonly Dictionary<(Type simType,ulong number),(Vector3 position,Vector3 eulerAngles,Vector3 localScale)>specificSpawnRequests=new Dictionary<(Type,ulong),(Vector3,Vector3,Vector3)>();
        internal void OnSpecificSpawnRequestAt((Type simType,ulong number)id,Vector3 position,Vector3 eulerAngles,Vector3 localScale){
         Log.DebugMessage("OnSpecificSpawnRequestAt:id:"+id);
         specificSpawnRequests[id]=(position,eulerAngles,localScale);
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
         if(SimObjectManager.singleton.persistentDataSavingBG.IsCompleted(SimObjectManager.singleton.persistentDataSavingBGThread.IsRunning)){
          CollectSavingData();
          PersistentDataSavingMultithreaded.Schedule(SimObjectManager.singleton.persistentDataSavingBG);
          return true;
         }
         return false;
        }
        internal void CollectSavingData(bool exitSave=false){
         foreach(var kvp in SimObjectManager.singleton.releasedIds){
          Type t=kvp.Key;
          SimObjectManager.singleton.persistentDataSavingBG.persistentReleasedIds[t].AddRange(kvp.Value);
          SimObjectManager.singleton.ids.TryGetValue(t,out ulong nextId);
          SimObjectManager.singleton.persistentDataSavingBG.persistentIds[t]=nextId;
         }
         foreach(var a in SimObjectManager.singleton.active){
          SimObjectManager.singleton.persistentDataSavingBG.simObjectDataToSerializeToFile[  a.Value.id.Value.simType].Add(  a.Value.id.Value.number,  a.Value.persistentData);
          if(a.Value is SimActor simActor){
           SimObjectManager.singleton.persistentDataSavingBG.simActorDataToSerializeToFile[  a.Value.id.Value.simType].Add(  a.Value.id.Value.number,simActor.persistentSimActorData);
          }
         }
         while(despawnQueue.Count>0){var toDespawn=despawnQueue.Dequeue();
          SimObjectManager.singleton.persistentDataSavingBG.simObjectDataToSerializeToFile[toDespawn.id.Value.simType].Add(toDespawn.id.Value.number,toDespawn.persistentData);
          if(toDespawn is SimActor simActor){
           SimObjectManager.singleton.persistentDataSavingBG.simActorDataToSerializeToFile[toDespawn.id.Value.simType].Add(toDespawn.id.Value.number,simActor.persistentSimActorData);
          }
          if(!exitSave){
           SimObjectManager.singleton.despawning.Add(toDespawn.id.Value,toDespawn);
          }
         }
         while(despawnAndReleaseIdQueue.Count>0){var toDespawnAndReleaseId=despawnAndReleaseIdQueue.Dequeue();
          SimObjectManager.singleton.persistentDataSavingBG.idsToRelease[toDespawnAndReleaseId.id.Value.simType].Add(toDespawnAndReleaseId.id.Value.number);
          if(!exitSave){
           SimObjectManager.singleton.despawningAndReleasingId.Add(toDespawnAndReleaseId.id.Value,toDespawnAndReleaseId);
          }
         }
        }
        void OnPersistentDataPushedToFile(){
         savingPersistentData=true;
        }
        bool OnPersistentDataSaved(){
         if(SimObjectManager.singleton.persistentDataSavingBG.IsCompleted(SimObjectManager.singleton.persistentDataSavingBGThread.IsRunning)){
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
          return true;
         }
         return false;
        }
        bool OnPersistentDataPullFromFile(){
         if(SimObjectManager.singleton.persistentDataLoadingBG.IsCompleted(SimObjectManager.singleton.persistentDataLoadingBGThread.IsRunning)){
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
          PersistentDataLoadingMultithreaded.Schedule(SimObjectManager.singleton.persistentDataLoadingBG);
          return true;
         }
         return false;
        }
        void OnPersistentDataPullingFromFile(){
         reloadTimer=reloadInterval;
         loadingPersistentData=true;
        }
        bool OnPersistentDataLoaded(){
         if(SimObjectManager.singleton.persistentDataLoadingBG.IsCompleted(SimObjectManager.singleton.persistentDataLoadingBGThread.IsRunning)){
          spawnQueue.Enqueue(SimObjectManager.singleton.persistentDataLoadingBG.spawnDataFromFiles);
          return true;
         }
         return false;
        }
    }
}