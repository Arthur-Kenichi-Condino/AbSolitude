using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace AKCondinoO.Utilities{
    ///<summary>
    ///  [Feito com ajuda da AI]
    ///</summary>
    internal class LeastRecentlyUsedCache{
     static readonly ConcurrentDictionary<CacheKey,Lazy<IObjectCache>>caches=new();
        readonly struct CacheKey:IEquatable<CacheKey>{
         readonly Type keyType;
         readonly Type valueType;
         readonly string id;
         readonly int maxEntries;
            internal CacheKey(Type keyType,Type valueType,string id,int maxEntries){
             this.keyType=keyType;
             this.valueType=valueType;
             this.id=id;
             this.maxEntries=maxEntries;
            }
            public bool Equals(CacheKey other)=>
             keyType==other.keyType&&
             valueType==other.valueType&&
             id==other.id&&
             maxEntries==other.maxEntries;
            public override bool Equals(object obj)=>
             obj is CacheKey other&&Equals(other);
            public override int GetHashCode()=>
             HashCode.Combine(keyType,valueType,id,maxEntries);
        }
        internal static ObjectCache<TKey,TValue>GetCache<TKey,TValue>(
         string id,
         int maxEntries
        )where TKey:notnull,IEquatable<TKey>{
         var key=new CacheKey(typeof(TKey),typeof(TValue),id,maxEntries);
         return(ObjectCache<TKey,TValue>)caches.GetOrAdd(
          key,
          _=>new Lazy<IObjectCache>(
           ()=>new ObjectCache<TKey,TValue>(maxEntries),
           LazyThreadSafetyMode.ExecutionAndPublication
          )
         ).Value;
        }
        internal static void Shutdown(){
         Clear();
        }
        internal static void Clear(){
         foreach(var cache in caches.Values){
          cache.Value.Clear();
         }
        }
        interface IObjectCache{
         void Clear();
        }
        internal sealed class ObjectCache<TKey,TValue>:IObjectCache where TKey:notnull,IEquatable<TKey>{
         readonly ReaderWriterLockSlim sync=new(LockRecursionPolicy.NoRecursion);
         readonly Dictionary<TKey,LinkedListNode<Entry>>items=new();
         readonly LinkedList<Entry>lru=new();
         readonly int maxEntries;
            readonly struct Entry{
             internal readonly TKey key;
             internal readonly TValue value;
             internal Entry(TKey key,TValue value){
              this.key=key;
              this.value=value;
             }
            }
            internal ObjectCache(int maxEntries){
             this.maxEntries=maxEntries;
            }
            internal bool TryGet(TKey key,out TValue value){
             sync.EnterReadLock();
             try{
              if(items.TryGetValue(key,out var node)){
               value=node.Value.value;
               return true;
              }
             }catch{
              throw;
             }finally{
              sync.ExitReadLock();
             }
             value=default!;
             return false;
            }
            internal void Set(TKey key,TValue value){
             sync.EnterWriteLock();
             try{
              if(items.TryGetValue(key,out var node)){
               node.Value=new Entry(key,value);
               lru.Remove(node);
               lru.AddFirst(node);
               return;
              }
              var newNode=lru.AddFirst(new Entry(key,value));
              items.Add(key,newNode);
              if(items.Count>maxEntries){
               var last=lru.Last!;
               lru.RemoveLast();
               items.Remove(last.Value.key);
              }
             }catch{
              throw;
             }finally{
              sync.ExitWriteLock();
             }
            }
            public void Clear(){
             sync.EnterWriteLock();
             try{
              items.Clear();
              lru.Clear();
             }catch{
              throw;
             }finally{
              sync.ExitWriteLock();
             }
            }
            internal int Count{
             get{
              sync.EnterReadLock();
              try{
               return items.Count;
              }catch{
               throw;
              }finally{
               sync.ExitReadLock();
              }
             }
            }
        }
    }
}