#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal class WindZoneControl:MonoBehaviour,ISingletonInitialization{
     internal static WindZoneControl singleton{get;set;}
     internal WindZone wind;
     [SerializeField]internal float windChangesTimeInterval=5f;
     [SerializeField]internal float directionChangeSpeed=10f;
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         wind=gameObject.GetComponent<WindZone>();
        }
        public void Init(){
         changesRoutine=StartCoroutine(ChangesRoutine());
         startChangesRoutine=StartCoroutine(StartChangesRoutine());
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("WindZoneControl:OnDestroyingCoreEvent");
         if(startChangesRoutine!=null){
          StopCoroutine(startChangesRoutine);
          startChangesRoutine=null;
         }
         if(changesRoutine!=null){
          StopCoroutine(changesRoutine);
          changesRoutine=null;
         }
        }
     Coroutine startChangesRoutine;
     Quaternion initRotation;
     Quaternion goalRotation;
        IEnumerator StartChangesRoutine(){
         float windChangesTimer=0f;
         WaitUntil waitForTimeInterval=new WaitUntil(
          ()=>{
           windChangesTimer+=Time.deltaTime;
           if(windChangesTimer>=windChangesTimeInterval){
            windChangesTimer=0f;
            return true;
           }
           return false;
          }
         );
         Loop:{
          yield return waitForTimeInterval;
          Log.DebugMessage("StartChangesRoutine Loop");
          initRotation=transform.rotation;
          Vector3 goalDirection=UnityEngine.Random.insideUnitSphere;
          goalDirection.y=0f;
          goalDirection.Normalize();
          goalRotation=Quaternion.LookRotation(goalDirection);
          startChangesFlag=true;
         }
         goto Loop;
        }
     Coroutine changesRoutine;
      bool startChangesFlag;
        IEnumerator ChangesRoutine(){
         WaitUntil waitForStartChangesFlag=new WaitUntil(
          ()=>{
           if(startChangesFlag){
            startChangesFlag=false;
            return true;
           }
           return false;
          }
         );
         Loop:{
          yield return waitForStartChangesFlag;
          Log.DebugMessage("ChangesRoutine Loop");
          while(transform.rotation!=goalRotation){
           transform.rotation=Quaternion.RotateTowards(transform.rotation,goalRotation,directionChangeSpeed*Time.deltaTime);
           yield return null;
          }
         }
         goto Loop;
        }
    }
}