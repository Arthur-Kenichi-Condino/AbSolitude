#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Humanoid;
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class DisfiguringHomunculusAnimatorControllerMotionUpdater:HumanoidAnimatorControllerMotionUpdater{
        internal override void UpdateAnimatorWeaponLayer(){
        }
        internal override void UpdateAnimatorMotionValue(){
         if(controller.actor is DisfiguringHomunculusAI disfiguringHomunculusAI){
             controller.animator.SetBool("MOTION_STAND" ,disfiguringHomunculusAI.motion==BaseAI.ActorMotion.MOTION_STAND );
             controller.animator.SetBool("MOTION_MOVE"  ,disfiguringHomunculusAI.motion==BaseAI.ActorMotion.MOTION_MOVE  );
              controller.animator.SetFloat("MOTION_MOVE_VELOCITY"       ,disfiguringHomunculusAI.      moveVelocityFlattened);
              controller.animator.SetFloat("MOTION_MOVE_VELOCITY_STRAFE",disfiguringHomunculusAI.moveStrafeVelocityFlattened);
               controller.animator.SetFloat("MOTION_MOVE_TURN",disfiguringHomunculusAI.turnAngle);
             controller.animator.SetBool("MOTION_ATTACK",disfiguringHomunculusAI.motion==BaseAI.ActorMotion.MOTION_ATTACK);
         }
        }
    }
}