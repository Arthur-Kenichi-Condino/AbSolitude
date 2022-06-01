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
                rotationStringStart+=10;
              int rotationStringEnd=s.IndexOf("), ",rotationStringStart);
              string rotationString=s.Substring(rotationStringStart,rotationStringEnd-rotationStringStart);
              string[]xyzwString=rotationString.Split(',');
              float x=float.Parse(xyzwString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float y=float.Parse(xyzwString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float z=float.Parse(xyzwString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float w=float.Parse(xyzwString[3].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              persistentData.rotation=new Quaternion(x,y,z,w);
             }
             int localScaleStringStart=s.IndexOf("localScale=(");
             if(localScaleStringStart>=0){
                localScaleStringStart+=12;
              int localScaleStringEnd=s.IndexOf("), ",localScaleStringStart);
              string localScaleString=s.Substring(localScaleStringStart,localScaleStringEnd-localScaleStringStart);
              string[]xyzString=localScaleString.Split(',');
              float x=float.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float y=float.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float z=float.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              persistentData.localScale=new Vector3(x,y,z);
             }
             if(persistentData.localScale.x<=0f||
                persistentData.localScale.y<=0f||
                persistentData.localScale.z<=0f){
              persistentData.localScale=Vector3.one;
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
     [NonSerialized]bool checkIfOutOfSight;
     [NonSerialized]bool poolRequested;
        internal virtual void ManualUpdate(bool doValidationChecks){
         checkIfOutOfSight|=doValidationChecks;
         if(transform.hasChanged){
          persistentData.UpdateData(this);
          checkIfOutOfSight|=transform.hasChanged;
            transform.hasChanged=false;
         }
         if(unplaceRequested){
            unplaceRequested=false;
             DisableInteractions();
             SimObjectManager.singleton.DeactivateAndReleaseIdQueue.Enqueue(this);
         }else{
             if(checkIfOutOfSight){
             }else{
                 if(poolRequested){
                    poolRequested=false;
                     DisableInteractions();
                     SimObjectManager.singleton.DeactivateQueue.Enqueue(this);
                 }
             }
         }
        }
        protected virtual bool IsOutOfSight(){
         return false;
        }
        protected virtual void OnDrawGizmos(){
        #if UNITY_EDITOR
        #endif
        }
    }
}