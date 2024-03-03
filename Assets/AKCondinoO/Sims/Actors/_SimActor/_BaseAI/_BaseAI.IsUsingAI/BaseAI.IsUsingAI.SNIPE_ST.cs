#if UNITY_EDITOR
#define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        protected virtual void OnSNIPE_ST_Reset(){
         if(characterController!=null){
            characterController.isAiming=false;
         }
         ResetRotation(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy);
        }
     internal float onSnipeRetreatTime=3f;
     internal float onSnipeRetreatDis;
     protected bool onSnipeAlternateRetreatShoot=false;
        protected virtual void OnSNIPE_ST_Start(){
         onSnipeAlternateRetreatShoot=false;
         if(
          IsTraversingPath()
         ){
          navMeshAgent.destination=navMeshAgent.transform.position;
         }
        }
     [SerializeField]internal QuaternionRotLerpHelper onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy=new QuaternionRotLerpHelper(10,.5f);
     bool onSnipeMoving;
     bool onSnipeReloading;
     bool onSnipeShooting;
        protected virtual void OnSNIPE_ST_Routine(Vector3 attackDistance,Vector3 attackDistanceWithWeapon){
         if(MyEnemy==null){
          return;
         }
         Log.DebugMessage("OnSNIPE_ST_Routine()");
         float myMoveSpeed=Mathf.Max(moveMaxVelocity.x,moveMaxVelocity.y,moveMaxVelocity.z);
         float myEnemyMoveSpeed=0f;
         if(MyEnemy is BaseAI myEnemyAI){
          myEnemyMoveSpeed=Mathf.Max(myEnemyAI.moveMaxVelocity.x,myEnemyAI.moveMaxVelocity.y,myEnemyAI.moveMaxVelocity.z);
         }
         float dis1=     myMoveSpeed*onSnipeRetreatTime;
         float dis2=myEnemyMoveSpeed*onSnipeRetreatTime;
         float ratio;
         if(dis2<=0f){
          ratio=1f;
         }else{
          ratio=dis1/dis2;
         }
         onSnipeRetreatDis=ratio*dis1;
         Log.DebugMessage("onSnipeRetreatDis:"+onSnipeRetreatDis);
         //
         bool alternateRoutineAction=false;
         bool moveToDestination=false;
         if(
          !IsTraversingPath()
         ){
          if(onSnipeReloading){
           if(!IsReloading()){
            OnSNIPE_ST_Reset();
            alternateRoutineAction=true;
            onSnipeReloading=false;
           }
          }else if(onSnipeShooting){
           if(!IsShooting()){
            OnSNIPE_ST_Reset();
            alternateRoutineAction=true;
            onSnipeShooting=false;
           }
          }else if(onSnipeMoving){
           alternateRoutineAction=true;
           onSnipeMoving=false;
          }else if(Vector3.Distance(transform.position,MyEnemy.transform.position)<=onSnipeRetreatDis){
           moveToDestination|=true;
          }else{
           onSnipeAlternateRetreatShoot=true;
          }
         }
         if(alternateRoutineAction){
          onSnipeAlternateRetreatShoot=!onSnipeAlternateRetreatShoot;
          alternateRoutineAction=false;
         }
         if(!onSnipeAlternateRetreatShoot){
          Log.DebugMessage("OnSNIPE_ST_Routine():move");
          if(characterController!=null){
             characterController.isAiming=false;
          }
          if(moveToDestination){
           Vector3 dir=(transform.position-MyEnemy.transform.position).normalized;
           MyDest=MyEnemy.transform.position+dir*onSnipeRetreatDis;
           navMeshAgent.destination=MyDest;
           Debug.DrawRay(MyEnemy.transform.position,dir*onSnipeRetreatDis,Color.yellow,5f);
           onSnipeMoving=true;
          }
         }
         if(onSnipeAlternateRetreatShoot){
          Log.DebugMessage("OnSNIPE_ST_Routine():shoot");
          if(
           !IsTraversingPath()
          ){
           if(!onSnipeReloading&&
              !onSnipeShooting
           ){
            if(OnSNIPE_ST_Shoot()){
            }
           }
          }
         }
        }
        protected virtual bool OnSNIPE_ST_Shoot(){
         if(LookToMyEnemy()){
          //
          characterController.isAiming=true;
          if(animatorController.animatorIKController==null||Vector3.Angle(animatorController.animatorIKController.headLookAtPositionLerp.tgtPos,animatorController.animatorIKController.headLookAtPositionLerped)<=1f){
           if(itemsEquipped!=null){
            if(itemsEquipped.Value.forAction1 is SimWeapon simWeapon){
             if(simWeapon.ammoLoaded<=0){
              if(simWeapon.TryStartReloadingAction(simAiming:this)){
               characterController.weaponsReloading.Add(simWeapon);
               onSnipeReloading=true;
               return true;
              }
             }else{
              if(simWeapon.TryStartShootingAction(simAiming:this)){
               onSnipeShooting=true;
               return true;
              }
             }
            }
           }
          }
         }
         return false;
        }
    }
}