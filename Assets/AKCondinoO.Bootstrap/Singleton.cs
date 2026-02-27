using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal interface ISingleton{
     int initOrder{get;}
        void Initialize();
        void Shutdown();
    }
    internal abstract class Singleton<T>:MonoBehaviour,ISingleton where T:MonoBehaviour{
     public abstract int initOrder{get;}
     protected virtual void Awake(){
      //  TO DO: register to manager
     }
     public virtual void Initialize(){
     }
     public virtual void Shutdown(){
     }
    }
}