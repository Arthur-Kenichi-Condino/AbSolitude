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
     [NonSerialized]protected Vector3 moveDest;
        internal bool Move(Vector3 dest,bool run=false){
         if(ai==null){
          return true;
         }
         //if(
         // !IsTraversingPath()
         //){
         // GetAStarPath(dest);
         //}
         moveDest=dest;
         //if(TurnToMoveDest()){
         // if(run){
         //  navMeshAgent.speed=navMeshAgentRunSpeed;
         // }else{
           navMeshAgent.speed=navMeshAgentWalkSpeed;
         // }
          navMeshAgent.destination=moveDest;
         // if(
         //  IsTraversingPath()
         // ){
           return true;
         // }
         //}
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
          //moveDest=navMeshAgent.transform.position;
          if(Vector3.Distance(navMeshAgent.destination,navMeshAgent.transform.position)>navMeshAgent.stoppingDistance){
           //navMeshAgent.destination=moveDest;
          }
         }
        }
     [NonSerialized]internal bool movePaused;
      [NonSerialized]internal float movePauseInterval=1f;
       [NonSerialized]internal float movePauseDelay;
        internal void MovePause(){
         if(ai==null){
          return;
         }
         if(movePaused){
          return;
         }
         //movePauseDelay=movePauseInterval;
         //movePaused=true;
        }
        internal void MoveResume(){
         if(ai==null){
          return;
         }
         if(movePaused){
          movePaused=false;
         }
        }
        internal bool Attack(SimObject enemy){
         bool result=false;
         if(result=TurnToMyEnemy()){
          DoAttackOnAnimationEvent();
         }
         return result;
        }
     [NonSerialized]RaycastHit[]tryShootHits=new RaycastHit[4];
        internal bool TryShoot(SimObject enemy,out bool reloading,out bool shooting){
         reloading=false;
         shooting=false;
         if(ai.MyEnemy==null){
          return false;
         }
         if(IsReloading()||IsShooting()){
          return false;
         }
         bool result=false;
         if(TurnToMyEnemy()){
          characterController.isAiming=true;
          if(animatorController.animatorIKController==null||Vector3.Angle(animatorController.animatorIKController.headLookAtPositionLerped,animatorController.animatorIKController.headLookAtPositionLerp.tgtPos)<=(.0625f)){
           if(itemsEquipped!=null){
            if(itemsEquipped.Value.forAction1 is SimWeapon simWeapon){
             if(simWeapon.ammoLoaded<=0){
              if(simWeapon.TryStartReloadingAction(simAiming:this)){
               characterController.weaponsReloading.Add(simWeapon);
               reloading=true;
               result=true;
              }
             }else{
              simWeapon.OnShootGetHits(this,ref tryShootHits,out int shootHitsLength);
              if(shootHitsLength>0){
               //Log.DebugMessage("'shootHitsLength>0'");
               for(int h=0;h<shootHitsLength;h++){
                //Log.DebugMessage("tryShootHits["+h+"].transform.name:"+tryShootHits[h].transform.name);
                if(tryShootHits[h].transform.root==ai.MyEnemy.transform.root){
                 if(simWeapon.TryStartShootingAction(simAiming:this)){
                  Vector3 head=GetHeadPosition(true);
                  shooting=true;
                  result=true;
                 }
                 break;
                }else{
                 BaseAI baseAI;
                 if((baseAI=tryShootHits[h].collider.transform.root.GetComponentInChildren<BaseAI>())!=null){
                  if(baseAI.IsFriendlyTo(this)){
                   break;
                  }
                 }
                }
               }
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