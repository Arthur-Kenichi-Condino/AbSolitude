using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal class SharedCoroutines:MonoSingleton<SharedCoroutines>{
     public override int initOrder{get{return 0;}}
        internal interface SharedCoroutineContainerJob{
            SharedCoroutineContainerJob dependency{get;set;}
            void OnScheduleSetContainerData();
            bool OnLoopExecuteStep(bool flush=false);
            void OnLoopCompleted();
        }
     private static volatile bool running;
     [SerializeField]private int workerCount=8;
     private readonly List<Coroutine>coroutines=new();
     static readonly Queue<SharedCoroutineContainerJob>readyJobs=new();
     static readonly List<SharedCoroutineContainerJob>blockedJobs=new();
     static readonly HashSet<SharedCoroutineContainerJob>runningJobs=new();
        public override void Initialize(){
         base.Initialize();
         int cpu=Environment.ProcessorCount;
         int workerCount=this.workerCount<=0?Math.Max(1,cpu-2):this.workerCount;
         if(this!=null){
          running=true;
          for(int i=0;i<workerCount;i++){
           coroutines.Add(StartCoroutine(Worker()));
          }
         }
        }
        public override void PreShutdown(){
         running=false;
         if(this!=null){
          for(int i=0;i<coroutines.Count;i++){
           StopCoroutine(coroutines[i]);
          }
         }
         coroutines.Clear();
         foreach(var job in runningJobs){
          while(job.OnLoopExecuteStep(true)){}
          job.OnLoopCompleted();
         }
         runningJobs.Clear();
         while(readyJobs.Count>0||blockedJobs.Count>0){
          while(readyJobs.Count>0){
           var job=readyJobs.Dequeue();
           while(job.OnLoopExecuteStep(true)){}
           job.OnLoopCompleted();
           PromoteBlockedJobs(job);
          }
          bool promoted=false;
          for(int i=blockedJobs.Count-1;i>=0;i--){
           var job=blockedJobs[i];
           if(job.dependency==null||!DependencyStillAlive(job.dependency)){
            blockedJobs.RemoveAt(i);
            readyJobs.Enqueue(job);
            promoted=true;
           }
          }
          if(!promoted)break;
         }
         blockedJobs.Clear();
         base.PreShutdown();
        }
        public override void Shutdown(){
         base.Shutdown();
        }
        IEnumerator Worker(){
         while(true){
          while(readyJobs.Count<=0){
           yield return null;
          }
          var job=readyJobs.Dequeue();
          runningJobs.Add(job);
          while(job.OnLoopExecuteStep()){
           yield return null;
          }
          job.OnLoopCompleted();
          runningJobs.Remove(job);
          PromoteBlockedJobs(job);
          job.dependency=null;
         }
        }
        static void PromoteBlockedJobs(SharedCoroutineContainerJob completed){
         for(int i=blockedJobs.Count-1;i>=0;i--){
          var job=blockedJobs[i];
          if(job.dependency==completed||!DependencyStillAlive(job.dependency)){
           job.dependency=null;
           blockedJobs.RemoveAt(i);
           readyJobs.Enqueue(job);
          }
         }
        }
        internal static bool TrySchedule(SharedCoroutineContainerJob job){
         if(!running){return false;}
         job.OnScheduleSetContainerData();
         if(job.dependency==null||!DependencyStillAlive(job.dependency)){
          job.dependency=null;
          readyJobs.Enqueue(job);
         }else{
          blockedJobs.Add(job);
         }
         return true;
        }
        static bool DependencyStillAlive(SharedCoroutineContainerJob dependency){
         if(dependency==null)return false;
         if(runningJobs.Contains(dependency))return true;
         if(readyJobs  .Contains(dependency))return true;
         for(int i=0;i<blockedJobs.Count;i++){
          if(blockedJobs[i]==dependency)
           return true;
         }
         return false;
        }
    }
}