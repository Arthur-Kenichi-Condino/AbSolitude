using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal class SharedCoroutines:MonoSingleton<SharedCoroutines>{
     public override int initOrder{get{return 0;}}
        internal interface SharedCoroutineContainerJob{
            void SetContainerDataOnBegin();
            bool ExecuteInLots(bool flush=false);
            void OnCompletedDoAtEnd();
        }
     static readonly Queue<SharedCoroutineContainerJob>scheduledJobs=new();
     static readonly HashSet<SharedCoroutineContainerJob>runningJobs=new();
     private int workerCount=8;
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
         base.PreShutdown();
        }
        public override void Shutdown(){
         base.Shutdown();
        }
        IEnumerator Worker(){
         WaitUntil waitUntil=new(()=>scheduledJobs.Count>0);
         while(true){
          yield return waitUntil;
          var job=scheduledJobs.Dequeue();
          runningJobs.Add(job);
          job.SetContainerDataOnBegin();
          while(!job.ExecuteInLots()){
           yield return null;
          }
          job.OnCompletedDoAtEnd();
          runningJobs.Remove(job);
         }
        }
    }
}