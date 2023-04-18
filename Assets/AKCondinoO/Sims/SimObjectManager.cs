#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SimObjectManager:MonoBehaviour,ISingletonInitialization{
     internal static SimObjectManager singleton{get;set;}
     [SerializeField]internal bool disableSnappingToSlots=false;
     internal PersistentDataSavingBackgroundContainer persistentDataSavingBG;
     internal PersistentDataSavingMultithreaded       persistentDataSavingBGThread;
     internal PersistentDataLoadingBackgroundContainer persistentDataLoadingBG;
     internal PersistentDataLoadingMultithreaded       persistentDataLoadingBGThread;
     internal PersistentSimInventoryDataSavingBackgroundContainer persistentSimInventoryDataSavingBG;
     internal PersistentSimInventoryDataSavingMultithreaded       persistentSimInventoryDataSavingBGThread;
     internal PersistentSimInventoryDataLoadingBackgroundContainer persistentSimInventoryDataLoadingBG;
     internal PersistentSimInventoryDataLoadingMultithreaded       persistentSimInventoryDataLoadingBGThread;
     internal static string simObjectSavePath;
     internal static string simObjectDataSavePath;
     internal static string simActorSavePath;
     internal static string simActorDataSavePath;
     internal static string idsFile;
     internal static string releasedIdsFile;
     internal readonly Dictionary<Type,ulong>ids=new Dictionary<Type,ulong>();
     internal readonly Dictionary<Type,List<ulong>>releasedIds=new Dictionary<Type,List<ulong>>();
     internal readonly Dictionary<Type,LinkedList<SimObject>>pool=new Dictionary<Type,LinkedList<SimObject>>();
     internal readonly Dictionary<(Type simType,ulong number),SimObject>spawned                 =new Dictionary<(Type,ulong),SimObject>();
     internal readonly Dictionary<(Type simType,ulong number),SimObject>active                  =new Dictionary<(Type,ulong),SimObject>();
     internal readonly Dictionary<(Type simType,ulong number),SimObject>despawning              =new Dictionary<(Type,ulong),SimObject>();
     internal readonly Dictionary<(Type simType,ulong number),SimObject>despawningAndReleasingId=new Dictionary<(Type,ulong),SimObject>();
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         PersistentDataSavingMultithreaded.Stop=false;
         persistentDataSavingBG=new PersistentDataSavingBackgroundContainer();
         persistentDataSavingBGThread=new PersistentDataSavingMultithreaded();
         PersistentDataLoadingMultithreaded.Stop=false;
         persistentDataLoadingBG=new PersistentDataLoadingBackgroundContainer();
         persistentDataLoadingBGThread=new PersistentDataLoadingMultithreaded();
          PersistentSimInventoryDataSavingMultithreaded.Stop=false;
          persistentSimInventoryDataSavingBG=new PersistentSimInventoryDataSavingBackgroundContainer();
          persistentSimInventoryDataSavingBGThread=new PersistentSimInventoryDataSavingMultithreaded();
          PersistentSimInventoryDataLoadingMultithreaded.Stop=false;
          persistentSimInventoryDataLoadingBG=new PersistentSimInventoryDataLoadingBackgroundContainer();
          persistentSimInventoryDataLoadingBGThread=new PersistentSimInventoryDataLoadingMultithreaded();
         //  TO DO: dispose, update references' locations shown in errors, change directory for sim inventory values, schedule and complete sim inventory bg tasks following persistentDataSavingBG
        }
        public void Init(){
         if(Core.singleton.isServer){
          simObjectSavePath=string.Format("{0}{1}",Core.savePath,"SimObject/");
          Directory.CreateDirectory(simObjectSavePath);
          simObjectDataSavePath=string.Format("{0}{1}",simObjectSavePath,"Data/");
          Directory.CreateDirectory(simObjectDataSavePath);
          simActorSavePath=string.Format("{0}{1}",Core.savePath,"SimActor/");
          Directory.CreateDirectory(simActorSavePath);
          simActorDataSavePath=string.Format("{0}{1}",simActorSavePath,"Data/");
          Directory.CreateDirectory(simActorDataSavePath);
                  idsFile=string.Format("{0}{1}",simObjectSavePath,        "ids.txt");
          releasedIdsFile=string.Format("{0}{1}",simObjectSavePath,"releasedIds.txt");
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimObjectManager:OnDestroyingCoreEvent");
         #region PersistentDataLoadingMultithreaded
          persistentDataLoadingBG.IsCompleted(persistentDataLoadingBGThread.IsRunning,-1);
           persistentSimInventoryDataLoadingBG.IsCompleted(persistentSimInventoryDataLoadingBGThread.IsRunning,-1);
          if(PersistentDataLoadingMultithreaded.Clear()!=0){
           Log.Error("PersistentDataLoadingMultithreaded will stop with pending work");
          }
          PersistentDataLoadingMultithreaded.Stop=true;
          persistentDataLoadingBGThread.Wait();
           if(PersistentSimInventoryDataLoadingMultithreaded.Clear()!=0){
            Log.Error("PersistentSimInventoryDataLoadingMultithreaded will stop with pending work");
           }
           PersistentSimInventoryDataLoadingMultithreaded.Stop=true;
           persistentSimInventoryDataLoadingBGThread.Wait();
          foreach(var kvp in persistentDataLoadingBGThread.simObjectFileStream){
           Type t=kvp.Key;
           if(Core.singleton.isServer){
            persistentDataLoadingBGThread.simObjectFileStream      [t].Dispose();
            persistentDataLoadingBGThread.simObjectFileStreamReader[t].Dispose();
            if(SimObjectUtil.IsSimActor(t)){
             persistentDataLoadingBGThread.simActorFileStream      [t].Dispose();
             persistentDataLoadingBGThread.simActorFileStreamReader[t].Dispose();
            }
           }
          }
          foreach(var kvp in persistentSimInventoryDataLoadingBGThread.simInventoryFileStream){
           Type t=kvp.Key;
           if(Core.singleton.isServer){
            persistentSimInventoryDataLoadingBGThread.simInventoryFileStream      [t].Dispose();
            persistentSimInventoryDataLoadingBGThread.simInventoryFileStreamReader[t].Dispose();
           }
          }
          persistentDataLoadingBG.Dispose();
           persistentSimInventoryDataLoadingBG.waitingForSimObjectSpawnData.Dispose();
           persistentSimInventoryDataLoadingBG.Dispose();
         #endregion
         #region PersistentDataSavingMultithreaded
          persistentDataSavingBG.IsCompleted(persistentDataSavingBGThread.IsRunning,-1);
           persistentSimInventoryDataSavingBG.IsCompleted(persistentSimInventoryDataSavingBGThread.IsRunning,-1);
          if(Core.singleton.isServer){
           Log.DebugMessage("SimObjectManager exit save");
           SimObjectSpawner.singleton.CollectSavingData(exitSave:true);
           persistentSimInventoryDataSavingBG.waitingForSimInventoryReleasedSimObjectsIdsToRelease=persistentDataSavingBG.waitingForSimInventoryReleasedSimObjectsIdsToRelease;
           PersistentDataSavingMultithreaded.Schedule(persistentDataSavingBG);
            PersistentSimInventoryDataSavingMultithreaded.Schedule(persistentSimInventoryDataSavingBG);
          }
          persistentDataSavingBG.IsCompleted(persistentDataSavingBGThread.IsRunning,-1);
           persistentSimInventoryDataSavingBG.IsCompleted(persistentSimInventoryDataSavingBGThread.IsRunning,-1);
          if(PersistentDataSavingMultithreaded.Clear()!=0){
           Log.Error("PersistentDataSavingMultithreaded will stop with pending work");
          }
          PersistentDataSavingMultithreaded.Stop=true;
          persistentDataSavingBGThread.Wait();
           if(PersistentSimInventoryDataSavingMultithreaded.Clear()!=0){
            Log.Error("PersistentSimInventoryDataSavingMultithreaded will stop with pending work");
           }
           PersistentSimInventoryDataSavingMultithreaded.Stop=true;
           persistentSimInventoryDataSavingBGThread.Wait();
          if(Core.singleton.isServer){
           persistentDataSavingBGThread.releasedIdsFileStreamWriter.Dispose();
           persistentDataSavingBGThread.releasedIdsFileStreamReader.Dispose();
           persistentDataSavingBGThread.idsFileStreamWriter.Dispose();
           persistentDataSavingBGThread.idsFileStreamReader.Dispose();
            persistentSimInventoryDataSavingBGThread.idsFileStreamWriter.Dispose();
            persistentSimInventoryDataSavingBGThread.idsFileStreamReader.Dispose();
            persistentSimInventoryDataSavingBGThread.releasedIdsFileStreamWriter.Dispose();
            persistentSimInventoryDataSavingBGThread.releasedIdsFileStreamReader.Dispose();
          }
          foreach(var kvp in persistentDataSavingBGThread.simObjectFileStream){
           Type t=kvp.Key;
           if(Core.singleton.isServer){
            persistentDataSavingBGThread.simObjectFileStreamWriter[t].Dispose();
            persistentDataSavingBGThread.simObjectFileStreamReader[t].Dispose();
            if(SimObjectUtil.IsSimActor(t)){
             persistentDataSavingBGThread.simActorFileStreamWriter[t].Dispose();
             persistentDataSavingBGThread.simActorFileStreamReader[t].Dispose();
            }
             persistentSimInventoryDataSavingBGThread.simInventoryFileStreamWriter[t].Dispose();
             persistentSimInventoryDataSavingBGThread.simInventoryFileStreamReader[t].Dispose();
           }
          }
          persistentDataSavingBG.waitingForSimInventoryReleasedSimObjectsIdsToRelease.Dispose();
          persistentDataSavingBG.Dispose();
           persistentSimInventoryDataSavingBG.Dispose();
         #endregion
        }
        void OnDestroy(){
        }
        bool terrainMovedFlag;
        internal void OnVoxelTerrainChunkPositionChange(Vector3 oldPos,Vector2Int newRgn){
         terrainMovedFlag=true;
        }
     [SerializeField]bool DEBUG_POOL_ALL_SIM_OBJECTS=false;
     [SerializeField]bool DEBUG_UNPLACE_ALL_SIM_OBJECTS=false;
     internal readonly Queue<SimObject>            deactivateQueue=new Queue<SimObject>();
     internal readonly Queue<SimObject>deactivateAndReleaseIdQueue=new Queue<SimObject>();
        private void Update(){
         if(DEBUG_UNPLACE_ALL_SIM_OBJECTS){
            DEBUG_UNPLACE_ALL_SIM_OBJECTS=false;
          foreach(var a in active){
           var simObject=a.Value;
           simObject.OnUnplaceRequest();
          }
         }else{
          if(DEBUG_POOL_ALL_SIM_OBJECTS){
             DEBUG_POOL_ALL_SIM_OBJECTS=false;
           foreach(var a in active){
            var simObject=a.Value;
            simObject.OnPoolRequest();
           }
          }
         }
         foreach(var a in active){
          var simObject=a.Value;
          simObject.ManualUpdate(terrainMovedFlag);
         }
         while(deactivateQueue.Count>0){var toDeactivate=deactivateQueue.Dequeue();
          OnDeactivate(toDeactivate);
         }
         while(deactivateAndReleaseIdQueue.Count>0){var toDeactivateAndReleaseId=deactivateAndReleaseIdQueue.Dequeue();
          OnDeactivateAndReleaseId(toDeactivateAndReleaseId);
         }
         terrainMovedFlag=false;
        }
        void OnDeactivate(SimObject simObject){
         active .Remove(simObject.id.Value);
         SimObjectSpawner.singleton.despawnQueue.Enqueue(simObject);
        }
        void OnDeactivateAndReleaseId(SimObject simObject){
         active .Remove(simObject.id.Value);
         SimObjectSpawner.singleton.despawnAndReleaseIdQueue.Enqueue(simObject);
        }
        private void LateUpdate(){
         foreach(var a in active){
          var simObject=a.Value;
          simObject.ManualLateUpdate();
         }
        }
    }
}