#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human;
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActorAnimatorController{
        internal void UpdateHumanAIAnimatorWeaponLayer(BaseAI baseAI,HumanAI humanAI){
              if(lastWeaponType!=baseAI.weaponType){
               if(weaponLayer.TryGetValue(baseAI.weaponType,out int layerIndex)){
                layerTargetWeight[layerIndex]=1.0f;
                if(weaponLayer.TryGetValue(lastWeaponType,out int lastLayerIndex)){
                 layerTargetWeight[lastLayerIndex]=0.0f;
                }
                lastWeaponType=baseAI.weaponType;
               }
              }
        }
        internal void UpdateHumanAIAnimatorMotionValue(BaseAI baseAI,HumanAI humanAI){
              if(baseAI.weaponType==SimActor.WeaponTypes.SniperRifle){
               animator.SetBool("MOTION_RIFLE_STAND",humanAI.motion==BaseAI.ActorMotion.MOTION_RIFLE_STAND);
               animator.SetBool("MOTION_RIFLE_MOVE" ,humanAI.motion==BaseAI.ActorMotion.MOTION_RIFLE_MOVE );
                animator.SetFloat("MOTION_RIFLE_MOVE_VELOCITY",humanAI.moveVelocityFlattened);
                 animator.SetFloat("MOTION_RIFLE_MOVE_TURN",humanAI.turnAngle/180f);
              }else{
               animator.SetBool("MOTION_STAND",humanAI.motion==BaseAI.ActorMotion.MOTION_STAND);
               animator.SetBool("MOTION_MOVE" ,humanAI.motion==BaseAI.ActorMotion.MOTION_MOVE );
                animator.SetFloat("MOTION_MOVE_VELOCITY",humanAI.moveVelocityFlattened);
                 animator.SetFloat("MOTION_MOVE_TURN",humanAI.turnAngle/180f);
              }
        }
    }
}