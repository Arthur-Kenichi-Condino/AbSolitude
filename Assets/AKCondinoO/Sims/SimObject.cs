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
             return string.Format(CultureInfoUtil.en_US,"persistentData={{ position={0}, rotation={1}, localScale={2}, }}",position,rotation,localScale);
            }
            internal static PersistentData Parse(string s){
             PersistentData persistentData=new PersistentData();
             int positionStringStart=s.IndexOf("position=(");
             if(positionStringStart>=0){
                positionStringStart+=10;
              int positionStringEnd=s.IndexOf("), ",positionStringStart);
              string positionString=s.Substring(positionStringStart,positionStringEnd-positionStringStart);
              string[]xyzString=positionString.Split(',');
              float x=float.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float y=float.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float z=float.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              persistentData.position=new Vector3(x,y,z);
             }
             int rotationStringStart=s.IndexOf("rotation=(");
             if(rotationStringStart>=0){
             }
             return persistentData;
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
     internal bool interactionsEnabled{get;private set;}
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