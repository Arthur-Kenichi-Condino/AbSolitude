using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObjectManager:Singleton<SimObjectManager>{
     [SerializeField]private bool debugSpawnTest=false;
     [SerializeField]private int debugSpawnCount=10000;
     public override int initOrder{get{return 2;}}
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
     static readonly ObjectPool<SpawnList>spawnListPool=
      Pool.GetPool<SpawnList>(
       "",
       ()=>new(),
       (SpawnList item)=>{item.Clear();}
      );
        public override void Initialize(){
         base.Initialize();
         if(this!=null){
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
     private Coroutine spawnCoroutine;
     internal readonly Queue<SpawnList>spawnQueue=new();
        IEnumerator SpawnCoroutine(){
         const double maxTimePerFrame=0.001d;//  ...unidade: em segundos
         while(true){
          yield return null;
          if(spawnQueue.Count>0){
           double startTime=Time.realtimeSinceStartupAsDouble;
           while(spawnQueue.Count>0){
            SpawnList spawnList=spawnQueue.Dequeue();
            while(spawnList.SpawnNext()){
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
        internal class DebugSpawnJob:MultithreadedContainerJob{
            public void BackgroundExecute(){
            }
            public void OnCompletedDoAtMainThread(){
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
        internal bool SpawnNext(){
         if(currentIndex>=data.Count){
          dequeued=true;
          return false;
         }
         SimObjectSpawn item=data[currentIndex];
         currentIndex++;
         //  TO DO: spawn here
         return true;
        }
    }
    internal struct SimObjectSpawn{
    }
}