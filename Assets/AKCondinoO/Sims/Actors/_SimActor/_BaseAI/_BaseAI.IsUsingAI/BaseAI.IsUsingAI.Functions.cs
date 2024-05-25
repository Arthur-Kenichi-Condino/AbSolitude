#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal partial class AI{
        }
     protected Vector3 moveDest;
        internal bool Move(Vector3 dest){
         if(ai==null){
          return false;
         }
         if(
          !IsTraversingPath()
         ){
          GetAStarPath(dest);
         }
         moveDest=dest;
         if(TurnToMoveDest()){
          navMeshAgent.destination=moveDest;
          if(
           IsTraversingPath()
          ){
           return true;
          }
         }
         return false;
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
        internal bool Attack(SimObject enemy){
         bool result=false;
         if(result=TurnToMyEnemy()){
          DoAttackOnAnimationEvent();
         }
         return result;
        }
        internal bool TryShoot(SimObject enemy,out bool reloading,out bool shooting){
         reloading=false;
         shooting=false;
         bool result=false;
         if(TurnToMyEnemy()){
          characterController.isAiming=true;
          if(animatorController.animatorIKController==null||Vector3.Angle(animatorController.animatorIKController.headLookAtPositionLerped,animatorController.animatorIKController.headLookAtPositionLerp.tgtPos)<=(.125f/2f)){
           if(itemsEquipped!=null){
            if(itemsEquipped.Value.forAction1 is SimWeapon simWeapon){
             if(simWeapon.ammoLoaded<=0){
              if(simWeapon.TryStartReloadingAction(simAiming:this)){
               characterController.weaponsReloading.Add(simWeapon);
               reloading=true;
               result=true;
              }
             }else{
              if(simWeapon.TryStartShootingAction(simAiming:this)){
               Debug.DrawLine(GetHeadPosition(true),animatorController.animatorIKController.headLookAtPositionLerped,Color.blue,5f);
               shooting=true;
               result=true;
              }
             }
            }
           }
          }
         }
         return result;
        }
    }
}