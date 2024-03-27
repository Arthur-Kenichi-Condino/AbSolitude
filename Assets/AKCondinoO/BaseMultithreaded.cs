#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace AKCondinoO{
    internal abstract class BackgroundContainer:IDisposable{
     internal readonly ManualResetEvent backgroundData=new ManualResetEvent( true);
     internal readonly   AutoResetEvent foregroundData=new   AutoResetEvent(false);
        internal bool IsCompleted(Func<bool>isRunning,int millisecondsTimeout=0){
         if(millisecondsTimeout<0&&isRunning.Invoke()!=true){
          return true;
         }
         return backgroundData.WaitOne(millisecondsTimeout);
        }
     protected bool disposed=false;
        public void Dispose(){
         Dispose(disposing:true);
         GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
          backgroundData.Dispose();
          foregroundData.Dispose();
         }
         //  free unmanaged resources here
         disposed=true;
        }
        ~BackgroundContainer(){
         Dispose(disposing:false);
        }
    }
    internal abstract class BaseMultithreaded<T>where T:BackgroundContainer{
     internal static bool Stopped{
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
     static readonly List<BaseMultithreaded<T>>allThreadsStarted=new List<BaseMultithreaded<T>>();
        internal static void Start(BaseMultithreaded<T>[]threads,ConstructorInfo ctorInfo,object[]ctorParams){
         Log.DebugMessage("BaseMultithreaded<"+typeof(T)+">:threads.Length:"+threads.Length);
         if(Stopped){
          Stopped=false;
         }
         if(threads!=null&&threads.Length>0){
          for(int i=0;i<threads.Length;++i){
                        threads[i]=Start(ctorInfo,ctorParams);
          }
         }
        }
        internal static BaseMultithreaded<T>Start(ConstructorInfo ctorInfo,object[]ctorParams){
         Log.DebugMessage("BaseMultithreaded<"+typeof(T)+">:start thread");
         if(Stopped){
          Stopped=false;
         }
         BaseMultithreaded<T>thread=(BaseMultithreaded<T>)ctorInfo.Invoke(ctorParams);
         allThreadsStarted.Add(thread);
         return thread;
        }
        internal static bool Stop(params BaseMultithreaded<T>[]allThreads){
         if(Clear()!=0){
          Log.Error(((allThreads!=null&&allThreads.Length>0&&allThreads[0]!=null)?allThreads[0].GetType():"BaseMultithreaded<"+typeof(T)+">")+" will stop with pending work");
         }
         Stopped=true;
         bool result=false;
         if(allThreads!=null&&allThreads.Length>0){
          for(int i=0;i<allThreads.Length;++i){
                     if(allThreads[i]==null){continue;}
                        allThreads[i].Wait();
          }
          result=true;
         }
         if(allThreadsStarted.Count>0){
          foreach(BaseMultithreaded<T>thread in allThreadsStarted){
           thread.Wait();
          }
         }
         allThreadsStarted.Clear();
         return result;
        }
     readonly System.Threading.ThreadPriority priority;
     readonly Task task;
        internal BaseMultithreaded(System.Threading.ThreadPriority priority=System.Threading.ThreadPriority.BelowNormal){
         this.priority=priority;
         Core.threadCount++;
         task=Task.Factory.StartNew(BG,TaskCreationOptions.LongRunning);
        }
     protected T container{get;private set;}
        void BG(){Thread.CurrentThread.IsBackground=false;
         Thread.CurrentThread.Priority=priority;
         ManualResetEvent backgroundData;
           AutoResetEvent foregroundData;
         while(!Stopped){enqueued.WaitOne();if(Stopped){enqueued.Set();goto _Stop;}
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
         //Log.DebugMessage("Background task ending gracefully!");
        }
        protected virtual void Renew(T next){}
        protected abstract void Execute();
        protected virtual void Release(){}
        protected virtual void Cleanup(){}
        internal bool IsRunning(){
         return Stopped==false&&task!=null&&!task.IsCompleted;
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