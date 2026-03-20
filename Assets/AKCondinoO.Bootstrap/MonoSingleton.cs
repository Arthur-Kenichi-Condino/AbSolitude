using AKCondinoO.SimObjects;
using AKCondinoO.World;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal static class SingletonManager{
     internal static readonly Dictionary<Type,int>initOrderTable=new(){
      {typeof(SharedCoroutines ), 0},
      {typeof(Main             ), 1},
      {typeof(InputHandler     ), 2},
      {typeof(MainCamera       ), 3},
      {typeof(BiomesSystem     ), 4},
      {typeof(WorldChunkManager), 5},
      {typeof(SimObjectManager ), 6},
     };
     private static readonly List<ISingleton>singletons=new();
        internal static void Register(ISingleton singleton){
         if(singleton==null||singletons.Contains(singleton)){
          Logs.Error($"invalid or duplicate singleton:{singleton}");
          return;
         }
         if(singletons.Any(s=>s.initOrder==singleton.initOrder)){
          Logs.Error($"invalid or duplicate initOrder:{singleton.initOrder}");
          return;
         }
         singletons.Add(singleton);
         Logs.Debug("'added singleton':"+$"(singleton.initOrder:{singleton.initOrder}):"+singleton+$";singletons.Count:{singletons.Count}");
        }
        private static void UnregisterAt(int i){
         var s=singletons[i];
         var mono=s as MonoBehaviour;
         if(mono!=null){
          GameObject.Destroy(mono.gameObject);
         }
         s.ClearStaticInstance();
         singletons.RemoveAt(i);
        }
        internal static void InitializeAll(){
         singletons.Sort((a,b)=>a.initOrder.CompareTo(b.initOrder));
         foreach(var s in singletons){
          Logs.Debug("'init singleton':"+s);
          s.Initialize();
         }
        }
        internal static void PreShutdownAll(){
         foreach(var s in singletons){
          Logs.Debug("'singleton pre-shutdown':"+s);
          s.PreShutdown();
         }
        }
        internal static void ShutdownAll(){
         for(int i=singletons.Count-1;i>=0;i--){
          var s=singletons[i];
          Logs.Debug("'singleton shutdown':"+s);
          s.Shutdown();
          UnregisterAt(i);
         }
         singletons.Clear();
        }
        internal static void ManualUpdateAll(){
         foreach(var s in singletons)s.ManualUpdate();
        }
    }
    internal interface ISingleton{
     int initOrder{get;}
        void ClearStaticInstance();
        void Initialize();
        void PreShutdown();
        void Shutdown();
        void ManualUpdate();
    }
    internal abstract class MonoSingleton<T>:MonoBehaviour,ISingleton where T:MonoBehaviour{
     public static T singleton{get;protected set;}
        public void ClearStaticInstance(){
         singleton=null;
        }
     public virtual int initOrder{get{return SingletonManager.initOrderTable[GetType()];}}
        protected virtual void Awake(){
         if(!object.ReferenceEquals(singleton,null)&&singleton!=this){
          DestroyImmediate(gameObject);
          return;
         }
         singleton=this as T;
         DontDestroyOnLoad(gameObject);
         SingletonManager.Register(this);
        }
        protected virtual void OnDestroy(){
         if(singleton==this){
         }
        }
        public virtual void Initialize(){
        }
        public virtual void PreShutdown(){
        }
        public virtual void Shutdown(){
        }
        public virtual void ManualUpdate(){
        }
    }
}