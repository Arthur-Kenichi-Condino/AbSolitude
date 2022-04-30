#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SimObjectSpawner:MonoBehaviour{
     internal static SimObjectSpawner Singleton;
        private void Awake(){
         if(Singleton==null){Singleton=this;}else{DestroyImmediate(this);return;}
        }
     internal readonly Dictionary<Type,GameObject>SimObjectPrefabs=new Dictionary<Type,GameObject>();
        internal void Init(){
         FileStream releasedIdsFileStream=SimObjectManager.Singleton.persistentDataSavingBGThread.releasedIdsFileStream=new FileStream(SimObjectManager.releasedIdsFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
         StreamWriter releasedIdsFileStreamWriter=SimObjectManager.Singleton.persistentDataSavingBGThread.releasedIdsFileStreamWriter=new StreamWriter(releasedIdsFileStream);
         StreamReader releasedIdsFileStreamReader=SimObjectManager.Singleton.persistentDataSavingBGThread.releasedIdsFileStreamReader=new StreamReader(releasedIdsFileStream);
         releasedIdsFileStream.Position=0L;
         releasedIdsFileStreamReader.DiscardBufferedData();
         string line;
         releasedIdsFileStream.Position=0L;
         releasedIdsFileStreamReader.DiscardBufferedData();
         FileStream idsFileStream=SimObjectManager.Singleton.persistentDataSavingBGThread.idsFileStream=new FileStream(SimObjectManager.idsFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
         StreamWriter idsFileStreamWriter=SimObjectManager.Singleton.persistentDataSavingBGThread.idsFileStreamWriter=new StreamWriter(idsFileStream);
         StreamReader idsFileStreamReader=SimObjectManager.Singleton.persistentDataSavingBGThread.idsFileStreamReader=new StreamReader(idsFileStream);
         idsFileStream.Position=0L;
         idsFileStreamReader.DiscardBufferedData();
         while((line=idsFileStreamReader.ReadLine())!=null){
          if(string.IsNullOrEmpty(line)){continue;}
          int typeStringStart=line.IndexOf("type=")+5;
          int typeStringEnd  =line.IndexOf(", ",typeStringStart);
          string typeString=line.Substring(typeStringStart,typeStringEnd-typeStringStart);
          Type t=Type.GetType(typeString);
          int nextIdStringStart=line.IndexOf("nextId=",typeStringEnd)+8;
          int nextIdStringEnd  =line.IndexOf(" }, ",nextIdStringStart);
          string nextIdString=line.Substring(nextIdStringStart,nextIdStringEnd-nextIdStringStart);
          ulong nextId=ulong.Parse(nextIdString);
          SimObjectManager.Singleton.ids[t]=nextId;
         }
         idsFileStream.Position=0L;
         idsFileStreamReader.DiscardBufferedData();
         foreach(var o in Resources.LoadAll("AKCondinoO/Sims/",typeof(GameObject))){
          GameObject gameObject=(GameObject)o;
           SimObject  simObject=gameObject.GetComponent<SimObject>();
          if(simObject==null)continue;
          Type t=simObject.GetType();
          SimObjectPrefabs.Add(t,gameObject);
          string saveFile=string.Format("{0}{1}{2}",Core.savePath,t,".txt");
          FileStream fileStream;
          SimObjectManager.Singleton.persistentDataSavingBGThread.fileStream[t]=fileStream=new FileStream(saveFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
          SimObjectManager.Singleton.persistentDataSavingBGThread.fileStreamWriter[t]=new StreamWriter(fileStream);
          SimObjectManager.Singleton.persistentDataSavingBGThread.fileStreamReader[t]=new StreamReader(fileStream);
          SimObjectManager.Singleton.persistentDataSavingBG.gameDataToSerializeToFile.Add(t,new Dictionary<ulong,SimObject.PersistentData>());
          SimObjectManager.Singleton.persistentDataSavingBG.idsToRelease.Add(t,new List<ulong>());
          SimObjectManager.Singleton.persistentDataSavingBG.persistentIds.Add(t,0);
          SimObjectManager.Singleton.persistentDataSavingBG.persistentReleasedIds.Add(t,new List<ulong>());
          SimObjectManager.Singleton.releasedIds.Add(t,new List<ulong>());
          SimObjectManager.Singleton.pool.Add(t,new LinkedList<SimObject>());
         }
         spawnCoroutine=StartCoroutine(SpawnCoroutine());
         Core.Singleton.OnDestroyingCoreEvent+=OnDestroyingCoreEvent;
        }
        void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimObjectSpawner:OnDestroyingCoreEvent");
        }
        void OnDestroy(){
        }
     Coroutine spawnCoroutine;
     [SerializeField]int       DEBUG_CREATE_SIM_OBJECT_AMOUNT=1;
     [SerializeField]Vector3   DEBUG_CREATE_SIM_OBJECT_ROTATION;
     [SerializeField]Vector3   DEBUG_CREATE_SIM_OBJECT_POSITION;
     [SerializeField]Vector3   DEBUG_CREATE_SIM_OBJECT_SCALE=Vector3.one;
     [SerializeField]SimObject DEBUG_CREATE_SIM_OBJECT=null;
     internal readonly Queue<SpawnData>              SpawnQueue=new Queue<SpawnData>();
     internal readonly Queue<SimObject>            DespawnQueue=new Queue<SimObject>();
     internal readonly Queue<SimObject>DespawnAndReleaseIdQueue=new Queue<SimObject>();
     readonly SpawnData spawnData=new SpawnData();
     bool savingPersistentData;
     bool loadingPersistentData;
     float reloadInterval=5f;
     float reloadTimer;
        void Update(){
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
                (Vector3 position,Vector3 rotation,Vector3 scale,Type type,ulong?id)at=(DEBUG_CREATE_SIM_OBJECT_POSITION,DEBUG_CREATE_SIM_OBJECT_ROTATION,DEBUG_CREATE_SIM_OBJECT_SCALE,t,null);
                spawnData.at.Add(at);
               }
               spawnData.dequeued=false;
               SpawnQueue.Enqueue(spawnData);
                DEBUG_CREATE_SIM_OBJECT=null;
              }
             }
             while(SpawnQueue.Count>0){SpawnData toSpawn=SpawnQueue.Dequeue();
              foreach(var at in toSpawn.at){
               _GetId:{}
               Type simType=at.type;
               ulong number;
               (Type simType,ulong number)id;
               bool numberIsNew=false;
               bool numberIsRecycled=false;
               var releasedIds=SimObjectManager.Singleton.releasedIds[simType];
               if(at.id==null){
                number=0;
                var ids=SimObjectManager.Singleton.ids;
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
               }
               id=(simType,number);
               if(SimObjectManager.Singleton.spawned.ContainsKey(id)){
                Log.DebugMessage("SpawnCoroutine:id already spawned:"+id);
                if(numberIsNew||numberIsRecycled){
                 goto _GetId;
                }
                continue;
               }
               GameObject gameObject;
                SimObject  simObject;
               var pool=SimObjectManager.Singleton.pool[at.type];
               if(pool.Count>0){
                 simObject=pool.First.Value;
                pool.RemoveFirst();
                 simObject.pooled=null;
                gameObject=simObject.gameObject;
               }else{
                gameObject=Instantiate(SimObjectPrefabs[at.type]);
                 simObject=gameObject.GetComponent<SimObject>();
               }
               gameObject.transform.position=at.position;
               gameObject.transform.rotation=Quaternion.Euler(at.rotation);
               gameObject.transform.localScale=at.scale;
               SimObjectManager.Singleton.spawned.Add(id,simObject);
               SimObjectManager.Singleton.active .Add(id,simObject);
                simObject.id=id;
                simObject.OnActivated();
              }
              toSpawn.at.Clear();
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
                                reloadTimer=reloadInterval;
                                OnPersistentDataPullingFromFile();
                            }
                        }else{
                            if(DespawnQueue.Count>0||DespawnAndReleaseIdQueue.Count>0){
                                Log.DebugMessage("DespawnQueue.Count>0||DespawnAndReleaseIdQueue.Count>0");
                                if(OnPersistentDataPushToFile()){
                                    OnPersistentDataPushedToFile();
                                }
                            }else{
                            }
                        }
                    }
                }
            }
            goto Loop;
        }
        bool OnPersistentDataPushToFile(){
         if(SimObjectManager.Singleton.persistentDataSavingBG.IsCompleted(SimObjectManager.Singleton.persistentDataSavingBGThread.IsRunning)){
          CollectSavingData();
          PersistentDataSavingMultithreaded.Schedule(SimObjectManager.Singleton.persistentDataSavingBG);
          return true;
         }
         return false;
        }
        internal void CollectSavingData(bool exitSave=false){
         foreach(var kvp in SimObjectManager.Singleton.releasedIds){
          Type t=kvp.Key;
          SimObjectManager.Singleton.persistentDataSavingBG.persistentReleasedIds[t].AddRange(kvp.Value);
          SimObjectManager.Singleton.ids.TryGetValue(t,out ulong nextId);
          SimObjectManager.Singleton.persistentDataSavingBG.persistentIds[t]=nextId;
         }
         foreach(var a in SimObjectManager.Singleton.active){
          SimObjectManager.Singleton.persistentDataSavingBG.gameDataToSerializeToFile[  a.Value.id.Value.simType].Add(  a.Value.id.Value.number,  a.Value.persistentData);
         }
         while(DespawnQueue.Count>0){var toDespawn=DespawnQueue.Dequeue();
          SimObjectManager.Singleton.persistentDataSavingBG.gameDataToSerializeToFile[toDespawn.id.Value.simType].Add(toDespawn.id.Value.number,toDespawn.persistentData);
          if(!exitSave){
           SimObjectManager.Singleton.despawning.Add(toDespawn.id.Value,toDespawn);
          }
         }
         while(DespawnAndReleaseIdQueue.Count>0){var toDespawnAndReleaseId=DespawnAndReleaseIdQueue.Dequeue();
          SimObjectManager.Singleton.persistentDataSavingBG.idsToRelease[toDespawnAndReleaseId.id.Value.simType].Add(toDespawnAndReleaseId.id.Value.number);
          if(!exitSave){
           SimObjectManager.Singleton.despawningAndReleasingId.Add(toDespawnAndReleaseId.id.Value,toDespawnAndReleaseId);
          }
         }
        }
        void OnPersistentDataPushedToFile(){
         savingPersistentData=true;
        }
        bool OnPersistentDataSaved(){
         if(SimObjectManager.Singleton.persistentDataSavingBG.IsCompleted(SimObjectManager.Singleton.persistentDataSavingBGThread.IsRunning)){
          foreach(var despawn in SimObjectManager.Singleton.despawning){
           SimObjectManager.Singleton.spawned.Remove(despawn.Key);
           despawn.Value.pooled=SimObjectManager.Singleton.pool[despawn.Value.id.Value.simType].AddLast(despawn.Value);
           despawn.Value.id=null;
          }
          SimObjectManager.Singleton.despawning.Clear();
          foreach(var despawnAndReleaseId in SimObjectManager.Singleton.despawningAndReleasingId){
           SimObjectManager.Singleton.spawned.Remove(despawnAndReleaseId.Key);
           SimObjectManager.Singleton.releasedIds[despawnAndReleaseId.Key.simType].Add(despawnAndReleaseId.Key.number);
           despawnAndReleaseId.Value.pooled=SimObjectManager.Singleton.pool[despawnAndReleaseId.Value.id.Value.simType].AddLast(despawnAndReleaseId.Value);
           despawnAndReleaseId.Value.id=null;
          }
          SimObjectManager.Singleton.despawningAndReleasingId.Clear();
          return true;
         }
         return false;
        }
        bool OnPersistentDataPullFromFile(){
         if(SimObjectManager.Singleton.persistentDataLoadingBG.IsCompleted(SimObjectManager.Singleton.persistentDataLoadingBGThread.IsRunning)){
          PersistentDataLoadingMultithreaded.Schedule(SimObjectManager.Singleton.persistentDataLoadingBG);
          return true;
         }
         return false;
        }
        void OnPersistentDataPullingFromFile(){
         loadingPersistentData=true;
        }
        bool OnPersistentDataLoaded(){
         if(SimObjectManager.Singleton.persistentDataLoadingBG.IsCompleted(SimObjectManager.Singleton.persistentDataLoadingBGThread.IsRunning)){
          return true;
         }
         return false;
        }
    }
}