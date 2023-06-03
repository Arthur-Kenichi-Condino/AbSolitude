#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActorAnimatorController{
        internal void UpdateVanilmirthAIAnimatorWeaponLayer(BaseAI baseAI,VanilmirthAI vanilmirthAI){
        }
        internal void UpdateVanilmirthAIAnimatorMotionValue(BaseAI baseAI,VanilmirthAI vanilmirthAI){
               animator.SetBool("MOTION_STAND",vanilmirthAI.motion==BaseAI.ActorMotion.MOTION_STAND);
               animator.SetBool("MOTION_MOVE" ,vanilmirthAI.motion==BaseAI.ActorMotion.MOTION_MOVE );
                animator.SetFloat("MOTION_MOVE_VELOCITY",vanilmirthAI.moveVelocityFlattened);
                 animator.SetFloat("MOTION_MOVE_TURN",vanilmirthAI.turnAngle/180f);
        }
    }
}