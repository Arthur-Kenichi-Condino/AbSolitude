#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SimObject:MonoBehaviour{
     static readonly CultureInfo en=CultureInfo.GetCultureInfo("en");
     internal PersistentData persistentData;
        internal struct PersistentData{
         public Quaternion rotation;
         public Vector3    position;
         public Vector3    localScale;
            internal void UpdateData(SimObject simObject){
             rotation=simObject.transform.rotation;
             position=simObject.transform.position;
             localScale=simObject.transform.localScale;
            }
            public override string ToString(){
             return string.Format(en,"persistentData={{ position={0}, rotation={1}, localScale={2}, }}",position,rotation,localScale);
            }
        }
     internal LinkedListNode<SimObject>pooled; 
     internal(Type simType,ulong number)?id=null;
     internal Collider[]colliders;
     internal readonly List<Collider>volumeColliders=new List<Collider>();
        protected virtual void Awake(){
         foreach(Collider collider in colliders=GetComponentsInChildren<Collider>()){
          if(collider.CompareTag("SimObjectVolume")){
           volumeColliders.Add(collider);
          }
         }
        }
        internal virtual void OnActivated(){
         Log.DebugMessage("OnActivated:id:"+id);
         EnableInteractions();
        }
     internal bool interactionsEnabled;
        void EnableInteractions(){
         interactionsEnabled=true;
        }
        void DisableInteractions(){
         interactionsEnabled=false;
        }
        internal void OnUnplaceRequest(){
         unplaceRequested=true;
        }
        internal void OnPoolRequest(){
         poolRequested=true;
        }
     [NonSerialized]bool unplaceRequested;
     [NonSerialized]bool poolRequested;
        internal virtual void ManualUpdate(){
         if(transform.hasChanged){
          persistentData.UpdateData(this);
            transform.hasChanged=false;
         }
         if(unplaceRequested){
            unplaceRequested=false;
             DisableInteractions();
             SimObjectManager.singleton.DeactivateAndReleaseIdQueue.Enqueue(this);
         }else{
          if(poolRequested){
             poolRequested=false;
              DisableInteractions();
              SimObjectManager.singleton.DeactivateQueue.Enqueue(this);
          }
         }
        }
        #if UNITY_EDITOR
        protected virtual void OnDrawGizmos(){
        }
        #endif
    }
}