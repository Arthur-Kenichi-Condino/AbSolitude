using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal static class SingletonManager{
     private static readonly List<ISingleton>singletons=new();
        internal static void Register(ISingleton singleton){
         if(singleton==null||singletons.Contains(singleton)){
          Logs.Message(Logs.LogType.Error,$"invalid or duplicate singleton:{singleton}");
          return;
         }
         if(singletons.Any(s=>s.initOrder==singleton.initOrder)){
          Logs.Message(Logs.LogType.Error,$"invalid or duplicate initOrder:{singleton.initOrder}");
          return;
         }
         singletons.Add(singleton);
         Logs.Message(Logs.LogType.Debug,"'added singleton':"+singleton+$";singletons.Count:{singletons.Count}");
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
          Logs.Message(Logs.LogType.Debug,"'init singleton':"+s);
          s.Initialize();
         }
        }
        internal static void PreShutdownAll(){
         for(int i=singletons.Count-1;i>=0;i--){
          var s=singletons[i];
          s.PreShutdown();
         }
        }
        internal static void ShutdownAll(){
         for(int i=singletons.Count-1;i>=0;i--){
          var s=singletons[i];
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
     public abstract int initOrder{get;}
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