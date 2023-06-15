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
    internal partial class SimActorAnimatorController{
        internal void UpdateDisfiguringHomunculusAIAnimatorWeaponLayer(BaseAI baseAI,DisfiguringHomunculusAI disfiguringHomunculusAI){
        }
        internal void UpdateDisfiguringHomunculusAIAnimatorMotionValue(BaseAI baseAI,DisfiguringHomunculusAI disfiguringHomunculusAI){
               animator.SetBool("MOTION_STAND",disfiguringHomunculusAI.motion==BaseAI.ActorMotion.MOTION_STAND);
               animator.SetBool("MOTION_MOVE" ,disfiguringHomunculusAI.motion==BaseAI.ActorMotion.MOTION_MOVE );
                animator.SetFloat("MOTION_MOVE_VELOCITY",disfiguringHomunculusAI.moveVelocityFlattened);
                 animator.SetFloat("MOTION_MOVE_TURN",disfiguringHomunculusAI.turnAngle/180f);
        }
    }
}