using AKCondinoO.Sims.Actors.ArthurCondino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class SimActorAnimatorController:MonoBehaviour{
     internal SimActor actor;
     internal Animator animator;
        void Awake(){
         animator=GetComponentInChildren<Animator>();
        }
        void Update(){
         if(actor is BaseAI baseAI){
             if(baseAI is ArthurCondinoAI arthurCondinoAI){
              animator.SetBool("MOTION_STAND",arthurCondinoAI.motion==BaseAI.ActorMotion.MOTION_STAND);
              animator.SetBool("MOTION_MOVE" ,arthurCondinoAI.motion==BaseAI.ActorMotion.MOTION_MOVE );
               animator.SetFloat("MOTION_MOVE_VELOCITY",arthurCondinoAI.moveVelocity);
                animator.SetFloat("MOTION_MOVE_TURN",arthurCondinoAI.turnAngle/180f);
             }
         }
        }
    }
}