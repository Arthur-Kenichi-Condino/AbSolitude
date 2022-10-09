#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class SimActorAnimatorController:MonoBehaviour{
     internal SimActor actor;
     internal Animator animator;
     internal SimActorAnimatorIKController animatorIKController;
        void Awake(){
        }
     bool synced=true;
     internal float animationTime=0f;
     BaseAI.ActorMotion lastMotion=BaseAI.ActorMotion.MOTION_STAND;
     readonly List<AnimatorClipInfo>animatorClip=new List<AnimatorClipInfo>();
     string lastClipName="";
     int loopCount=0;//  use integer part of normalizedTime [https://answers.unity.com/questions/1317841/how-to-find-the-normalised-time-of-a-looping-anima.html]
      int lastLoopCount=0;
        void Update(){
         if(animator==null){
          animator=GetComponentInChildren<Animator>();
          if(animator!=null){
           Log.DebugMessage("add SimActorAnimatorIKController");
           animatorIKController=animator.gameObject.AddComponent<SimActorAnimatorIKController>();
           animatorIKController.simActorAnimatorController=this;
          }
         }
         if(animator!=null&&actor is BaseAI baseAI){
          //  [https://answers.unity.com/questions/1035587/how-to-get-current-time-of-an-animator.html]
          animatorClip.Clear();
          AnimatorStateInfo animatorState=animator.GetCurrentAnimatorStateInfo(0);
                                          animator.GetCurrentAnimatorClipInfo (0,animatorClip);
          if(animatorClip.Count>0){
           if(lastClipName!=animatorClip[0].clip.name){
            Log.DebugMessage("changed to new animatorClip[0].clip.name:"+animatorClip[0].clip.name);
            lastClipName=animatorClip[0].clip.name;
           }
           //Log.DebugMessage("current animatorClip[0].clip.name:"+animatorClip[0].clip.name);
           animationTime=animatorClip[0].clip.length*animatorState.normalizedTime;
          }
          if(lastMotion!=baseAI.motion){
           Log.DebugMessage("actor motion will be set from:"+lastMotion+" to:"+baseAI.motion);
          }
             if(baseAI is ArthurCondinoAI arthurCondinoAI){
              animator.SetBool("MOTION_STAND",arthurCondinoAI.motion==BaseAI.ActorMotion.MOTION_STAND);
              animator.SetBool("MOTION_MOVE" ,arthurCondinoAI.motion==BaseAI.ActorMotion.MOTION_MOVE );
               animator.SetFloat("MOTION_MOVE_VELOCITY",arthurCondinoAI.moveVelocity);
                animator.SetFloat("MOTION_MOVE_TURN",arthurCondinoAI.turnAngle/180f);
             }
          if(lastMotion!=baseAI.motion){
           Log.DebugMessage("actor changed motion from:"+lastMotion+" to:"+baseAI.motion);
          }
          lastMotion=baseAI.motion;
         }
        }
        /// <summary>
        ///  Check if animation locks another motion beforehand
        /// </summary>
        /// <param name="motion"></param>
        /// <returns></returns>
        internal bool CurrentAnimationAllowsMotionChangeTo(BaseAI.ActorMotion motion){
         return true;
        }
        /// <summary>
        ///  Wait for the end of the currently running animation
        /// </summary>
        /// <returns></returns>
        internal bool Sync(){
         return synced;
        }
    }
}