using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors;
using AKCondinoO.Utilities;
using AKCondinoO.World.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static AKCondinoO.SimActors.SimDirector;
namespace AKCondinoO.SimObjects{
    internal class SimObjectManager:MonoSingleton<SimObjectManager>{
     [SerializeField]private SimObjectPrefabs[]prefabsRegistry;
     [SerializeField]private bool      debugSpawnTest=false;
     [SerializeField]private SimObject debugSpawnType=null;
     [SerializeField]private int       debugSpawnCount=1;
     [SerializeField]private Vector3   debugSpawnPosition;
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
            if(!prefab.simObject.IsSimActor()){
             simObjects.Add(key,new());
             lazyUpdaterSnapshot.Add(key,new());
            }else{
             sims.Add(key,new());
             SimDirector.singleton.OnSimActorFactoryCreated(key,prefab);
            }
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
     internal readonly Dictionary<(Type type,string variant),Dictionary<ulong,SimActor>>sims=new();
        internal void OnSpawn(SimObject simObject){
         var key=(simObject.simObjectType,simObject.variant);
         if(!simObject.IsSimActor()){
          simObjects[key][simObject.id]=simObject;
         }else{
          var simActor=(SimActor)simObject;
          sims[key][simObject.id]=simActor;
          SimDirector.singleton.OnSimSpawn(simActor);
         }
        }
        internal void Despawn(SimObject simObject){
         if(simObjectFactories.TryGetValue((simObject.simObjectType,simObject.variant),out var factory)){
          factory.Despawn(simObject);
         }
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         foreach(var kvp1 in sims){
          var simsById=kvp1.Value;
          foreach(var kvp2 in simsById){
           var sim=kvp2.Value;
           sim.DynamicUpdate();
          }
         }
         FlushSpawnRequests();
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
     internal readonly List<SpawnRequest>spawnRequests=new();
        internal void FlushSpawnRequests(){
         #region Debug
             if(debugSpawnTest&&debugSpawnType!=null){
              debugSpawnTest=false;
              SpawnRequest spawnRequest=new(){
               type    =debugSpawnType.simObjectType,
               variant =debugSpawnType.variant,
               count   =debugSpawnCount,
               position=debugSpawnPosition,
              };
              spawnRequests.Add(spawnRequest);
             }
         #endregion
         if(spawnRequests.Count>0){
          SpawnJob spawnJob=SpawnJob.Rent(typeof(SpawnJob));
          bool scheduled=ThreadDispatcher.TrySchedule(spawnJob);
          Logs.Debug(()=>"scheduled:"+scheduled);
          if(!scheduled){
           SpawnJob.Return(spawnJob.GetType(),spawnJob);
          }else{
           spawnRequests.Clear();
          }
         }
        }
        internal struct SpawnRequest{
         internal Type type;
         internal string variant;
         internal int count;
         internal Vector3 position;
        }
        internal class SpawnJob:MultithreadedContainerJob{
         static readonly Dictionary<(Type,string),ObjectPoolBase>spawnJobPool=new(){
          {(typeof(SpawnJob           ),""),Pool.GetPool<SpawnJob           >("",()=>new(),(SpawnJob            item)=>{item.Reset();})},
          {(typeof(SimSpawnJob        ),""),Pool.GetPool<SimSpawnJob        >("",()=>new(),(SimSpawnJob         item)=>{item.Reset();})},
          {(typeof(CriticalSimSpawnJob),""),Pool.GetPool<CriticalSimSpawnJob>("",()=>new(),(CriticalSimSpawnJob item)=>{item.Reset();})},
         };
        internal static SpawnJob Rent(Type poolId){
         return(SpawnJob)spawnJobPool[(poolId,"")].ObjectRent();
        }
        internal static void Return(Type poolId,SpawnJob spawnJob){
         spawnJobPool[(poolId,"")].ObjectReturn(spawnJob);
        }
         protected readonly List<SpawnRequest>requested=new();
            protected virtual void Reset(){
             requested.Clear();
            }
         protected SpawnList spawnList;
            public void CancelGraciously(){
            }
            public void OnDoScheduleSetContainerData(){
             PrepareSpawnJob();
            }
            protected virtual void PrepareSpawnJob(){
             var singleton=SimObjectManager.singleton;
             requested.AddRange(singleton.spawnRequests);
            }
            public void ExecuteAtBackgroundThread(){
             Logs.Debug(()=>"SpawnJob.ExecuteAtBackgroundThread");
             spawnList=SpawnList.pool.Rent();
             FillSpawnList();
            }
            protected virtual void FillSpawnList(){
             foreach(var request in requested){
              for(int i=0;i<request.count;i++){
               spawnList.Add(
                new(request.type,request.variant,request.position)
               );
              }
             }
            }
            public void OnCompletedDoAtMainThread(){
             EnqueueSpawnList();
             Return(GetType(),this);
            }
            protected virtual void EnqueueSpawnList(){
             var singleton=SimObjectManager.singleton;
             if(!object.ReferenceEquals(singleton,null)){
              singleton.spawnQueue.Enqueue(spawnList);
             }else{
              SpawnList.pool.Return(spawnList);
             }
             spawnList=null;
            }
        }
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
     internal Vector3 position;
        internal SimObjectSpawn(Type simObjectType,string variant,Vector3 position){
         this.simObjectType=simObjectType;
         this.variant=variant;
         this.position=position;
        }
    }
}