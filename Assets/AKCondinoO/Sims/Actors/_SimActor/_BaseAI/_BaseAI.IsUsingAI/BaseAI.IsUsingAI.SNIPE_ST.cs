#if UNITY_EDITOR
#define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal partial class AI{
         internal SNIPE_ST snipeSt;
            internal class SNIPE_ST:ST{
                internal SNIPE_ST(BaseAI me,AI ai):base(me,ai){
                }
             internal float minTimeBeforeCanChase=8f;
             internal float time;
                internal void Finish(){
                 if(me.characterController!=null){
                    me.characterController.isAiming=false;
                 }
                }
                internal void Start(){
                }
                internal void DoRoutine(){
                }
            }
        }
     //internal float onSnipeRetreatTime=3f;
     //internal float onSnipeRetreatDis;
     //protected bool onSnipeAlternateRetreatShoot=false;
     //   protected virtual void OnSNIPE_ST_Start(){
     //    Log.DebugMessage("OnSNIPE_ST_Start()");
     //    onSnipeTime=0f;
     //    onSnipeAlternateRetreatShoot=false;
     //    if(
     //     IsTraversingPath()
     //    ){
     //     MoveStop();
     //    }
     //    onSnipeMoving=false;
     //    onSnipeTryingShooting=false;
     //    onSnipeReloading=false;
     //    onSnipeShooting=false;
     //    onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.tgtRot=characterController.character.transform.rotation;
     //    onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.EndRotation();
     //   }
     //[SerializeField]internal QuaternionRotLerpHelper onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy=new QuaternionRotLerpHelper(10,1.5f);
     //bool onSnipeMoving;
     //bool onSnipeTryingShooting;
     //bool onSnipeReloading;
     //bool onSnipeShooting;
     //   protected virtual void OnSNIPE_ST_Routine(Vector3 attackDistance,Vector3 attackDistanceWithWeapon){
     //    onSnipeTime+=Time.deltaTime;
     //    if(MyEnemy==null){
     //     return;
     //    }
     //    //Log.DebugMessage("OnSNIPE_ST_Routine()");
     //    float myMoveSpeed=Mathf.Max(moveMaxVelocity.x,moveMaxVelocity.y,moveMaxVelocity.z);
     //    float myEnemyMoveSpeed=0f;
     //    if(MyEnemy is BaseAI myEnemyAI){
     //     myEnemyMoveSpeed=Mathf.Max(myEnemyAI.moveMaxVelocity.x,myEnemyAI.moveMaxVelocity.y,myEnemyAI.moveMaxVelocity.z);
     //    }
     //    float dis1=     myMoveSpeed*onSnipeRetreatTime;
     //    float dis2=myEnemyMoveSpeed*onSnipeRetreatTime;
     //    float ratio;
     //    if(dis2<=0f){
     //     ratio=1f;
     //    }else{
     //     ratio=dis1/dis2;
     //    }
     //    onSnipeRetreatDis=ratio*dis1;
     //    //Log.DebugMessage("onSnipeRetreatDis:"+onSnipeRetreatDis);
     //    //
     //    if(onSnipeReloading){
     //     //Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeReloading");
     //     if(
     //      IsTraversingPath()
     //     ){
     //      MoveStop();
     //     }
     //     if(!IsReloading()){
     //      //OnSNIPE_ST_Reset();
     //      onSnipeAlternateRetreatShoot=false;
     //      onSnipeReloading=false;
     //     }else{
     //      AI_LookToMyEnemy(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy);
     //     }
     //    }else if(onSnipeShooting){
     //     Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeShooting");
     //     if(
     //      IsTraversingPath()
     //     ){
     //      MoveStop();
     //     }
     //     if(!IsShooting()){
     //      //OnSNIPE_ST_Reset(); 
     //      onSnipeAlternateRetreatShoot=false;
     //      onSnipeShooting=false;
     //     }else{
     //      AI_LookToMyEnemy(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy);
     //     }
     //    }else if(onSnipeMoving){
     //     Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeMoving");
     //     if(
     //      !IsTraversingPath()
     //     ){
     //      onSnipeAlternateRetreatShoot=true;
     //      onSnipeMoving=false;
     //     }else{
     //      if(!AI_LookToMyDest(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy)){
     //       MovePause();
     //      }else{
     //       MoveResume();
     //      }
     //     }
     //    }
     //    //Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeAlternateRetreatShoot:"+onSnipeAlternateRetreatShoot);
     //    if(!onSnipeReloading&&
     //       !onSnipeShooting&&
     //       !onSnipeMoving
     //    ){
     //     if(onSnipeAlternateRetreatShoot){
     //      Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeAlternateRetreatShoot==true");
     //      onSnipeTryingShooting=true;
     //     }
     //    }
     //    if(onSnipeTryingShooting){
     //     Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeTryingShooting");
     //     if(
     //      !IsTraversingPath()
     //     ){
     //      if(!onSnipeReloading&&
     //         !onSnipeShooting
     //      ){
     //       if(OnSNIPE_ST_Shoot()){
     //        Log.DebugMessage("OnSNIPE_ST_Routine():shoot");
     //        onSnipeTryingShooting=false;
     //       }
     //      }
     //     }else{
     //      if(!AI_LookToMyDest(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy)){
     //       MovePause();
     //      }else{
     //       MoveResume();
     //      }
     //     }
     //    }else{
     //     if(!onSnipeAlternateRetreatShoot){
     //      Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeAlternateRetreatShoot==false");
     //      if(characterController!=null){
     //         characterController.isAiming=false;
     //      }
     //      bool moveToDestination=!onSnipeMoving;
     //      if(Vector3.Distance(transform.position,MyEnemy.transform.position)<=onSnipeRetreatDis){
     //       if(
     //        !IsTraversingPath()
     //       ){
     //        moveToDestination|=true;
     //       }
     //      }
     //      if(moveToDestination){
     //       Log.DebugMessage("OnSNIPE_ST_Routine():move");
     //       Vector3 dir=(transform.position-MyEnemy.transform.position).normalized;
     //       dir.y=0f;
     //       MyDest=MyEnemy.transform.position+dir*onSnipeRetreatDis+Vector3.down*(height/2f);
     //       if(AI_LookToMyDest(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy)){
     //        Move(MyDest);
     //        Debug.DrawRay(MyEnemy.transform.position,dir*onSnipeRetreatDis,Color.blue,5f);
     //        onSnipeMoving=true;
     //       }
     //      }
     //     }
     //    }
     //    if(animatorController.animatorIKController!=null){
     //     Debug.DrawLine(GetHeadPosition(true),animatorController.animatorIKController.headLookAtPositionLerp.tgtPos,Color.yellow);
     //    }
     //   }
     //   protected virtual bool OnSNIPE_ST_Shoot(){
     //    if(AI_LookToMyEnemy(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy)){
     //     //
     //     characterController.isAiming=true;
     //     if(animatorController.animatorIKController==null||(Vector3.Angle(animatorController.animatorIKController.headLookAtPositionLerped,animatorController.animatorIKController.headLookAtPositionLerp.tgtPos)<=.125f&&animatorController.animatorIKController.headLookAtPositionLerp.tgtPos==animatorController.animatorIKController.headLookAtPositionLerp.tgtPos_Last&&animatorController.animatorIKController.headLookAtPositionLerp.tgtPosLerpVal>=1f)){
     //      if(itemsEquipped!=null){
     //       if(itemsEquipped.Value.forAction1 is SimWeapon simWeapon){
     //        if(simWeapon.ammoLoaded<=0){
     //         if(simWeapon.TryStartReloadingAction(simAiming:this)){
     //          characterController.weaponsReloading.Add(simWeapon);
     //          onSnipeReloading=true;
     //          return true;
     //         }
     //        }else{
     //         if(simWeapon.TryStartShootingAction(simAiming:this)){
     //          onSnipeShooting=true;
     //          Debug.DrawLine(GetHeadPosition(true),animatorController.animatorIKController.headLookAtPositionLerped,Color.blue,5f);
     //          return true;
     //         }
     //        }
     //       }
     //      }
     //     }
     //    }
     //    return false;
     //   }
    }
}