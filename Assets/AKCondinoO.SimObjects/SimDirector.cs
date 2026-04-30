using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors.SimInteractions;
using AKCondinoO.SimObjects;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.SimObjects.SimObjectManager;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.SimActors{
    internal class SimDirector:MonoSingleton<SimDirector>{
     [SerializeField]private InteractablesRegistry[]interactablesRegistry;
        public override void Initialize(){
         base.Initialize();
         foreach(var r in interactablesRegistry){
          foreach(var interactableInteractions in r.interactables){
           interactableInteractions.Register();
          }
         }
        }
     private readonly HashSet<(Type type,string variant)>simActorTypes=new();
     private readonly HashSet<(Type type,string variant)>playableTypes=new();
        internal void OnSimActorFactoryCreated((Type type,string variant)key,SimObjectPrefabData prefab){
         simActorTypes.Add(key);
         sims.Add(key,new());
         var actorPrefab=(SimActor)prefab.simObject;
         if(actorPrefab.isPlayable){
          playableTypes.Add(key);
          selectableSims.Add(key,new());
         }
        }
        public override void PreShutdown(){
         base.PreShutdown();
        }
        public override void Shutdown(){
         InteractionDefinitions.UnregisterAll();
         base.Shutdown();
        }
     internal readonly Dictionary<(Type type,string variant),Dictionary<ulong,SimActor>>sims=new();
     internal readonly Dictionary<(Type type,string variant),Dictionary<ulong,SimActor>>selectableSims=new();
     internal readonly Dictionary<(Type type,string variant),int>selectableSimsCount=new();
        internal void OnSimSpawn(SimActor sim){
         var key=(sim.simObjectType,sim.variant);
         sims[key][sim.id]=sim;
         if(sim.isPlayable){
          selectableSims[key][sim.id]=sim;
          if(!selectableSimsCount.TryGetValue(key,out int count)){
           selectableSimsCount.Add(key,count=0);
          }
          selectableSimsCount[key]=count+1;
         }
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         if(ActiveZone.main!=null){
          EnsureActiveSimExists();
         }
         FlushCriticalSimSpawnRequests();
         FlushSpawnRequests();
        }
        void EnsureActiveSimExists(){
         if(isSpawningCriticalSims){
          return;
         }
         if(selectableSimsCount.Count<=0){
          Logs.Debug(()=>"no selectable sims found!");
          foreach(var playable in playableTypes){
           SpawnRequest request=new(){
            type    =playable.type,
            variant =playable.variant,
            count   =1,
            position=new(0f,Height/2f,0f),
           };
           criticalSimRequests.Add(request);
           break;
          }
         }
        }
     internal bool isSpawningCriticalSims{
      get{
       if(doingCriticalSimSpawnJob){
        return true;
       }
       if(criticalSpawnList!=null){
        return!criticalSpawnList.dequeued;
       }
       return false;
      }
     }
     private bool doingCriticalSimSpawnJob;
     internal SpawnList criticalSpawnList;
     internal readonly List<SpawnRequest>criticalSimRequests=new();
        internal void FlushCriticalSimSpawnRequests(){
         if(criticalSpawnList?.dequeued==true){
          criticalSpawnList=null;
         }
         if(!isSpawningCriticalSims){
          if(criticalSimRequests.Count>0){
           CriticalSimSpawnJob spawnJob=(CriticalSimSpawnJob)SpawnJob.Rent(typeof(CriticalSimSpawnJob));
           bool scheduled=ThreadDispatcher.TrySchedule(spawnJob);
           Logs.Debug(()=>"scheduled:"+scheduled);
           if(!scheduled){
            SpawnJob.Return(spawnJob.GetType(),spawnJob);
           }else{
            doingCriticalSimSpawnJob=true;
            criticalSimRequests.Clear();
           }
          }
         }
        }
        internal class CriticalSimSpawnJob:SimSpawnJob{
            protected override void PrepareSpawnJob(){
             var singleton=SimDirector.singleton;
             requested.AddRange(singleton.criticalSimRequests);
            }
            protected override void EnqueueSpawnList(){
             var singleton=SimDirector.singleton;
             singleton.criticalSpawnList=spawnList;
             base.EnqueueSpawnList();
             singleton.doingCriticalSimSpawnJob=false;
            }
        }
     internal readonly List<SpawnRequest>simRequests=new();
        internal void FlushSpawnRequests(){
         if(simRequests.Count>0){
          SimSpawnJob spawnJob=(SimSpawnJob)SpawnJob.Rent(typeof(SimSpawnJob));
          bool scheduled=ThreadDispatcher.TrySchedule(spawnJob);
          Logs.Debug(()=>"scheduled:"+scheduled);
          if(!scheduled){
           SpawnJob.Return(spawnJob.GetType(),spawnJob);
          }else{
           simRequests.Clear();
          }
         }
        }
        internal class SimSpawnJob:SpawnJob{
            protected override void PrepareSpawnJob(){
             var singleton=SimDirector.singleton;
             requested.AddRange(singleton.simRequests);
            }
            protected override void EnqueueSpawnList(){
             var singleton=SimDirector.singleton;
             base.EnqueueSpawnList();
            }
        }
        internal void GetSelectableSim(out SimActor sim){
         sim=null;
         foreach(var kvp1 in selectableSims){
          foreach(var kvp2 in kvp1.Value){
           sim=kvp2.Value;
           break;
          }
         }
        }
    }
}