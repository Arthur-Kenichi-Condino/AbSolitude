using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using AKCondinoO.World.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
namespace AKCondinoO.SimObjects{
    internal class SimObjectManager:MonoSingleton<SimObjectManager>{
     [SerializeField]private SimObjectPrefabs[]prefabsRegistry;
     [SerializeField]private bool      debugMassiveSpawnTest=false;
     [SerializeField]private SimObject debugMassiveSpawnType=null;
     [SerializeField]private int       debugMassiveSpawnCount=5000;
        protected override void Awake(){
         base.Awake();
         if(singleton==this){
         }
        }
        protected override void OnDestroy(){
         Camera.onPreCull-=RenderInstanced;
         if(singleton==this){
         }
         base.OnDestroy();
        }
     internal SimObjectInstancedRendering instancedRendering;
     internal BiomesSimObjectSpawningSystem biomesSpawningSystem;
     internal SimIds simIds;
     private readonly Dictionary<(Type type,string variant),SimObjectFactory<SimObject>>simObjectFactories=new();
     private Coroutine spawnCoroutine;
     private Coroutine simObjectManualUpdateInLotsCoroutine;
        public override void Initialize(){
         base.Initialize();
         instancedRendering=new();
         biomesSpawningSystem=new();
         simIds=new();
         if(this!=null){
          foreach(var prefabsRegistered in prefabsRegistry){
           foreach(var prefab in prefabsRegistered.list){
            var type=prefab.simObject.simObjectType;
            var variant=prefab.simObject.variant;
            simObjectFactories[(type,variant)]=new(prefab.simObject,transform);
            var key=(type,variant);
            simObjects.Add(key,new());
            lazyUpdaterSnapshot.Add(key,new());
           }
          }
          spawnCoroutine=StartCoroutine(SpawnCoroutine());
          simObjectManualUpdateInLotsCoroutine=StartCoroutine(SimObjectManualUpdateInLotsCoroutine());
         }
         Camera.onPreCull+=RenderInstanced;
        }
        public override void PreShutdown(){
         if(this!=null){
          if(spawnCoroutine!=null){
           StopCoroutine(spawnCoroutine);
          }
          if(simObjectManualUpdateInLotsCoroutine!=null){
           StopCoroutine(simObjectManualUpdateInLotsCoroutine);
          }
         }
         base.PreShutdown();
        }
        public override void Shutdown(){
         Camera.onPreCull-=RenderInstanced;
         foreach(var kvp in simObjectFactories){
          var factory=kvp.Value;
          factory.Destroy();
         }
         simObjectFactories.Clear();
         if(spawnQueue.Count>0){
          while(spawnQueue.Count>0){
           SpawnList.pool.Return(spawnQueue.Dequeue());
          }
         }
         instancedRendering.Clear(true);
         base.Shutdown();
        }
     internal readonly Queue<SpawnList>spawnQueue=new();
        IEnumerator SpawnCoroutine(){
         const double maxTimePerFrame=0.001d;//  ...unidade: em segundos
         while(true){
          yield return null;
          if(spawnQueue.Count>0){
           double startTime=Time.realtimeSinceStartupAsDouble;
           while(spawnQueue.Count>0){
            SpawnList spawnList=spawnQueue.Dequeue();
            while(spawnList.SpawnNext(simObjectFactories)){
             if(ShouldYield())yield return null;
            }
            SpawnList.pool.Return(spawnList);
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
     internal readonly Dictionary<(Type type,string variant),Dictionary<ulong,SimObject>>simObjects=new();
        internal void OnSpawn(SimObject simObject){
         var key=(simObject.simObjectType,simObject.variant);
         simObjects[key][simObject.id]=simObject;
        }
        internal void Despawn(SimObject simObject){
         if(simObjectFactories.TryGetValue((simObject.simObjectType,simObject.variant),out var factory)){
          factory.Despawn(simObject);
         }
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         if(debugMassiveSpawnTest&&debugMassiveSpawnType!=null){
          debugMassiveSpawnTest=false;
          Logs.Debug(()=>"'antes de rent':DebugMassiveSpawnJob.pool.bagCount:"+DebugMassiveSpawnJob.pool.bagCount);
          DebugMassiveSpawnJob debugMassiveSpawnJob=DebugMassiveSpawnJob.pool.Rent();
          bool scheduled=ThreadDispatcher.TrySchedule(debugMassiveSpawnJob);
          Logs.Debug(()=>"scheduled:"+scheduled);
          if(!scheduled){
           DebugMassiveSpawnJob.pool.Return(debugMassiveSpawnJob);
          }
          Logs.Debug(()=>"'depois de return':DebugMassiveSpawnJob.pool.bagCount:"+DebugMassiveSpawnJob.pool.bagCount);
         }
        }
     internal readonly Dictionary<(Type type,string variant),List<SimObject>>lazyUpdaterSnapshot=new();
        IEnumerator SimObjectManualUpdateInLotsCoroutine(){
         const double maxTimePerFrame=0.001d;//  ...unidade: em segundos
         while(true){
          double startTime=Time.realtimeSinceStartupAsDouble;
          foreach(var kvp in lazyUpdaterSnapshot){
           var key=kvp.Key;
           var simObjectsList=kvp.Value;
           simObjectsList.AddRange(this.simObjects[key].Values);
           if(ShouldYield())yield return null;
           foreach(var simObject in simObjectsList){
            simObject.LazyUpdate();
            if(ShouldYield())yield return null;
           }
           simObjectsList.Clear();
          }
          yield return null;
          bool ShouldYield(){
           if(Time.realtimeSinceStartupAsDouble-startTime>=maxTimePerFrame){
            startTime=Time.realtimeSinceStartupAsDouble;
            return true;
           }
           return false;
          }
         }
        }
        void RenderInstanced(Camera camera){
         if(camera.cameraType!=CameraType.Game&&camera.cameraType!=CameraType.SceneView){return;}
          #if UNITY_EDITOR
           if(camera.cameraType==CameraType.SceneView&&(UnityEditor.SceneView.lastActiveSceneView==null||camera!=UnityEditor.SceneView.lastActiveSceneView.camera)){
            return;
           }
          #endif
         if(instancedRendering!=null)instancedRendering.DrawAll();
        }
        #region Debug
            internal class DebugMassiveSpawnJob:MultithreadedContainerJob{
             internal static readonly Utilities.ObjectPool<DebugMassiveSpawnJob>pool=
              Pool.GetPool<DebugMassiveSpawnJob>(
               "",
               ()=>new(),
               (DebugMassiveSpawnJob item)=>{}
              );
             private Type   debugMassiveSpawnType;
             private string debugMassiveSpawnVariant;
             private int    debugMassiveSpawnCount;
             private SpawnList spawnList;
                public void CancelGraciously(){
                }
                public void OnDoScheduleSetContainerData(){
                 debugMassiveSpawnType   =singleton.debugMassiveSpawnType.simObjectType;
                 debugMassiveSpawnVariant=singleton.debugMassiveSpawnType.variant;
                 debugMassiveSpawnCount  =singleton.debugMassiveSpawnCount;
                }
                public void ExecuteAtBackgroundThread(){
                 Logs.Debug(()=>"DebugMassiveSpawnJob.BackgroundExecute");
                 spawnList=SpawnList.pool.Rent();
                 for(int i=0;i<debugMassiveSpawnCount;i++){
                  spawnList.Add(
                   new(debugMassiveSpawnType,debugMassiveSpawnVariant)
                  );
                 }
                }
                public void OnCompletedDoAtMainThread(){
                 if(!object.ReferenceEquals(singleton,null)){
                  singleton.spawnQueue.Enqueue(spawnList);
                 }else{
                  SpawnList.pool.Return(spawnList);
                 }
                 spawnList=null;
                 DebugMassiveSpawnJob.pool.Return(this);
                }
            }
        #endregion
    }
    internal class SpawnList{
     internal static readonly Utilities.ObjectPool<SpawnList>pool=
      Pool.GetPool<SpawnList>(
       "",
       ()=>new(),
       (SpawnList item)=>{item.Clear();}
      );
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
        internal bool SpawnNext(Dictionary<(Type type,string variant),SimObjectFactory<SimObject>>simObjectPrefabs){
         if(currentIndex>=data.Count){
          dequeued=true;
          return false;
         }
         SimObjectSpawn item=data[currentIndex];
         currentIndex++;
         if(simObjectPrefabs.TryGetValue((item.simObjectType,item.variant),out var factory)){
          var simObject=factory.Spawn(item);
         }
         return true;
        }
    }
    internal struct SimObjectSpawn{
     internal Type simObjectType;
     internal string variant;
        internal SimObjectSpawn(Type simObjectType,string variant){
         this.simObjectType=simObjectType;
         this.variant=variant;
        }
    }
}