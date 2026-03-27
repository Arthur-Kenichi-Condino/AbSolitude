using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    ///<summary>
    ///  Scheduler cooperativo de "coroutines" compartilhadas para execução de trabalhos leves no main thread.
    ///  
    ///  Propósito:
    /// - manter um pequeno conjunto de workers (coroutines Unity) que processam jobs leves implementando
    /// <see cref="SharedCoroutineContainerJob"/> de forma cooperativa.
    /// - oferecer API de agendamento (<see cref="TrySchedule"/>) para produtores enfileirarem jobs.
    /// - aplicar cotas por frame e por job via <see cref="SharedCoroutineBudget"/> para evitar travamentos
    /// e distribuir trabalho ao longo dos frames.
    ///  
    ///  Modelo de concorrência / invariantes:
    /// - todas as coleções (ready, blocked, running) são estáticas e compartilhadas entre workers.
    /// - produtores devem chamar <see cref="TrySchedule"/> da main thread; assim como as callbacks
    /// de execução (OnLoopExecuteStep / OnLoopCompleted) são invocadas pelos workers no main thread.
    /// - o snapshot de jobs segue este ciclo de vida: enfileirado (ready) -> running -> re-enfileirado (ready) ou concluído.
    /// - dependências entre jobs são expressas por <c>dependency</c>; um job bloqueado permanece em blocked até dependency ser executada (sair de todas as filas).
    ///  
    ///  Observações de segurança:
    /// - chamadores que acessam APIs de snapshot/ruído paralelamente devem observar locks apropriados.
    /// - não altere as coleções estáticas sem compreender as implicações de race conditions.
    ///</summary>
    internal class SharedCoroutines:MonoSingleton<SharedCoroutines>{
        internal interface SharedCoroutineContainerJob{
            SharedCoroutineContainerJob dependency{get;set;}
            bool isCancelledCanStop{get;set;}
            void CancelGraciously();
            void OnScheduleSetContainerData();
            int OnLoopExecuteStep(bool flush=false);
            void OnLoopCompleted();
        }
     private static bool running;
     [SerializeField]private int workerCount=8;
     private readonly List<Coroutine>coroutines=new();
     static readonly Queue<SharedCoroutineContainerJob>[]readyJobsByPriority={
      new(),new(),new(),new(),new(),new(),new(),new(),//  ...de max priority para min priority
     };
     static readonly HashSet<SharedCoroutineContainerJob>readyJobsSet=new();
        static void EnqueueReady(SharedCoroutineContainerJob job,int priority){
         readyJobsByPriority[priority].Enqueue(job);readyJobsSet.Add(job);
        }
        static SharedCoroutineContainerJob DequeueReady(int priority){
         var job=readyJobsByPriority[priority].Dequeue();readyJobsSet.Remove(job);
         return job;
        }
     static readonly List<SharedCoroutineContainerJob>[]blockedJobsByPriority={
      new(),new(),new(),new(),new(),new(),new(),new(),//  ...de max priority para min priority
     };
     static readonly HashSet<SharedCoroutineContainerJob>blockedJobsSet=new();
        static void EnqueueBlockedJob(SharedCoroutineContainerJob job,int priority){
         blockedJobsByPriority[priority].Add(job);blockedJobsSet.Add(job);
        }
        static void RemoveBlockedJobAt(int j,SharedCoroutineContainerJob job,int priority){
         blockedJobsByPriority[priority].RemoveAt(j);blockedJobsSet.Remove(job);
        }
     static readonly HashSet<SharedCoroutineContainerJob>runningJobs=new();
        public override void Initialize(){
         base.Initialize();
         SharedCoroutineBudget.maxFrameTime=maxFrameTime;
         SharedCoroutineBudget.maxLoopsPerJob=maxLoopsPerJob;
         SharedCoroutineBudget.maxLoopsPerRoutine=maxLoopsPerRoutine;
         int cpu=Environment.ProcessorCount;
         int workerCount=this.workerCount<=0?Math.Max(1,cpu-2):this.workerCount;
         workerCount=Math.Clamp(workerCount,1,  readyJobsByPriority.Length);
         workerCount=Math.Clamp(workerCount,1,blockedJobsByPriority.Length);
         int maxPriority=0;
         maxPriority=Math.Max(maxPriority,  readyJobsByPriority.Length-1);
         maxPriority=Math.Max(maxPriority,blockedJobsByPriority.Length-1);
         if(this!=null){
          running=true;
          for(int i=0;i<workerCount;i++){
           int startPriority=i;
           int endPriority=i;
           if(i>=workerCount-1){
            if(endPriority<maxPriority){endPriority=maxPriority;}
           }
           coroutines.Add(StartCoroutine(Worker(startPriority,endPriority)));
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
         int length=Math.Min(readyJobsByPriority.Length,blockedJobsByPriority.Length);
         for(int i=0;i<length;i++){
          var readyJobs=readyJobsByPriority[i];
          var blockedJobs=blockedJobsByPriority[i];
          while(readyJobs.Count>0||blockedJobs.Count>0){
           while(readyJobs.Count>0){
            var job=DequeueReady(i);
            while(job.OnLoopExecuteStep(true)>=0){}
            job.OnLoopCompleted();
            PromoteBlockedJobs(job,i);
           }
           bool promoted=false;
           for(int j=blockedJobs.Count-1;j>=0;j--){
            var job=blockedJobs[j];
            if(job.dependency==null||!DependencyStillAlive(job.dependency)){
             RemoveBlockedJobAt(j,job,i);
             EnqueueReady(job,i);
             promoted=true;
            }
           }
           if(!promoted)break;
          }
          readyJobs.Clear();
          blockedJobs.Clear();
         }
         readyJobsSet.Clear();
         blockedJobsSet.Clear();
         base.PreShutdown();
        }
        public override void Shutdown(){
         base.Shutdown();
        }
        IEnumerator Worker(int startPriority,int endPriority){
         int workerLoops=0;
         while(true){
          for(int priority=startPriority;priority<=endPriority;priority++){
           while(readyJobsByPriority[priority].Count<=0){
            yield return null;
            PromoteBlockedJobs(null,priority);
           }
           int jobLoops=0;
           var job=DequeueReady(priority);
           bool cancelled=job.isCancelledCanStop;
           if(cancelled){
            job.OnLoopCompleted();
            PromoteBlockedJobs(job,priority);
            continue;
           }
           runningJobs.Add(job);
           int jobStillLooping=0;
           int steps=1;
           while(steps>0&&jobStillLooping>=0){
            workerLoops++;
            jobLoops++;
            steps--;
            jobStillLooping=job.OnLoopExecuteStep();
            if(jobStillLooping>0){
             steps+=jobStillLooping;
             if(!SharedCoroutineBudget.HasBudget(workerLoops)){
              break;
             }
             if(jobLoops>SharedCoroutineBudget.maxLoopsPerJob){
              break;
             }
            }else if(jobStillLooping==0){
             break;
            }
           }
           if(jobStillLooping>=0){
            EnqueueReady(job,priority);
            runningJobs.Remove(job);
           }else{
            job.OnLoopCompleted();
            runningJobs.Remove(job);
            PromoteBlockedJobs(job,priority);
           }
           if(!SharedCoroutineBudget.HasBudget(workerLoops)){
            yield return null;
            workerLoops=0;
           }
          }
         }
        }
        static void PromoteBlockedJobs(SharedCoroutineContainerJob completed,int priority){
         var blockedJobs=blockedJobsByPriority[priority];
         for(int j=blockedJobs.Count-1;j>=0;j--){
          var job=blockedJobs[j];
          if(job.dependency==completed||!DependencyStillAlive(job.dependency)){
           job.dependency=null;
           RemoveBlockedJobAt(j,job,priority);
           EnqueueReady(job,priority);
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
        internal static bool TrySchedule(SharedCoroutineContainerJob job,int priority=0){
         priority=Math.Clamp(priority,0,  readyJobsByPriority.Length-1);
         priority=Math.Clamp(priority,0,blockedJobsByPriority.Length-1);
         Logs.Debug(()=>"'SharedCoroutines number of jobs before schedule':"+(readyJobsByPriority[priority].Count+blockedJobsByPriority[priority].Count));
         if(!running){return false;}
         job.OnScheduleSetContainerData();
         if(job.dependency==null||!DependencyStillAlive(job.dependency)){
          job.dependency=null;
          EnqueueReady(job,priority);
         }else{
          EnqueueBlockedJob(job,priority);
         }
         Logs.Debug(()=>"'SharedCoroutines number of jobs after':"+(readyJobsByPriority[priority].Count+blockedJobsByPriority[priority].Count));
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