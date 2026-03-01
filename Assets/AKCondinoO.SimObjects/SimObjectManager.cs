using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObjectManager:MonoSingleton<SimObjectManager>{
     public override int initOrder{get{return 15;}}
     [SerializeField]private SimObjectPrefabs prefabsRegistry;
     [SerializeField]private bool      debugMassiveSpawnTest=false;
     [SerializeField]private SimObject debugMassiveSpawnType=null;
     [SerializeField]private int       debugMassiveSpawnCount=50000;
        protected override void Awake(){
         base.Awake();
         if(singleton==this){
         }
        }
        protected override void OnDestroy(){
         if(singleton==this){
         }
         base.OnDestroy();
        }
     private readonly Dictionary<Type,SimObject>simObjectPrefabs=new();
     private Coroutine spawnCoroutine;
        public override void Initialize(){
         base.Initialize();
         if(this!=null){
          foreach(var prefab in prefabsRegistry.list){
           simObjectPrefabs[prefab.GetType()]=prefab;
          }
          spawnCoroutine=StartCoroutine(SpawnCoroutine());
         }
        }
        public override void Shutdown(){
         if(this!=null){
          if(spawnCoroutine!=null){
           StopCoroutine(spawnCoroutine);
          }
         }
         base.Shutdown();
        }
     static readonly ObjectPool<SpawnList>spawnListPool=
      Pool.GetPool<SpawnList>(
       "",
       ()=>new(),
       (SpawnList item)=>{item.Clear();}
      );
     internal readonly Queue<SpawnList>spawnQueue=new();
        IEnumerator SpawnCoroutine(){
         const double maxTimePerFrame=0.001d;//  ...unidade: em segundos
         while(true){
          yield return null;
          if(spawnQueue.Count>0){
           double startTime=Time.realtimeSinceStartupAsDouble;
           while(spawnQueue.Count>0){
            SpawnList spawnList=spawnQueue.Dequeue();
            while(spawnList.SpawnNext(simObjectPrefabs)){
             if(ShouldYield())yield return null;
            }
            spawnListPool.Return(spawnList);
            if(ShouldYield())yield return null;
           }
           bool ShouldYield(){
            if(Time.realtimeSinceStartupAsDouble-startTime>=maxTimePerFrame){
             startTime=Time.realtimeSinceStartupAsDouble;
             return true;
            }
            return false;
           }
          }
         }
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         if(debugMassiveSpawnTest&&debugMassiveSpawnType!=null){
          debugMassiveSpawnTest=false;
          Logs.Message(Logs.LogType.Debug,"'antes de rent':debugMassiveSpawnJobPool.count:"+debugMassiveSpawnJobPool.count);
          DebugMassiveSpawnJob debugMassiveSpawnJob=debugMassiveSpawnJobPool.Rent();
          bool scheduled=ThreadDispatcher.TrySchedule(debugMassiveSpawnJob);
          Logs.Message(Logs.LogType.Debug,"scheduled:"+scheduled);
          if(!scheduled){
           debugMassiveSpawnJobPool.Return(debugMassiveSpawnJob);
          }
          Logs.Message(Logs.LogType.Debug,"'depois de return':debugMassiveSpawnJobPool.count:"+debugMassiveSpawnJobPool.count);
         }
        }
     static readonly ObjectPool<DebugMassiveSpawnJob>debugMassiveSpawnJobPool=
      Pool.GetPool<DebugMassiveSpawnJob>(
       "",
       ()=>new(),
       (DebugMassiveSpawnJob item)=>{}
      );
        internal class DebugMassiveSpawnJob:MultithreadedContainerJob{
         private Type debugMassiveSpawnType;
         private int  debugMassiveSpawnCount;
         private SpawnList spawnList;
            public void SetContainerDataAtMainThread(){
             debugMassiveSpawnType =singleton.debugMassiveSpawnType.GetType();
             debugMassiveSpawnCount=singleton.debugMassiveSpawnCount;
            }
            public void BackgroundExecute(){
             Logs.Message(Logs.LogType.Debug,"DebugMassiveSpawnJob.BackgroundExecute");
             spawnList=spawnListPool.Rent();
             for(int i=0;i<debugMassiveSpawnCount;i++){
              spawnList.Add(
               new(debugMassiveSpawnType)
              );
             }
            }
            public void OnCompletedDoAtMainThread(){
             singleton.spawnQueue.Enqueue(spawnList);
             spawnList=null;
             debugMassiveSpawnJobPool.Return(this);
            }
        }
    }
    internal class SpawnList{
     internal bool dequeued{get;private set;}
     private readonly List<SimObjectSpawn>data;
     private int currentIndex;
        internal SpawnList():this(0){
        }
        internal SpawnList(int capacity){
         data=new(capacity);
         currentIndex=0;
         dequeued=true;
        }
        internal void Add(SimObjectSpawn toSpawn){
         data.Add(toSpawn);
         dequeued=false;
        }
        internal void Clear(){
         data.Clear();
         currentIndex=0;
         dequeued=true;
        }
        internal bool SpawnNext(Dictionary<Type,SimObject>simObjectPrefabs){
         if(currentIndex>=data.Count){
          dequeued=true;
          return false;
         }
         SimObjectSpawn item=data[currentIndex];
         currentIndex++;
         Logs.Message(Logs.LogType.Debug,"SpawnNext");
         if(simObjectPrefabs.TryGetValue(item.simObjectType,out SimObject prefab)){
          GameObject gameObject;
           SimObject  simObject;
           simObject=GameObject.Instantiate(prefab);
          gameObject=simObject.gameObject;
         }
         return true;
        }
    }
    internal struct SimObjectSpawn{
     internal Type simObjectType;
        internal SimObjectSpawn(Type simObjectType){
         this.simObjectType=simObjectType;
        }
    }
}