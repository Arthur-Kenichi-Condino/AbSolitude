#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
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
     internal static string simObjectSavePath;
     internal static string simObjectDataSavePath;
     internal static string simActorSavePath;
     internal static string simActorDataSavePath;
     internal static string simStatsSavePath;
     internal static string simStatsDataSavePath;
     internal static string idsFile;
     internal static string releasedIdsFile;
     internal readonly Dictionary<Type,ulong>ids=new Dictionary<Type,ulong>();
     internal readonly Dictionary<Type,HashSet<ulong>>releasedIds=new Dictionary<Type,HashSet<ulong>>();
     internal readonly Dictionary<Type,LinkedList<SimObject>>pool=new Dictionary<Type,LinkedList<SimObject>>();
     internal readonly Dictionary<(Type simObjectType,ulong idNumber),SimObject>spawned                 =new Dictionary<(Type,ulong),SimObject>();
     internal readonly Dictionary<(Type simObjectType,ulong idNumber),SimObject>active                  =new Dictionary<(Type,ulong),SimObject>();
     internal readonly Dictionary<(Type  simActorType,ulong idNumber),SimActor >activeActor             =new Dictionary<(Type,ulong),SimActor >();
     internal readonly Dictionary<(Type simObjectType,ulong idNumber),SimObject>despawning              =new Dictionary<(Type,ulong),SimObject>();
     internal readonly Dictionary<(Type simObjectType,ulong idNumber),SimObject>despawningAndReleasingId=new Dictionary<(Type,ulong),SimObject>();
      internal readonly HashSet<SimObject>netActive=new HashSet<SimObject>();
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         SimConstruction.constructionLayer=LayerMask.GetMask("Construction");
         System.Random seedGenerator=new System.Random();
         SimObject.Stats.seedGenerator=new System.Random(seedGenerator.Next());
         BaseAI         .seedGenerator=new System.Random(seedGenerator.Next());
         PersistentDataSavingMultithreaded.Stopped=false;
         persistentDataSavingBG=new PersistentDataSavingBackgroundContainer();
         persistentDataSavingBGThread=new PersistentDataSavingMultithreaded();
         PersistentDataLoadingMultithreaded.Stopped=false;
         persistentDataLoadingBG=new PersistentDataLoadingBackgroundContainer();
         persistentDataLoadingBGThread=new PersistentDataLoadingMultithreaded();
          #region SimInventoryManager
              PersistentSimInventoryDataSavingMultithreaded.Stopped=false;
              SimInventoryManager.singleton.persistentSimInventoryDataSavingBG=new PersistentSimInventoryDataSavingBackgroundContainer();
              SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread=new PersistentSimInventoryDataSavingMultithreaded();
              PersistentSimInventoryDataLoadingMultithreaded.Stopped=false;
              SimInventoryManager.singleton.persistentSimInventoryDataLoadingBG=new PersistentSimInventoryDataLoadingBackgroundContainer();
              SimInventoryManager.singleton.persistentSimInventoryDataLoadingBGThread=new PersistentSimInventoryDataLoadingMultithreaded();
          #endregion
         //  TO DO: dispose, update references' locations shown in errors, change directory for sim inventory values, schedule and complete sim inventory bg tasks following persistentDataSavingBG
        }
        public void Init(){
         if(Core.singleton.isServer){
          simObjectSavePath=string.Format("{0}{1}",Core.savePath,"SimObject/");
          Directory.CreateDirectory(simObjectSavePath);
          simObjectDataSavePath=string.Format("{0}{1}",simObjectSavePath,"Data/");
          Directory.CreateDirectory(simObjectDataSavePath);
                  idsFile=string.Format("{0}{1}",simObjectSavePath,        "ids.txt");
          releasedIdsFile=string.Format("{0}{1}",simObjectSavePath,"releasedIds.txt");
          simActorSavePath=string.Format("{0}{1}",Core.savePath,"SimActor/");
          Directory.CreateDirectory(simActorSavePath);
          simActorDataSavePath=string.Format("{0}{1}",simActorSavePath,"Data/");
          Directory.CreateDirectory(simActorDataSavePath);
          simStatsSavePath=string.Format("{0}{1}",Core.savePath,"SimStats/");
          Directory.CreateDirectory(simStatsSavePath);
          simStatsDataSavePath=string.Format("{0}{1}",simStatsSavePath,"Data/");
          Directory.CreateDirectory(simStatsDataSavePath);
         }
         BaseAI.animatorClipNameToActorMotion.Clear();
         BaseAI.animatorClipNameToActorWeaponLayerMotion.Clear();
        }
     readonly List<(Type buffType,SkillBuff skillBuff)>buffsToPool=new List<(Type,SkillBuff)>();
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         //Log.DebugMessage("SimObjectManager:OnDestroyingCoreEvent");
         #region PersistentDataLoadingMultithreaded
          persistentDataLoadingBG.IsCompleted(persistentDataLoadingBGThread.IsRunning,-1);
           #region SimInventoryManager
               SimInventoryManager.singleton.persistentSimInventoryDataLoadingBG.IsCompleted(SimInventoryManager.singleton.persistentSimInventoryDataLoadingBGThread.IsRunning,-1);
           #endregion
          if(PersistentDataLoadingMultithreaded.Clear()!=0){
           Log.Error("PersistentDataLoadingMultithreaded will stop with pending work");
          }
          PersistentDataLoadingMultithreaded.Stopped=true;
          persistentDataLoadingBGThread.Wait();
           #region SimInventoryManager
               if(PersistentSimInventoryDataLoadingMultithreaded.Clear()!=0){
                Log.Error("PersistentSimInventoryDataLoadingMultithreaded will stop with pending work");
               }
               PersistentSimInventoryDataLoadingMultithreaded.Stopped=true;
               SimInventoryManager.singleton.persistentSimInventoryDataLoadingBGThread.Wait();
           #endregion
          foreach(var kvp in persistentDataLoadingBGThread.simObjectFileStream){
           Type simObjectType=kvp.Key;
           if(Core.singleton.isServer){
            persistentDataLoadingBGThread.simObjectFileStream      [simObjectType].Dispose();
            persistentDataLoadingBGThread.simObjectFileStreamReader[simObjectType].Dispose();
            if(SimObjectUtil.IsSimActor(simObjectType)){
             persistentDataLoadingBGThread.simActorFileStream      [simObjectType].Dispose();
             persistentDataLoadingBGThread.simActorFileStreamReader[simObjectType].Dispose();
            }
           }
          }
           #region SimInventoryManager
               foreach(var kvp in SimInventoryManager.singleton.persistentSimInventoryDataLoadingBGThread.simInventoryFileStream){
                Type simObjectType=kvp.Key;
                if(Core.singleton.isServer){
                 SimInventoryManager.singleton.persistentSimInventoryDataLoadingBGThread.simInventoryFileStream      [simObjectType].Dispose();
                 SimInventoryManager.singleton.persistentSimInventoryDataLoadingBGThread.simInventoryFileStreamReader[simObjectType].Dispose();
                }
               }
           #endregion
          persistentDataLoadingBG.Dispose();
           #region SimInventoryManager
               SimInventoryManager.singleton.persistentSimInventoryDataLoadingBG.waitingForSimObjectSpawnData.Dispose();
               SimInventoryManager.singleton.persistentSimInventoryDataLoadingBG.Dispose();
           #endregion
         #endregion
         #region PersistentDataSavingMultithreaded
          if(Core.singleton.isServer){
           Log.DebugMessage("SimObjectManager exit save");
           _Loop:{}
           persistentDataSavingBG.IsCompleted(persistentDataSavingBGThread.IsRunning,-1);
            #region SimInventoryManager
                SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.IsCompleted(SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.IsRunning,-1);
            #endregion
           SimObjectSpawner.singleton.CollectSavingData(exitSave:true);
           //
           foreach(var kvp in SkillBuff.allActiveBuffs){
            foreach(SkillBuff skillBuff in kvp.Value){
             buffsToPool.Add((kvp.Key,skillBuff));
            }
           }
           foreach((Type buffType,SkillBuff skillBuff)skillBuffToPool in buffsToPool){
            SkillBuff.Pool(skillBuffToPool.skillBuff,true);
           }
           buffsToPool.Clear();
           SchedulePersistentDataSaving();
           if(active.Count>0){
            bool loop=false;
            foreach(var a in active){
             loop=loop|a.Value.OnExitSaveRecursion();
            }
            active.Clear();
            //Log.DebugMessage("SimObjectManager exit save: is loop needed:"+loop);
            if(loop){
             goto _Loop;
            }
           }
          }
          persistentDataSavingBG.IsCompleted(persistentDataSavingBGThread.IsRunning,-1);
           #region SimInventoryManager
               SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.IsCompleted(SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.IsRunning,-1);
           #endregion
          if(PersistentDataSavingMultithreaded.Clear()!=0){
           Log.Error("PersistentDataSavingMultithreaded will stop with pending work");
          }
          PersistentDataSavingMultithreaded.Stopped=true;
          persistentDataSavingBGThread.Wait();
           #region SimInventoryManager
               if(PersistentSimInventoryDataSavingMultithreaded.Clear()!=0){
                Log.Error("PersistentSimInventoryDataSavingMultithreaded will stop with pending work");
               }
               PersistentSimInventoryDataSavingMultithreaded.Stopped=true;
               SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.Wait();
           #endregion
          if(Core.singleton.isServer){
           persistentDataSavingBGThread.releasedIdsFileStreamWriter.Dispose();
           persistentDataSavingBGThread.releasedIdsFileStreamReader.Dispose();
           persistentDataSavingBGThread.idsFileStreamWriter.Dispose();
           persistentDataSavingBGThread.idsFileStreamReader.Dispose();
            #region SimInventoryManager
                SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.idsFileStreamWriter.Dispose();
                SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.idsFileStreamReader.Dispose();
                SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.releasedIdsFileStreamWriter.Dispose();
                SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.releasedIdsFileStreamReader.Dispose();
            #endregion
          }
          foreach(var kvp in persistentDataSavingBGThread.simObjectFileStream){
           Type simObjectType=kvp.Key;
           if(Core.singleton.isServer){
            persistentDataSavingBGThread.simObjectFileStreamWriter[simObjectType].Dispose();
            persistentDataSavingBGThread.simObjectFileStreamReader[simObjectType].Dispose();
            if(SimObjectUtil.IsSimActor(simObjectType)){
             persistentDataSavingBGThread.simActorFileStreamWriter[simObjectType].Dispose();
             persistentDataSavingBGThread.simActorFileStreamReader[simObjectType].Dispose();
            }
             #region SimInventoryManager
                 SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.simInventoryFileStreamWriter[simObjectType].Dispose();
                 SimInventoryManager.singleton.persistentSimInventoryDataSavingBGThread.simInventoryFileStreamReader[simObjectType].Dispose();
             #endregion
           }
          }
          persistentDataSavingBG.waitingForSimInventoryReleasedSimObjectsIdsToRelease.Dispose();
          persistentDataSavingBG.Dispose();
           #region SimInventoryManager
               SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.Dispose();
           #endregion
         #endregion
        }
        void OnDestroy(){
        }
        internal void SchedulePersistentDataSaving(){
          #region SimInventoryManager
              SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.waitingForSimInventoryReleasedSimObjectsIdsToRelease=persistentDataSavingBG.waitingForSimInventoryReleasedSimObjectsIdsToRelease;
          #endregion
         persistentDataSavingBG.simInventoryReleasedSimObjectsIdsToRelease=SimInventoryManager.singleton.persistentSimInventoryDataSavingBG.simInventoryReleasedSimObjectsIdsToRelease;
         PersistentDataSavingMultithreaded.Schedule(persistentDataSavingBG);
          #region SimInventoryManager
              PersistentSimInventoryDataSavingMultithreaded.Schedule(SimInventoryManager.singleton.persistentSimInventoryDataSavingBG);
          #endregion
        }
        internal void SchedulePersistentDataLoading(){
         persistentDataLoadingBG.waitingForSimObjectSpawnData=SimInventoryManager.singleton.persistentSimInventoryDataLoadingBG.waitingForSimObjectSpawnData;
          #region SimInventoryManager
              SimInventoryManager.singleton.persistentSimInventoryDataLoadingBG.spawnDataFromFiles=persistentDataLoadingBG.spawnDataFromFiles;
          #endregion
         PersistentDataLoadingMultithreaded.Schedule(persistentDataLoadingBG);
          #region SimInventoryManager
              PersistentSimInventoryDataLoadingMultithreaded.Schedule(SimInventoryManager.singleton.persistentSimInventoryDataLoadingBG);
          #endregion
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
         //Log.DebugMessage("active.Count:"+active.Count);
         foreach(var a in active){
          var simObject=a.Value;
          if(simObject==null||simObject.gameObject==null){continue;}
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
         active     .Remove(simObject.id.Value);
         activeActor.Remove(simObject.id.Value);
         SimObjectSpawner.singleton.despawnQueue.Enqueue(simObject);
         simObject.OnDeactivated();
        }
        void OnDeactivateAndReleaseId(SimObject simObject){
         active     .Remove(simObject.id.Value);
         activeActor.Remove(simObject.id.Value);
         SimObjectSpawner.singleton.despawnAndReleaseIdQueue.Enqueue(simObject);
         simObject.OnDeactivated();
        }
        private void LateUpdate(){
         foreach(var a in active){
          var simObject=a.Value;
          if(simObject==null||simObject.gameObject==null){continue;}
          simObject.ManualLateUpdate();
         }
         foreach(var nA in netActive){
          nA.ManualLateUpdate();
         }
        }
    }
}