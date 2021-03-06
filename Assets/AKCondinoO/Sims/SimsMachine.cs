#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.ArthurCondino;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SimsMachine:MonoBehaviour{
     internal static SimsMachine singleton;
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        internal void Init(){
         Core.singleton.OnDestroyingCoreEvent+=OnDestroyingCoreEvent;
        }
        void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimsMachine:OnDestroyingCoreEvent");
        }
     readonly(Type simType,ulong number)idArthurCondino=(typeof(ArthurCondinoAI),0);
     float specificSpawnRequestsDelay=5f;
     float specificSpawnRequestsCooldown=5f;
        void Update(){
         if(specificSpawnRequestsCooldown>0f){
            specificSpawnRequestsCooldown-=Time.deltaTime;
         }
         if(specificSpawnRequestsCooldown<=0f){
            specificSpawnRequestsCooldown=specificSpawnRequestsDelay;
          if(!SimObjectManager.singleton.active.ContainsKey(idArthurCondino)){
           Log.DebugMessage("SimsMachine:call to current location:idArthurCondino:"+idArthurCondino);
           SimObjectSpawner.singleton.OnSpecificSpawnRequestAt(idArthurCondino,MainCamera.singleton.transform.position,Vector3.zero,Vector3.one);
          }
         }
        }
    }
}