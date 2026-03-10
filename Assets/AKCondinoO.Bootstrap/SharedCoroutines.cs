using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal class SharedCoroutines:MonoSingleton<SharedCoroutines>{
     public override int initOrder{get{return 0;}}
        internal interface SharedCoroutineContainerJob{
            void OnScheduleSetContainerData();
            bool OnLoopExecuteStep(bool flush=false);
            void OnLoopCompleted();
        }
     private static volatile bool running;
     [SerializeField]private int workerCount=8;
     private readonly List<Coroutine>coroutines=new();
     static readonly Queue<SharedCoroutineContainerJob>scheduledJobs=new();
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
         while(scheduledJobs.Count>0){
          var job=scheduledJobs.Dequeue();
          while(job.OnLoopExecuteStep(true)){}
          job.OnLoopCompleted();
         }
         base.PreShutdown();
        }
        public override void Shutdown(){
         base.Shutdown();
        }
        IEnumerator Worker(){
         while(true){
          yield return null;
          while(scheduledJobs.Count<=0){
           yield return null;
          }
          var job=scheduledJobs.Dequeue();
          runningJobs.Add(job);
          while(job.OnLoopExecuteStep()){
           yield return null;
          }
          job.OnLoopCompleted();
          runningJobs.Remove(job);
         }
        }
        internal static bool TrySchedule(SharedCoroutineContainerJob job){
         if(!running){return false;}
         job.OnScheduleSetContainerData();
         scheduledJobs.Enqueue(job);
         return true;
        }
    }
}