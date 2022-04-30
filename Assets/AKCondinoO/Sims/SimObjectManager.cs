#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SimObjectManager:MonoBehaviour{
     internal static SimObjectManager Singleton;
     internal PersistentDataSavingBackgroundContainer persistentDataSavingBG;
     internal PersistentDataSavingMultithreaded       persistentDataSavingBGThread;
     internal PersistentDataLoadingBackgroundContainer persistentDataLoadingBG;
     internal PersistentDataLoadingMultithreaded       persistentDataLoadingBGThread;
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
         if(Singleton==null){Singleton=this;}else{DestroyImmediate(this);return;}
         PersistentDataSavingMultithreaded.Stop=false;
         persistentDataSavingBG=new PersistentDataSavingBackgroundContainer();
         persistentDataSavingBGThread=new PersistentDataSavingMultithreaded();
         PersistentDataLoadingMultithreaded.Stop=false;
         persistentDataLoadingBG=new PersistentDataLoadingBackgroundContainer();
         persistentDataLoadingBGThread=new PersistentDataLoadingMultithreaded();
        }
        internal void Init(){
         Core.Singleton.OnDestroyingCoreEvent+=OnDestroyingCoreEvent;
                 idsFile=string.Format("{0}{1}",Core.savePath,        "ids.txt");
         releasedIdsFile=string.Format("{0}{1}",Core.savePath,"releasedIds.txt");
        }
        void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimObjectManager:OnDestroyingCoreEvent");
         #region PersistentDataLoadingMultithreaded
          persistentDataLoadingBG.IsCompleted(persistentDataLoadingBGThread.IsRunning,-1);
          if(PersistentDataLoadingMultithreaded.Clear()!=0){
           Log.Error("PersistentDataLoadingMultithreaded will stop with pending work");
          }
          PersistentDataLoadingMultithreaded.Stop=true;
          persistentDataLoadingBGThread.Wait();
         #endregion
         #region PersistentDataSavingMultithreaded
          persistentDataSavingBG.IsCompleted(persistentDataSavingBGThread.IsRunning,-1);
           Log.DebugMessage("SimObjectManager exit save");
           SimObjectSpawner.Singleton.CollectSavingData(exitSave:true);
           PersistentDataSavingMultithreaded.Schedule(persistentDataSavingBG);
          persistentDataSavingBG.IsCompleted(persistentDataSavingBGThread.IsRunning,-1);
          if(PersistentDataSavingMultithreaded.Clear()!=0){
           Log.Error("PersistentDataSavingMultithreaded will stop with pending work");
          }
          PersistentDataSavingMultithreaded.Stop=true;
          persistentDataSavingBGThread.Wait();
          persistentDataSavingBGThread.releasedIdsFileStreamWriter.Dispose();
          persistentDataSavingBGThread.releasedIdsFileStreamReader.Dispose();
          persistentDataSavingBGThread.idsFileStreamWriter.Dispose();
          persistentDataSavingBGThread.idsFileStreamReader.Dispose();
          foreach(var kvp in persistentDataSavingBGThread.fileStream){
           Type t=kvp.Key;
           persistentDataSavingBGThread.fileStreamWriter[t].Dispose();
           persistentDataSavingBGThread.fileStreamReader[t].Dispose();
          }
         #endregion
        }
        void OnDestroy(){
        }
     [SerializeField]bool DEBUG_POOL_ALL_SIM_OBJECTS=false;
     [SerializeField]bool DEBUG_UNPLACE_ALL_SIM_OBJECTS=false;
     internal readonly Queue<SimObject>            DeactivateQueue=new Queue<SimObject>();
     internal readonly Queue<SimObject>DeactivateAndReleaseIdQueue=new Queue<SimObject>();
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
          simObject.ManualUpdate();
         }
         while(DeactivateQueue.Count>0){var toDeactivate=DeactivateQueue.Dequeue();
          OnDeactivate(toDeactivate);
         }
         while(DeactivateAndReleaseIdQueue.Count>0){var toDeactivateAndReleaseId=DeactivateAndReleaseIdQueue.Dequeue();
          OnDeactivateAndReleaseId(toDeactivateAndReleaseId);
         }
        }
        void OnDeactivate(SimObject simObject){
         active .Remove(simObject.id.Value);
         SimObjectSpawner.Singleton.DespawnQueue.Enqueue(simObject);
        }
        void OnDeactivateAndReleaseId(SimObject simObject){
         active .Remove(simObject.id.Value);
         SimObjectSpawner.Singleton.DespawnAndReleaseIdQueue.Enqueue(simObject);
        }
    }
}