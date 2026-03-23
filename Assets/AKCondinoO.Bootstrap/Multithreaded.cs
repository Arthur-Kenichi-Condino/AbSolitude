using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal interface MultithreadedContainerJob{
        void CancelGraciously();
        void OnDoScheduleSetContainerData();
        void ExecuteAtBackgroundThread();
        void OnCompletedDoAtMainThread();
    }
    internal static class ThreadDispatcher{
     private static int running;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsRunning()=>Volatile.Read(ref running)==1;
     private static Thread[]workers;
     private static readonly ConcurrentQueue<MultithreadedContainerJob>[]scheduledByPriority={
      new(),new(),new(),new(),new(),new(),new(),new(),//  ...de max priority para min priority
     };
     private static readonly ConcurrentQueue<MultithreadedContainerJob>completed=new();
     private static int maxConcurrentJobs;
     private static int workerCount;
         private static int accepting;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool IsAccepting()=>Volatile.Read(ref accepting)==1;
     private static int activeJobs;
        internal static void Initialize(int?setWorkerCount=null){
         if(Interlocked.Exchange(ref running,1)==1)return;
         Interlocked.Exchange(ref accepting,1);
         int cpu=Environment.ProcessorCount;
         workerCount=setWorkerCount??Math.Max(1,cpu-2);
         maxConcurrentJobs=workerCount;
         workers=new Thread[workerCount];
         for(int i=0;i<workerCount;i++){
          workers[i]=new Thread(WorkerLoop);
          workers[i].IsBackground=false;
          workers[i].Priority=System.Threading.ThreadPriority.BelowNormal;
          workers[i].Start();
         }
        }
        internal static void Shutdown(){
         Interlocked.Exchange(ref accepting,0);
         while(Volatile.Read(ref threadsEnqueueing)>0){
          Thread.Yield();
         }
         Interlocked.Exchange(ref running,0);
         if(workers==null)return;
         for(int i=0;i<workers.Length;i++){
          workers[i]?.Join();
         }
         workers=null;
        }
        private static void WorkerLoop(){
         SpinWait spin=new SpinWait();
         while(IsRunning()||HasPendingWork()){
          int jobs=Volatile.Read(ref activeJobs);
          if(jobs>=maxConcurrentJobs){
           spin.SpinOnce();
           continue;
          }
          MultithreadedContainerJob job=Dequeue(out int priority);
          if(job==null){
           spin.SpinOnce();
           continue;
          }
          if(Interlocked.CompareExchange(ref activeJobs,jobs+1,jobs)!=jobs){
           scheduledByPriority[priority].Enqueue(job);
           spin.SpinOnce();
           continue;
          }
          spin.Reset();
          try{
           job.ExecuteAtBackgroundThread();
          }catch(Exception e){
           Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
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
         if(Volatile.Read(ref threadsEnqueueing)>0)
          return true;
         for(int i=0;i<scheduledByPriority.Length;i++)
          if(!scheduledByPriority[i].IsEmpty)
           return true;
         return false;
        }
     private static int threadsEnqueueing;
        internal static bool TrySchedule(MultithreadedContainerJob job,int priority=0){
         Interlocked.Increment(ref threadsEnqueueing);
         if(!IsRunning()||!IsAccepting()){
          Interlocked.Decrement(ref threadsEnqueueing);
          return false;
         }
         try{
          job.OnDoScheduleSetContainerData();
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }
         priority=Math.Clamp(priority,0,scheduledByPriority.Length-1);
         scheduledByPriority[priority].Enqueue(job);
         Interlocked.Decrement(ref threadsEnqueueing);
         return true;
        }
        internal static void FlushCompleted(bool shutdown=false){
         const double maxTimePerFrame=0.001d;//  ...unidade: em segundos
         double startTime=Time.realtimeSinceStartupAsDouble;
         while(completed.TryDequeue(out var job)){
          try{
           job.OnCompletedDoAtMainThread();
          }catch(Exception e){
           Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          }
          if(ShouldYield()){
           break;
          }
         }
         bool ShouldYield(){
          if(shutdown){
           return false;
          }
          if(Time.realtimeSinceStartupAsDouble-startTime>=maxTimePerFrame){
           startTime=Time.realtimeSinceStartupAsDouble;
           return true;
          }
          return false;
         }
        }
    }
}