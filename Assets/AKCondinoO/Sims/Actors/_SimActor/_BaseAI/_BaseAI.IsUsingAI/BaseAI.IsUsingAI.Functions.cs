#if UNITY_EDITOR
#define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal partial class AI{
        }
     protected Vector3 moveDest;
        internal void Move(Vector3 dest){
         if(ai==null){
          return;
         }
         if(
          !IsTraversingPath()
         ){
          GetAStarPath(dest);
         }
         moveDest=dest;
         if(TurnToMoveDest()){
          navMeshAgent.destination=moveDest;
         }
        }
        internal void MoveStop(){
         if(ai==null){
          return;
         }
         movePaused=false;
         if(
          IsTraversingPath()
         ){
          moveDest=navMeshAgent.transform.position;
          navMeshAgent.destination=moveDest;
         }
        }
     internal bool movePaused;
        internal void MovePause(){
         if(ai==null){
          return;
         }
         movePaused=true;
         navMeshAgent.destination=navMeshAgent.transform.position;
        }
        internal void MoveResume(){
         if(ai==null){
          return;
         }
         if(movePaused){
          movePaused=false;
          navMeshAgent.destination=moveDest;
         }
        }
        internal void Attack(SimObject enemy){
         if(TurnToMyEnemy()){
          DoAttackOnAnimationEvent();
         }
        }
    }
}