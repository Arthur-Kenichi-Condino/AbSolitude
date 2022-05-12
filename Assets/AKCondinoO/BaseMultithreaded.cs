#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace AKCondinoO{
    internal abstract class BackgroundContainer{
     internal readonly ManualResetEvent backgroundData=new ManualResetEvent( true);
     internal readonly   AutoResetEvent foregroundData=new   AutoResetEvent(false);
        internal bool IsCompleted(Func<bool>isRunning,int millisecondsTimeout=0){
         if(millisecondsTimeout<0&&isRunning.Invoke()!=true){
          return true;
         }
         return backgroundData.WaitOne(millisecondsTimeout);
        }
    }
    internal abstract class BaseMultithreaded<T>where T:BackgroundContainer{
     internal static bool Stop{
      get{bool tmp;lock(Stop_syn){tmp=Stop_v;      }return tmp;}
      set{         lock(Stop_syn){    Stop_v=value;}if(value){enqueued.Set();}}
     }static bool Stop_v=false;static readonly object Stop_syn=new object();
     static readonly AutoResetEvent enqueued=new AutoResetEvent(false);
     static readonly ConcurrentQueue<T>queued=new ConcurrentQueue<T>();
        internal static void Schedule(T next){
         next.backgroundData.Reset();
         next.foregroundData.Set();
         queued.Enqueue(next);
         enqueued.Set();
        }
        internal static int Clear(){
         int count=queued.Count;
         while(queued.TryDequeue(out T dequeued)){
          dequeued.foregroundData.WaitOne(0);
          dequeued.backgroundData.Set();
         }
         return count;
        }
     readonly Task task;
        internal BaseMultithreaded(){
         Core.threadCount++;
         task=Task.Factory.StartNew(BG,TaskCreationOptions.LongRunning);
        }
     protected T container{get;private set;}
        void BG(){Thread.CurrentThread.IsBackground=false;
         ManualResetEvent backgroundData;
           AutoResetEvent foregroundData;
         while(!Stop){enqueued.WaitOne();if(Stop){enqueued.Set();goto _Stop;}
          if(queued.TryDequeue(out T dequeued)){
           container=dequeued;
           foregroundData=container.foregroundData;
           backgroundData=container.backgroundData;
           try{
            Renew(dequeued);
           }catch(Exception e){
            Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
           }
          }else{
           continue;
          };
          if(queued.Count>0){
           enqueued.Set();
          }
          foregroundData.WaitOne();
          try{
           Execute();
          }catch(Exception e){
           Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          }
          try{
           Release();
          }catch(Exception e){
           Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          }
          backgroundData.Set();
          container=null;
          try{
           Cleanup();
          }catch(Exception e){
           Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          }
         }
         _Stop:{}
         Log.DebugMessage("Background task ending gracefully!");
        }
        protected virtual void Renew(T next){}
        protected abstract void Execute();
        protected virtual void Release(){}
        protected virtual void Cleanup(){}
        internal bool IsRunning(){
         return Stop==false&&task!=null&&!task.IsCompleted;
        }
        internal void Wait(){
         try{
          task.Wait();
          Core.threadCount--;
         }catch(Exception e){
          Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }
        }
    }
}