using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.Sims.Actors{
    internal class SimActor:SimObject{
     internal PersistentSimActorData persistentSimActorData;
        internal struct PersistentSimActorData{
        }
     internal NavMeshAgent navMeshAgent;
        protected override void Awake(){
         base.Awake();
         navMeshAgent=GetComponent<NavMeshAgent>();
        }
        internal override void OnLoadingPool(){
         base.OnLoadingPool();
        }
        protected override void EnableInteractions(){
         interactionsEnabled=true;
        }
        protected override void DisableInteractions(){
         interactionsEnabled=false;
        }
        internal override int ManualUpdate(bool doValidationChecks){
         int result=0;
         if((result=base.ManualUpdate(doValidationChecks))!=0){
          DisableNavMeshAgent();
          return result;
         }
         return result;
        }
        void DisableNavMeshAgent(){
        }
    }
}