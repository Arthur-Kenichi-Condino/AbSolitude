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
     [SerializeField]internal float windDirectionChangesTimeInterval=5f;
      [SerializeField]internal float directionChangeSpeed=10f;
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         wind=gameObject.GetComponent<WindZone>();
        }
        public void Init(){
         directionChangesRoutine=StartCoroutine(DirectionChangesRoutine());
         startDirectionChangesRoutine=StartCoroutine(StartDirectionChangesRoutine());
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         //Log.DebugMessage("WindZoneControl:OnDestroyingCoreEvent");
         if(this!=null){
          if(startDirectionChangesRoutine!=null){
           StopCoroutine(startDirectionChangesRoutine);
           startDirectionChangesRoutine=null;
          }
          if(directionChangesRoutine!=null){
           StopCoroutine(directionChangesRoutine);
           directionChangesRoutine=null;
          }
         }
        }
     Coroutine startDirectionChangesRoutine;
      Quaternion initRotation;
      Quaternion goalRotation;
        IEnumerator StartDirectionChangesRoutine(){
         float windChangesTimer=0f;
         WaitUntil waitForTimeInterval=new WaitUntil(
          ()=>{
           windChangesTimer+=Time.deltaTime;
           if(windChangesTimer>=windDirectionChangesTimeInterval){
            windChangesTimer=0f;
            return true;
           }
           return false;
          }
         );
         Loop:{
          yield return waitForTimeInterval;
          //Log.DebugMessage("StartDirectionChangesRoutine Loop");
          initRotation=transform.rotation;
          Vector3 goalDirection=UnityEngine.Random.insideUnitSphere;
          goalDirection.y=0f;
          goalDirection.Normalize();
          goalRotation=Quaternion.LookRotation(goalDirection);
          startDirectionChangesFlag=true;
         }
         goto Loop;
        }
     Coroutine directionChangesRoutine;
      bool startDirectionChangesFlag;
        IEnumerator DirectionChangesRoutine(){
         WaitUntil waitForStartChangesFlag=new WaitUntil(
          ()=>{
           if(startDirectionChangesFlag){
            startDirectionChangesFlag=false;
            return true;
           }
           return false;
          }
         );
         Loop:{
          yield return waitForStartChangesFlag;
          //Log.DebugMessage("DirectionChangesRoutine Loop");
          while(transform.rotation!=goalRotation){
           transform.rotation=Quaternion.RotateTowards(transform.rotation,goalRotation,directionChangeSpeed*Time.deltaTime);
           yield return null;
          }
         }
         goto Loop;
        }
    }
}