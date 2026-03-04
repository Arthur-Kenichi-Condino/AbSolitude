using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
namespace AKCondinoO.SimObjects{
    internal class SimObjectManager:MonoSingleton<SimObjectManager>{
     public override int initOrder{get{return 15;}}
     [SerializeField]private SimObjectPrefabs prefabsRegistry;
     [SerializeField]private bool      debugMassiveSpawnTest=false;
     [SerializeField]private SimObject debugMassiveSpawnType=null;
     [SerializeField]private int       debugMassiveSpawnCount=50000;
     internal SimObjectInstancedRendering instancedRendering;
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
     private readonly Dictionary<Type,SimObjectFactory<SimObject>>simObjectPrefabs=new();
     private Coroutine spawnCoroutine;
     private Coroutine simObjectManualUpdateInLotsCoroutine;
        public override void Initialize(){
         base.Initialize();
         if(this!=null){
          instancedRendering=new();
          foreach(var prefab in prefabsRegistry.list){
           var type=prefab.simObject.GetType();
           if(prefab.simObject.useInstancedRendering){
            MeshRenderer meshRenderer=prefab.simObject.meshObject.GetComponentInChildren<MeshRenderer>();
            MeshFilter   meshFilter  =prefab.simObject.meshObject.GetComponentInChildren<MeshFilter  >();
            Mesh mesh;
            if(meshRenderer!=null&&meshFilter!=null&&(mesh=meshFilter.sharedMesh)!=null){
             Logs.Message(Logs.LogType.Debug,"mesh.name:"+mesh.name);
             instancedRendering.RegisterType(type,mesh,meshRenderer.sharedMaterials,prefab.simObject.meshObject.layer);
            }
           }
           simObjectPrefabs[type]=new(prefab.simObject);
          }
          spawnCoroutine=StartCoroutine(SpawnCoroutine());
         }
         Camera.onPreCull+=RenderInstanced;
        }
        public override void Shutdown(){
         Camera.onPreCull-=RenderInstanced;
         if(this!=null){
          if(spawnCoroutine!=null){
           StopCoroutine(spawnCoroutine);
          }
         }
         foreach(var kvp in simObjectPrefabs){
          var factory=kvp.Value;
          factory.Destroy(false);
         }
         if(spawnQueue.Count>0){
          while(spawnQueue.Count>0){
           spawnListPool.Return(spawnQueue.Dequeue());
          }
         }
         base.Shutdown();
        }
     static readonly Utilities.ObjectPool<SpawnList>spawnListPool=
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
        IEnumerator SimObjectManualUpdateInLotsCoroutine(){
         while(true){
          yield return null;
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
        void RenderInstanced(Camera camera){
         if(camera.cameraType!=CameraType.Game&&camera.cameraType!=CameraType.SceneView){return;}
          #if UNITY_EDITOR
           if(camera.cameraType==CameraType.SceneView&&(UnityEditor.SceneView.lastActiveSceneView==null||camera!=UnityEditor.SceneView.lastActiveSceneView.camera)){
            return;
           }
          #endif
         if(instancedRendering!=null)instancedRendering.DrawAll();
        }
     static readonly Utilities.ObjectPool<DebugMassiveSpawnJob>debugMassiveSpawnJobPool=
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
        internal bool SpawnNext(Dictionary<Type,SimObjectFactory<SimObject>>simObjectPrefabs){
         if(currentIndex>=data.Count){
          dequeued=true;
          return false;
         }
         SimObjectSpawn item=data[currentIndex];
         currentIndex++;
         Logs.Message(Logs.LogType.Debug,"SpawnNext");
         if(simObjectPrefabs.TryGetValue(item.simObjectType,out var factory)){
          SimObject simObject=factory.Spawn(item);
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