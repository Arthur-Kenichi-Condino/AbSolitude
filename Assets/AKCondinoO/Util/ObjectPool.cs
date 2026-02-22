using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace AKCondinoO.PoolUtil{
    ///<summary>
    ///  [Feito com ajuda da AI]
    ///  Object pool genérico, thread-safe. Use GetPool{T} para obter um pool singleton por tipo.
    /// Você pode fornecer factory e action de reset (limpeza) para cada tipo.
    ///</summary>
    internal sealed class ObjectPool<T>{
     readonly ConcurrentBag<T>bag=new ConcurrentBag<T>();
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
         if(preallocate>0){for(int i=0;i<preallocate;i++){bag.Add(this.factory());}}
        }
        internal T Rent(){
         if(bag.TryTake(out var item)){
          return item;
         }
         return factory();
        }
        internal void Return(T item){
         if(item is null)return;
         reset?.Invoke(item);
         bag.Add(item);
        }
     internal int count=>bag.Count;
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
    }
}