using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal class SharedCoroutines:MonoSingleton<SharedCoroutines>{
        internal interface SharedCoroutineContainerJob{
            SharedCoroutineContainerJob dependency{get;set;}
            bool isCancelledCanStop{get;set;}
            void CancelGraciously();
            void OnScheduleSetContainerData();
            int OnLoopExecuteStep(bool flush=false);
            void OnLoopCompleted();
        }
     private static volatile bool running;
     [SerializeField]private int workerCount=8;
     private readonly List<Coroutine>coroutines=new();
     static readonly Queue<SharedCoroutineContainerJob>readyJobs=new();
     static readonly HashSet<SharedCoroutineContainerJob>readyJobsSet=new();
        static void EnqueueReady(SharedCoroutineContainerJob job){
         readyJobs.Enqueue(job);readyJobsSet.Add(job);
        }
        static SharedCoroutineContainerJob DequeueReady(){
         var job=readyJobs.Dequeue();readyJobsSet.Remove(job);
         return job;
        }
     static readonly List<SharedCoroutineContainerJob>blockedJobs=new();
     static readonly HashSet<SharedCoroutineContainerJob>blockedJobsSet=new();
        static void EnqueueBlockedJob(SharedCoroutineContainerJob job){
         blockedJobs.Add(job);blockedJobsSet.Add(job);
        }
        static void RemoveBlockedJobAt(int i,SharedCoroutineContainerJob job){
         blockedJobs.RemoveAt(i);blockedJobsSet.Remove(job);
        }
     static readonly HashSet<SharedCoroutineContainerJob>runningJobs=new();
        public override void Initialize(){
         base.Initialize();
         SharedCoroutineBudget.maxFrameTime=maxFrameTime;
         SharedCoroutineBudget.maxLoopsPerJob=maxLoopsPerJob;
         SharedCoroutineBudget.maxLoopsPerRoutine=maxLoopsPerRoutine;
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
          while(job.OnLoopExecuteStep(true)>=0){}
          job.OnLoopCompleted();
         }
         runningJobs.Clear();
         while(readyJobs.Count>0||blockedJobs.Count>0){
          while(readyJobs.Count>0){
           var job=DequeueReady();
           while(job.OnLoopExecuteStep(true)>=0){}
           job.OnLoopCompleted();
           PromoteBlockedJobs(job);
          }
          bool promoted=false;
          for(int i=blockedJobs.Count-1;i>=0;i--){
           var job=blockedJobs[i];
           if(job.dependency==null||!DependencyStillAlive(job.dependency)){
            RemoveBlockedJobAt(i,job);
            EnqueueReady(job);
            promoted=true;
           }
          }
          if(!promoted)break;
         }
         readyJobs.Clear();
         readyJobsSet.Clear();
         blockedJobs.Clear();
         blockedJobsSet.Clear();
         base.PreShutdown();
        }
        public override void Shutdown(){
         base.Shutdown();
        }
        IEnumerator Worker(){
         int workerLoops=0;
         while(true){
          while(readyJobs.Count<=0){
           yield return null;
          }
          int jobLoops=0;
          var job=DequeueReady();
          bool cancelled=job.isCancelledCanStop;
          if(cancelled){
           job.OnLoopCompleted();
           PromoteBlockedJobs(job);
           continue;
          }
          runningJobs.Add(job);
          int jobStillRunning=0;
          int steps=1;
          while(steps>0&&jobStillRunning>=0){
           workerLoops++;
           jobLoops++;
           steps--;
           jobStillRunning=job.OnLoopExecuteStep();
           if(jobStillRunning>0){
            steps+=jobStillRunning;
            if(!SharedCoroutineBudget.HasBudget(workerLoops)){
             break;
            }
            if(jobLoops>SharedCoroutineBudget.maxLoopsPerJob){
             break;
            }
           }else if(jobStillRunning==0){
            break;
           }
          }
          if(jobStillRunning>=0){
           EnqueueReady(job);
           runningJobs.Remove(job);
          }else{
           job.OnLoopCompleted();
           runningJobs.Remove(job);
           PromoteBlockedJobs(job);
          }
          if(!SharedCoroutineBudget.HasBudget(workerLoops)){
           yield return null;
           workerLoops=0;
          }
         }
        }
        static void PromoteBlockedJobs(SharedCoroutineContainerJob completed){
         for(int i=blockedJobs.Count-1;i>=0;i--){
          var job=blockedJobs[i];
          if(job.dependency==completed||!DependencyStillAlive(job.dependency)){
           job.dependency=null;
           RemoveBlockedJobAt(i,job);
           EnqueueReady(job);
          }
         }
        }
     [SerializeField]private double maxFrameTime=0.002d;
     [SerializeField]private int maxLoopsPerJob=8;
     [SerializeField]private int maxLoopsPerRoutine=729;
        internal static class SharedCoroutineBudget{
         internal static double frameStartTime;
         internal static double maxFrameTime=0.002d;//  ...unidade: em segundos;
         internal static int maxLoopsPerJob=8;
         internal static int maxLoopsPerRoutine=729;
            internal static void BeginFrame(){
             frameStartTime=Time.realtimeSinceStartupAsDouble;
            }
            internal static bool HasBudget(int routineLoopsDoneThisFrame){
             if(routineLoopsDoneThisFrame>=maxLoopsPerRoutine){return false;}
             double elapsed=(Time.realtimeSinceStartupAsDouble-frameStartTime);
             return elapsed<maxFrameTime;
            }
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         SharedCoroutineBudget.BeginFrame();
        }
        internal static bool TrySchedule(SharedCoroutineContainerJob job){
         Logs.Debug("'SharedCoroutines number of jobs before schedule':"+(readyJobs.Count+blockedJobs.Count));
         if(!running){return false;}
         job.OnScheduleSetContainerData();
         if(job.dependency==null||!DependencyStillAlive(job.dependency)){
          job.dependency=null;
          EnqueueReady(job);
         }else{
          EnqueueBlockedJob(job);
         }
         Logs.Debug("'SharedCoroutines number of jobs after':"+(readyJobs.Count+blockedJobs.Count));
         return true;
        }
        static bool DependencyStillAlive(SharedCoroutineContainerJob dependency){
         if(dependency==null)return false;
         if(runningJobs.Contains(dependency))return true;
         if(readyJobsSet.Contains(dependency))return true;
         if(blockedJobsSet.Contains(dependency))return true;
         return false;
        }
    }
}