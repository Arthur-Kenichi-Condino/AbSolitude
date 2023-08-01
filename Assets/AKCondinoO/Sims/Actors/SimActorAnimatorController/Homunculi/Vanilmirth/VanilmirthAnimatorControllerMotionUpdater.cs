#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class VanilmirthAnimatorControllerMotionUpdater:HomunculiAnimatorControllerMotionUpdater{
        internal override void UpdateAnimatorWeaponLayer(){
        }
        internal override void UpdateAnimatorMotionValue(){
         if(controller.actor is VanilmirthAI vanilmirthAI){
               controller.animator.SetBool("MOTION_STAND",vanilmirthAI.motion==BaseAI.ActorMotion.MOTION_STAND);
               controller.animator.SetBool("MOTION_MOVE" ,vanilmirthAI.motion==BaseAI.ActorMotion.MOTION_MOVE );
                controller.animator.SetFloat("MOTION_MOVE_VELOCITY"       ,vanilmirthAI.      moveVelocityFlattened);
                controller.animator.SetFloat("MOTION_MOVE_VELOCITY_STRAFE",vanilmirthAI.moveStrafeVelocityFlattened);
                 controller.animator.SetFloat("MOTION_MOVE_TURN",vanilmirthAI.turnAngle);
         }
        }
    }
}