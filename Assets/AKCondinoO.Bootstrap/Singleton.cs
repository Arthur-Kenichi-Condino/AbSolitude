using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal static class SingletonManager{
     private static readonly List<ISingleton>singletons=new();
        internal static void Register(ISingleton singleton){
         if(singleton==null||singletons.Contains(singleton)){
          Log.Message(Log.LogType.Error,$"duplicate singleton:{singleton}");
          return;
         }
         if(singletons.Any(s=>s.initOrder==singleton.initOrder)){
          Log.Message(Log.LogType.Error,$"duplicate initOrder:{singleton.initOrder}");
          return;
         }
         singletons.Add(singleton);
        }
        internal static void Unregister(ISingleton singleton){
         singletons.Remove(singleton);
        }
        internal static void InitializeAll(){
         singletons.Sort((a,b)=>a.initOrder.CompareTo(b.initOrder));
         foreach(var s in singletons)s.Initialize();
        }
        internal static void ShutdownAll(){
         for(int i=singletons.Count-1;i>=0;i--)singletons[i].Shutdown();
        }
    }
    internal interface ISingleton{
     int initOrder{get;}
        void Initialize();
        void Shutdown();
    }
    internal abstract class Singleton<T>:MonoBehaviour,ISingleton where T:MonoBehaviour{
     public static T singleton{get;protected set;}
     public abstract int initOrder{get;}
        protected virtual void Awake(){
         if(singleton!=null&&singleton!=this){
          Destroy(gameObject);
          return;
         }
         singleton=this as T;
         DontDestroyOnLoad(gameObject);
         SingletonManager.Register(this);
        }
        protected virtual void OnDestroy(){
         if(singleton==this){
          SingletonManager.Unregister(this);
          singleton=null;
         }
        }
        public virtual void Initialize(){
        }
        public virtual void Shutdown(){
        }
    }
}