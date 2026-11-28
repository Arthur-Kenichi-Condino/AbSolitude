using AKCondinoO.Bootstrap;
using AKCondinoO.PersistentData;
using AKCondinoO.Utilities;
using AKCondinoO.World;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.PersistentData.SpatialMapFiles;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.SimObjects{
    internal class SimObjectSaveSystem{
     internal Dictionary<SimObjectSaveKey,SimObjectSaveValue>pendingSave=new();
        internal readonly struct SimObjectSaveKey:IEquatable<SimObjectSaveKey>{
         internal readonly Type simObjectType;
         internal readonly string variant;
         internal readonly ulong id;
            internal SimObjectSaveKey(Type simObjectType,string variant,ulong id){
             this.simObjectType=simObjectType;
             this.variant=variant;
             this.id=id;
            }
            public bool Equals(SimObjectSaveKey other){
             return simObjectType==other.simObjectType&&
                    string.Equals(variant,other.variant,StringComparison.Ordinal)&&
                    id==other.id;
            }
            public override bool Equals(object obj){
             return obj is SimObjectSaveKey other&&Equals(other);
            }
            public override int GetHashCode(){
             unchecked{
              int hash=simObjectType?.GetHashCode()??0;
              hash=(hash*397)^(variant?.GetHashCode()??0);
              hash=(hash*397)^id.GetHashCode();
              return hash;
             }
            }
        }
        internal readonly struct SimObjectSaveValue:IEquatable<SimObjectSaveValue>{
         internal readonly SimObjectPersistentData persistentData;
         internal readonly bool exists;
            internal SimObjectSaveValue(SimObjectPersistentData persistentData,bool exists){
             this.persistentData=persistentData;
             this.exists=exists;
            }
            public bool Equals(SimObjectSaveValue other){
             return ReferenceEquals(persistentData,other.persistentData)&&exists==other.exists;
            }
            public override bool Equals(object obj){
             return obj is SimObjectSaveValue other&&Equals(other);
            }
            public override int GetHashCode(){
             unchecked{
              return(persistentData==null?0:System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(persistentData)*397)^exists.GetHashCode();
             }
            }
            public static bool operator==(SimObjectSaveValue a,SimObjectSaveValue b)=>a.Equals(b);
            public static bool operator!=(SimObjectSaveValue a,SimObjectSaveValue b)=>!a.Equals(b);
        }
        internal void OnSimObjectChanged(SimObject simObject,bool exists){
         var key=new SimObjectSaveKey(
          simObject.simObjectType,
          simObject.variant,
          simObject.id
         );
         SimObjectPersistentData snapshot;
         bool hasValidPendingSave=pendingSave.TryGetValue(key,out var existingData);
         if(hasValidPendingSave){
          snapshot=existingData.persistentData;
         }else{
          snapshot=simObject.RentPersistentData();
         }
         var persistentData=simObject.persistentData;
         persistentData.Update(simObject,snapshot);
         var value=new SimObjectSaveValue(
          snapshot,
          exists
         );
         if(hasValidPendingSave){
          if(value!=existingData){
           hasValidPendingSave=false;
           pendingSave.Remove(key);
          }
         }
         if(!hasValidPendingSave)
          pendingSave.Add(key,value);
         if(!exists){
          PoolPersistentData(simObject.persistentData);
          simObject.persistentData=null;
         }
        }
        protected static void PoolPersistentData(SimObjectPersistentData persistentData){
         ReturnPersistentData(persistentData);
        }
        protected static void ReturnPersistentData(SimObjectPersistentData data){
         SimObjectPersistentData.Return(data.GetType(),data);
        }
     internal HashSet<Vector2Int>pendingLoad=new();
        internal void OnChunkExists(WorldChunk chunk){
         var cCoord=chunk.cCoord;
         pendingLoad.Add(cCoord);
        }
     private float processPendingInterval=5f;
     private float processPendingTimer;
        internal void OnManualUpdate(){
         if(processPendingInterval>0f){
          if(processPendingTimer<processPendingInterval){
           processPendingTimer+=Time.deltaTime;
          }
          if(processPendingTimer>=processPendingInterval){
           if(TryProcessPending()){
            processPendingTimer=0f;
           }
          }
         }
        }
     public static bool processing{get;protected set;}
     internal Dictionary<SimObjectSaveKey,SimObjectSaveValue>processingSave=new();
     internal HashSet<Vector2Int>processingLoad=new();
        internal bool TryProcessPending(){
         if(processing){
          return false;
         }
         bool pending=
          pendingSave.Count>0||
          pendingLoad.Count>0;
         if(pending){
          processing=true;
          Dictionary<SimObjectSaveKey,SimObjectSaveValue>scheduleSaveForProcessing=null;
          HashSet<Vector2Int>scheduleLoadForProcessing=null;
          if(pendingSave.Count>0){
           Logs.Debug(()=>"'process pending save data, count':"+pendingSave.Count);
           scheduleSaveForProcessing=pendingSave;
           pendingSave=processingSave;
           processingSave=scheduleSaveForProcessing;
          }
          if(pendingLoad.Count>0){
           Logs.Debug(()=>"'process pending load data, count':"+pendingLoad.Count);
           scheduleLoadForProcessing=pendingLoad;
           pendingLoad=processingLoad;
           processingLoad=scheduleLoadForProcessing;
          }
          processSaveFilesJob=ProcessSaveFilesJob.pool.Rent();
          processSaveFilesJob.processingSave=processingSave;
          processSaveFilesJob.processingLoad=processingLoad;
          bool scheduled=ThreadDispatcher.TrySchedule(processSaveFilesJob);
          Logs.Debug(()=>"scheduled:"+scheduled);
          if(!scheduled){
           Logs.Warning("'pending save data could not be processed, count':"+processingSave.Count);
           if(scheduleSaveForProcessing!=null){
            processingSave=pendingSave;
            pendingSave=scheduleSaveForProcessing;
           }
           Logs.Warning("'pending load data could not be processed, count':"+processingLoad.Count);
           if(scheduleLoadForProcessing!=null){
            processingLoad=pendingLoad;
            pendingLoad=scheduleLoadForProcessing;
           }
           ProcessSaveFilesJob.pool.Return(processSaveFilesJob);
           processSaveFilesJob=null;
           processing=false;
          }
         }
         return true;
        }
     internal ProcessSaveFilesJob processSaveFilesJob=null;
        internal class ProcessSaveFilesJob:MultithreadedContainerJob{
         internal static readonly Utilities.ObjectPool<ProcessSaveFilesJob>pool=
          Pool.GetPool<ProcessSaveFilesJob>(
           "",
           ()=>new(),
           (ProcessSaveFilesJob item)=>{item.OnReturnToPoolRecycle();}
          );
     internal Dictionary<SimObjectSaveKey,SimObjectSaveValue>processingSave;
     internal HashSet<Vector2Int>processingLoad;
            protected virtual void OnReturnToPoolRecycle(){
             processingSave=null;
            }
            public void CancelGraciously(){
            }
            public void OnDoScheduleSetContainerData(){
            }
            public void ExecuteAtBackgroundThread(){
             Logs.Debug(()=>"ProcessSaveFilesJob.ExecuteAtBackgroundThread");
             OnSave();
             OnLoad();
            }
            internal void OnSave(){
             Logs.Debug(()=>"OnSave:'begin'");
             foreach(var saveKeyPersistentDataPair in processingSave){
              var key=saveKeyPersistentDataPair.Key;
              var value=saveKeyPersistentDataPair.Value;
              var persistentData=value.persistentData;
              var type=key.simObjectType;
              var variant=key.variant;
              var id=key.id;
              var pos=persistentData.pos;
              var cCoord=vecPosTocCoord(pos);
              var cnkRgn=cCoordTocnkRgn(cCoord);
              Logs.Debug(()=>"OnSave:type:"+type+";variant:"+variant+";id:"+id+";pos:"+pos+";cCoord:"+cCoord+";cnkRgn:"+cnkRgn);
              (PersistentDataManager.singleton.GetFileManager(typeof(SpatialMapFiles))as SpatialMapFiles)?.WriteToSpatialMapFile(cnkRgn,type,variant,id);
               (PersistentDataManager.singleton.GetFileManager(typeof(SimObjectFiles))as SimObjectFiles)?.WriteToSimObjectFile(cnkRgn,type,variant,id,persistentData);
              PoolPersistentData(persistentData);
             }
             processingSave.Clear();
            }
         readonly List<SpatialMapKey>keys=new();
         protected SpawnList spawnList;
            internal void OnLoad(){
             if(processingLoad.Count>0){
              spawnList=SpawnList.pool.Rent();
              var simObjectFiles=(PersistentDataManager.singleton.GetFileManager(typeof(SimObjectFiles))as SimObjectFiles);
              foreach(var cCoord in processingLoad){
               var cnkRgn=cCoordTocnkRgn(cCoord);
               (PersistentDataManager.singleton.GetFileManager(typeof(SpatialMapFiles))as SpatialMapFiles)?.ReadFromSpatialMapFile(cnkRgn,keys);
               Logs.Debug(()=>"OnLoad:cnkRgn:"+cnkRgn+";keys.Count:"+keys.Count);
               foreach(var key in keys){
                var type=key.type;
                var variant=key.variant;
                var id=key.id;
                Logs.Debug(()=>"OnLoad:type:"+type+";variant:"+variant+";id:"+id+";cCoord:"+cCoord+";cnkRgn:"+cnkRgn);
                var persistentData=SimObjectPersistentData.RentFor(type);
                bool read=simObjectFiles.ReadFromSimObjectFile(cnkRgn,type,variant,id,persistentData);
                Logs.Debug(()=>"OnLoad:id:"+id+";read:"+read+";persistentData:"+persistentData);
                if(!read){
                 PoolPersistentData(persistentData);
                }else{
                 spawnList.Add(new(){simObjectType=type,variant=variant,id=id,},persistentData);
                }
               }
               keys.Clear();
              }
              processingLoad.Clear();
             }
            }
            public void OnCompletedDoAtMainThread(){
             var singleton=SimObjectManager.singleton;
             if(!object.ReferenceEquals(singleton,null)){
              if(spawnList!=null){
               singleton.spawnQueue.Enqueue(spawnList);
              }
             }else{
              if(spawnList!=null){
               SpawnList.pool.Return(spawnList);
              }
             }
             spawnList=null;
             ProcessSaveFilesJob.pool.Return(this);
             processing=false;
            }
        }
    }
}