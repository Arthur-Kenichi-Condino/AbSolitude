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
      internal Vector3 actorLeft;
      internal Vector3 actorRight;
     internal Animator animator;
     internal SimActorAnimatorIKController animatorIKController;
     Vector3 tgtRot,tgtRot_Last;
     Vector3 tgtPos,tgtPos_Last;
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
          tgtPos=actor.simActorCharacterController.characterController.transform.position+actor.simUMADataPosOffset;
          actorLeft=-actor.transform.right;
          actorRight=actor.transform.right;
          Vector3 boundsMaxRight=actor.simActorCharacterController.characterController.bounds.max;
                  boundsMaxRight.y=actor.simActorCharacterController.characterController.transform.position.y;
                  boundsMaxRight.z=actor.simActorCharacterController.characterController.transform.position.z;
          float maxRightDis=Vector3.Distance(actor.simActorCharacterController.characterController.transform.position,boundsMaxRight);
          Vector3 maxRight=actor.simActorCharacterController.characterController.transform.position+actor.simActorCharacterController.characterController.transform.rotation*(Vector3.right*maxRightDis);
          Vector3 boundsMinLeft=actor.simActorCharacterController.characterController.bounds.min;
                  boundsMinLeft.y=actor.simActorCharacterController.characterController.transform.position.y;
                  boundsMinLeft.z=actor.simActorCharacterController.characterController.transform.position.z;
          float minLeftDis=Vector3.Distance(actor.simActorCharacterController.characterController.transform.position,boundsMinLeft);
          Vector3 minLeft=actor.simActorCharacterController.characterController.transform.position+actor.simActorCharacterController.characterController.transform.rotation*(Vector3.left*minLeftDis);
          if(actor.navMeshAgent.enabled||actor.simActorCharacterController.characterController.isGrounded){
           Debug.DrawRay(maxRight,Vector3.down,Color.blue);
           if(Physics.Raycast(maxRight,Vector3.down,out RaycastHit rightFloorHit)){
            Debug.DrawRay(rightFloorHit.point,rightFloorHit.normal);
            Vector3 bottom=actor.simActorCharacterController.characterController.bounds.center;
                    bottom.y=actor.simActorCharacterController.characterController.bounds.min.y;
            Plane floorPlane=new Plane(rightFloorHit.normal,bottom);
            Ray leftRay=new Ray(minLeft,Vector3.down);
            Debug.DrawRay(leftRay.origin,leftRay.direction,Color.blue);
            if(floorPlane.Raycast(leftRay,out float enter)){
             Vector3 leftFloorHitPoint=leftRay.GetPoint(enter);
             float minY=Mathf.Min(bottom.y,leftFloorHitPoint.y,rightFloorHit.point.y);
             tgtPos.y+=minY-bottom.y;
             Debug.DrawLine(bottom,tgtPos,Color.yellow);
            }
           }
          }
          if(actor.simUMAData!=null){
           actor.simUMAData.transform.parent.position=tgtPos;
          }
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
              animator.SetBool("MOTION_RIFLE_STAND",arthurCondinoAI.motion==BaseAI.ActorMotion.MOTION_RIFLE_STAND);
              animator.SetBool("MOTION_RIFLE_MOVE" ,arthurCondinoAI.motion==BaseAI.ActorMotion.MOTION_RIFLE_MOVE );
               animator.SetFloat("MOTION_RIFLE_MOVE_VELOCITY",arthurCondinoAI.moveVelocity);
                animator.SetFloat("MOTION_RIFLE_MOVE_TURN",arthurCondinoAI.turnAngle/180f);
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