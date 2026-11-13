using AKCondinoO.Bootstrap;
using AKCondinoO.PersistentData;
using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
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
     private float processPendingInterval=5f;
     private float processPendingTimer;
        internal void OnManualUpdate(){
         if(processPendingInterval>0f){
          if(processPendingTimer<processPendingInterval){
           processPendingTimer+=Time.unscaledDeltaTime;
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
        internal bool TryProcessPending(){
         if(processing){
          return false;
         }
         if(pendingSave.Count>0){
          Logs.Debug(()=>"'process pending save data, count':"+pendingSave.Count);
          processing=true;
          var scheduleForProcessing=pendingSave;
          pendingSave=processingSave;
          processingSave=scheduleForProcessing;
          processSaveFilesJob=ProcessSaveFilesJob.pool.Rent();
          processSaveFilesJob.processingSave=processingSave;
          bool scheduled=ThreadDispatcher.TrySchedule(processSaveFilesJob);
          Logs.Debug(()=>"scheduled:"+scheduled);
          if(!scheduled){
           Logs.Warning("'pending save data could not be processed, count':"+processingSave.Count);
           processingSave=pendingSave;
           pendingSave=scheduleForProcessing;
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
            public void OnCompletedDoAtMainThread(){
             ProcessSaveFilesJob.pool.Return(this);
             processing=false;
            }
        }
    }
}