#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Humanoid.Human;
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class HumanAnimatorControllerMotionUpdater:HumanoidAnimatorControllerMotionUpdater{
        internal override void UpdateAnimatorWeaponLayer(){
            if(controller.lastWeaponType!=controller.actor.weaponType){
             controller.currentWeaponLayerIndex=null;
             if(controller.currentWeaponAimLayerIndex!=null){
              controller.layerTargetWeight[controller.currentWeaponAimLayerIndex.Value]=0.0f;
              controller.currentWeaponAimLayerIndex=null;
             }
             if(controller.weaponLayer.TryGetValue(controller.actor.weaponType,out int layerIndex)){
              controller.currentWeaponLayerIndex=layerIndex;
              controller.layerTargetWeight[layerIndex]=1.0f;
              if(controller.weaponAimLayer.TryGetValue(controller.actor.weaponType,out int aimLayerIndex)&&aimLayerIndex!=layerIndex){
               controller.currentWeaponAimLayerIndex=aimLayerIndex;
              }
             }
             if(controller.weaponLayer.TryGetValue(controller.lastWeaponType,out int lastLayerIndex)){
              controller.layerTargetWeight[lastLayerIndex]=0.0f;
             }
             controller.lastWeaponType=controller.actor.weaponType;
            }
        }
        internal override void UpdateAnimatorMotionValue(){
         if(controller.actor is HumanAI humanAI){
             if(controller.actor.weaponType==BaseAI.WeaponTypes.SniperRifle){
              controller.animator.SetBool("MOTION_STAND_RIFLE",humanAI.motion==BaseAI.ActorMotion.MOTION_STAND_RIFLE);
              controller.animator.SetBool("MOTION_MOVE_RIFLE" ,humanAI.motion==BaseAI.ActorMotion.MOTION_MOVE_RIFLE );
               controller.animator.SetFloat("MOTION_MOVE_RIFLE_VELOCITY"       ,humanAI.      moveVelocityFlattened);
               controller.animator.SetFloat("MOTION_MOVE_RIFLE_VELOCITY_STRAFE",humanAI.moveStrafeVelocityFlattened);
                controller.animator.SetFloat("MOTION_MOVE_RIFLE_TURN",humanAI.turnAngle);
              controller.animator.SetBool("MOTION_HIT_RIFLE"  ,humanAI.motion==BaseAI.ActorMotion.MOTION_HIT_RIFLE  );
             }else{
              controller.animator.SetBool("MOTION_STAND",humanAI.motion==BaseAI.ActorMotion.MOTION_STAND);
              controller.animator.SetBool("MOTION_MOVE" ,humanAI.motion==BaseAI.ActorMotion.MOTION_MOVE );
               controller.animator.SetFloat("MOTION_MOVE_VELOCITY"       ,humanAI.      moveVelocityFlattened);
               controller.animator.SetFloat("MOTION_MOVE_VELOCITY_STRAFE",humanAI.moveStrafeVelocityFlattened);
                controller.animator.SetFloat("MOTION_MOVE_TURN",humanAI.turnAngle);
              controller.animator.SetBool("MOTION_HIT"  ,humanAI.motion==BaseAI.ActorMotion.MOTION_HIT  );
             }
             bool canDoShootingAnimation=false;
             if(controller.currentWeaponAimLayerIndex!=null){
              if(controller.actor.isAiming){
               controller.layerTargetWeight[controller.currentWeaponAimLayerIndex.Value]=1.0f;
               canDoShootingAnimation=true;
              }else{
               controller.layerTargetWeight[controller.currentWeaponAimLayerIndex.Value]=0.0f;
              }
             }
             controller.animator.SetBool("MOTION_STAND_FIRING_RIFLE",canDoShootingAnimation&&humanAI.isShooting);
         }
        }
    }
}