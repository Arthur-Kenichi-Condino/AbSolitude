using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
namespace AKCondinoO.Main{
    internal interface MultithreadedContainerJob{
        void BackgroundExecute();
        void OnCompletedDoAtMainThread();
    }
    internal static class ThreadDispatcher{
     private static volatile bool running;
     private static volatile bool acceptingJobs;
     private static Thread[]workers;
     private static readonly ConcurrentQueue<MultithreadedContainerJob>[]scheduledByPriority={
      new(),//  ...max priority
      new(),
      new(),
      new(),
     };
     private static readonly ConcurrentQueue<MultithreadedContainerJob>completed=new();
     private static int activeJobs;
     private static int maxConcurrentJobs;
     private static int workerCount;
        internal static void Initialize(int?setWorkerCount=null){
         if(running)return;
         int cpu=Environment.ProcessorCount;
         workerCount=setWorkerCount??Math.Max(1,cpu-2);
         maxConcurrentJobs=workerCount*2;
         workers=new Thread[workerCount];
         running=true;
         for(int i=0;i<workerCount;i++){
          workers[i]=new Thread(WorkerLoop);
          workers[i].IsBackground=false;
          workers[i].Priority=System.Threading.ThreadPriority.BelowNormal;
          workers[i].Start();
         }
         acceptingJobs=true;
        }
        internal static void Shutdown(){
         acceptingJobs=false;
         running=false;
         if(workers==null)return;
         for(int i=0;i<workers.Length;i++){
          workers[i]?.Join();
         }
         workers=null;
        }
        private static void WorkerLoop(){
         SpinWait spin=new SpinWait();
         while(running||HasPendingWork()){
          MultithreadedContainerJob job=Dequeue(out int priority);
          if(job==null){
           spin.SpinOnce();
           continue;
          }
          if(Interlocked.Increment(ref activeJobs)>maxConcurrentJobs){
           Interlocked.Decrement(ref activeJobs);
           scheduledByPriority[priority].Enqueue(job);
           spin.SpinOnce();
           continue;
          }
          spin.Reset();
          try{
           job.BackgroundExecute();
          }catch(Exception e){
           //  TO DO: usar sistema de Log
           //Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          }
          completed.Enqueue(job);
          Interlocked.Decrement(ref activeJobs);
         }
        }
        private static MultithreadedContainerJob Dequeue(out int priority){
         priority=-1;
         for(int i=0;i<scheduledByPriority.Length;i++){
          if(scheduledByPriority[i].TryDequeue(out var job)){priority=i;return job;}
         }
         return null;
        }
        private static bool HasPendingWork(){
         if(Volatile.Read(ref activeJobs)>0)
          return true;
         for(int i=0;i<scheduledByPriority.Length;i++)
          if(!scheduledByPriority[i].IsEmpty)
           return true;
         return false;
        }
        internal static bool TrySchedule(MultithreadedContainerJob job,int priority){
         if(!acceptingJobs)return false;
         if(!running)return false;
         priority=Math.Clamp(priority,0,scheduledByPriority.Length-1);
         scheduledByPriority[priority].Enqueue(job);
         return true;
        }
        internal static void FlushCompleted(){
         while(completed.TryDequeue(out var job)){
          try{
           job.OnCompletedDoAtMainThread();
          }catch(Exception e){
           //  TO DO: usar sistema de Log
           //Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          }
         }
        }
    }
}