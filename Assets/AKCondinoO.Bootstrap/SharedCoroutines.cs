using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal class SharedCoroutines:MonoSingleton<SharedCoroutines>{
     public override int initOrder{get{return 0;}}
        internal interface SharedCoroutineContainerJob{
            void SetContainerDataOnBegin();
            bool LoopExecuteStep(bool flush=false);
            void OnCompletedDoAtEnd();
        }
     static readonly Queue<SharedCoroutineContainerJob>scheduledJobs=new();
     static readonly HashSet<SharedCoroutineContainerJob>runningJobs=new();
     [SerializeField]private int workerCount=8;
     private readonly List<Coroutine>coroutines=new();
        public override void Initialize(){
         base.Initialize();
         for(int i=0;i<workerCount;i++){
          coroutines.Add(StartCoroutine(Worker()));
         }
        }
        public override void PreShutdown(){
         if(this!=null){
          for(int i=0;i<coroutines.Count;i++){
           StopCoroutine(coroutines[i]);
          }
         }
         coroutines.Clear();
         foreach(var job in runningJobs){
          while(job.LoopExecuteStep(true)){}
          job.OnCompletedDoAtEnd();
         }
         runningJobs.Clear();
         while(scheduledJobs.Count>0){
          var job=scheduledJobs.Dequeue();
          job.SetContainerDataOnBegin();
          while(job.LoopExecuteStep(true)){}
          job.OnCompletedDoAtEnd();
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
          job.SetContainerDataOnBegin();
          while(job.LoopExecuteStep()){
           yield return null;
          }
          job.OnCompletedDoAtEnd();
          runningJobs.Remove(job);
         }
        }
        internal static void Schedule(SharedCoroutineContainerJob job){
         scheduledJobs.Enqueue(job);
        }
    }
}