using AKCondinoO.Bootstrap;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
namespace AKCondinoO.Utilities{
    ///<summary>
    ///  [Feito com ajuda da AI]
    ///  Object pool genérico, thread-safe. Use GetPool{T} para obter um pool singleton por tipo.
    /// Você pode fornecer factory e action de reset (limpeza) para cada tipo.
    ///</summary>
    internal sealed class ObjectPool<T>:ObjectPoolBase{
     readonly ConcurrentQueue<T>bag=new();
     internal readonly Func<T>    factory;
     internal readonly MethodInfo factoryMethod;
     internal readonly object     factoryTarget;
     internal readonly Action<T>  reset;
     internal readonly MethodInfo resetMethod;
     internal readonly object     resetTarget;
        internal ObjectPool(Func<T>factory,Action<T>reset=null,int preallocate=0){
         this.factory=factory;
         this.factoryMethod=factory.Method;
         this.factoryTarget=factory.Target;
         this.reset=reset;
         this.resetMethod=reset?.Method;
         this.resetTarget=reset?.Target;
         staticReturn=ObjectPool<T>.StaticReturn;
         if(preallocate>0){for(int i=0;i<preallocate;i++){bag.Enqueue(this.factory());}}
        }
        internal T Rent(){
         if(bag.TryDequeue(out var item)){
          return item;
         }
         return factory();
        }
        internal override object ObjectRent(){
         return Rent();
        }
        internal void Return(T item){
         if(item is null)return;
         Pool.MultithreadedReturnDispatcher.Enqueue(this,item);
        }
        internal void MultithreadedReturn(T item){
         reset?.Invoke(item);
         bag.Enqueue(item);
        }
        internal override void ObjectReturn(object item){
         Return((T)item);
        }
     internal int bagCount=>bag.Count;
     internal readonly Action<object,object>staticReturn;
        private static void StaticReturn(object poolObj,object itemObj){
         var pool=(ObjectPool<T>)poolObj;
         var item=(T)itemObj;
         pool.MultithreadedReturn(item);
        }
    }
    internal abstract class ObjectPoolBase{
        internal abstract object ObjectRent();internal abstract void ObjectReturn(object item);
    }
    ///<summary>
    ///  Gerenciador de pools por tipo.
    ///  Ex.: var hs=Pool.GetPool<HashSet<Vector3Int>>(()=>new HashSet<Vector3Int>(),h=>h.Clear()).Rent();
    /// Pool.GetPool<HashSet<Vector3Int>>().Return(hs);
    ///</summary>
    internal static class Pool{
     static readonly ConcurrentDictionary<(Type type,string id),object>pools=new();
        internal static ObjectPool<T>GetPool<T>(string id,Func<T>factory=null,Action<T>reset=null,int preallocate=0){
         var key=(typeof(T),id);
         bool requestedNonDefault=factory!=null;
         var requestedFactory=factory??CreateDefaultFactory<T>();
         if(requestedFactory==null){
          return null;
         }
         ObjectPool<T>created=null;
         ObjectPool<T>result=(ObjectPool<T>)pools.GetOrAdd(key,
          _=>{
           created=new ObjectPool<T>(
            requestedFactory,
            reset,
            preallocate
           );
           return created;
          }
         );
         if(!ReferenceEquals(result,created)){
          if(!FactoriesMatch(result,requestedFactory)){
           return null;
          }
          if(!ResetsMatch(result,reset)){
           return null;
          }
         }
         return result;
        }
        static bool FactoriesMatch<T>(ObjectPool<T>pool,Func<T>factory){
         if(pool.factoryMethod!=factory.Method){
          return false;
         }
         if(!ReferenceEquals(pool.factoryTarget,factory.Target)){
          return false;
         }
         return true;
        }
        static bool ResetsMatch<T>(ObjectPool<T>pool,Action<T>reset){
         if(reset==null)
          return pool.reset==null;
         if(pool.resetMethod!=reset.Method){
          return false;
         }
         if(!ReferenceEquals(pool.resetTarget,reset.Target)){
          return false;
         }
         return true;
        }
        static Func<T>CreateDefaultFactory<T>(){
         //  Try to handle common cases: IList, ICollection, arrays and parameterless ctor
         var t=typeof(T);
         if(t.IsArray)return()=>(T)Activator.CreateInstance(t,0);
         return()=>Activator.CreateInstance<T>();
        }
        #region  Array helpers (uses ArrayPool<T> under the hood)
        internal static T[]RentArray<T>(int minimumLength){
         return ArrayPool<T>.Shared.Rent(minimumLength);
        }
        internal static void ReturnArray<T>(T[]array,bool clearArray=true){
         if(array==null)return;
         ArrayPool<T>.Shared.Return(array,clearArray);
        }
        #endregion
        struct MultithreadedReturnJob{
         public object pool;
         public object item;
         public Action<object,object>staticReturn;
        }
        internal static class MultithreadedReturnDispatcher{
         private static int running;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool IsRunning()=>Volatile.Read(ref running)==1;
         private static Thread[]workers;
         private static readonly ConcurrentQueue<MultithreadedReturnJob>scheduled=new();
         private static int workerCount;
         private static int accepting;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool IsAccepting()=>Volatile.Read(ref accepting)==1;
         private static int inFlight;
            internal static void Initialize(int?setWorkerCount=null){
             if(Interlocked.Exchange(ref running,1)==1)return;
             Interlocked.Exchange(ref accepting,1);
             int cpu=Environment.ProcessorCount;
             workerCount=setWorkerCount??Math.Max(1,cpu-2);
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
              if(scheduled.TryDequeue(out var job)){
               try{
                job.staticReturn(job.pool,job.item);
               }catch(Exception e){
                Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
               }finally{
                Interlocked.Decrement(ref inFlight);
               }
              }else{
               spin.SpinOnce();
              }
             }
            }
            private static bool HasPendingWork(){
             if(Volatile.Read(ref inFlight)>0)
              return true;
             if(Volatile.Read(ref threadsEnqueueing)>0)
              return true;
             if(!scheduled.IsEmpty){
              return true;
             }
             return false;
            }
         private static int threadsEnqueueing;
            internal static void Enqueue<T>(ObjectPool<T>pool,T item){
             Interlocked.Increment(ref threadsEnqueueing);
             if(!IsRunning()||!IsAccepting()){
              Interlocked.Decrement(ref threadsEnqueueing);
              pool.MultithreadedReturn(item);
              return;
             }
             Interlocked.Increment(ref inFlight);
             scheduled.Enqueue(
              new MultithreadedReturnJob{
               pool=pool,
               item=item,
               staticReturn=pool.staticReturn
              }
             );
             Interlocked.Decrement(ref threadsEnqueueing);
            }
        }
    }
}