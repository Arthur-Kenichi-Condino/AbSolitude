#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal virtual void OnShouldSetNextMotionAnimatorAnimationLooped(AnimatorStateInfo animatorState,int layerIndex,string currentClipName){
         //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:"+currentClipName);
         if(onDoShootingSetMotion){
          if      (MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_STAND_FIRING_RIFLE){
           onDoShootingSetMotion=false;
          }
         }
         if(onHitSetMotion){
          //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:onHitSetMotion:currentClipName:"+currentClipName);
          if      (MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_HIT){
           onHitSetMotion=false;
            onHitResetMotion=false;
           MyMotion=ActorMotion.MOTION_STAND;
           OnMotionHitAnimationEnd();
          }else if(MapAnimatorClipNameToActorMotion(currentClipName,out             motion)&&motion.Value==ActorMotion.MOTION_HIT_RIFLE){
           onHitSetMotion=false;
            onHitResetMotion=false;
           MyMotion=ActorMotion.MOTION_STAND_RIFLE;
           OnMotionHitAnimationEnd();
          }
         }else{
          if(onDoAttackSetMotion){
           //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:onDoAttackSetMotion:currentClipName:"+currentClipName);
           if(MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_ATTACK){
            //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:motion.Value:"+motion.Value);
            onDoAttackSetMotion=false;
            MyMotion=ActorMotion.MOTION_STAND;
           }
          }
         }
        }
        internal virtual void OnShouldSetNextMotionAnimatorAnimationChanged(AnimatorStateInfo animatorState,int layerIndex,string lastClipName,string currentClipName){
         //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationChanged:currentClipName:"+currentClipName+",lastClipName:"+lastClipName);
        }
        internal virtual void OnShouldSetNextMotionAnimatorAnimationIsPlaying(AnimatorStateInfo animatorState,int layerIndex,string currentClipName){
         //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationIsPlaying:"+currentClipName);
         if(onHitSetMotion){
          //Log.DebugMessage("onHitResetMotion=="+onHitResetMotion);
          if(onHitResetMotion){
           if      (MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_HIT){
            string fullPath=animatorController.GetFullPath(layerIndex,currentClipName);
            Log.DebugMessage("fullPath:"+fullPath);
            animatorController.animator.Play(fullPath,layerIndex,0f);
            onHitResetMotion=false;
            OnMotionHitReset();
           }else if(MapAnimatorClipNameToActorMotion(currentClipName,out             motion)&&motion.Value==ActorMotion.MOTION_HIT_RIFLE){
            string fullPath=animatorController.GetFullPath(layerIndex,currentClipName);
            Log.DebugMessage("fullPath:"+fullPath);
            animatorController.animator.Play(fullPath,layerIndex,0f);
            onHitResetMotion=false;
            OnMotionHitReset();
           }
          }
         }
        }
     internal static readonly Dictionary<string,ActorMotion>animatorClipNameToActorMotion=new Dictionary<string,ActorMotion>();
        internal bool MapAnimatorClipNameToActorMotion(string clipName,out ActorMotion?motion){
         motion=null;
         if(animatorClipNameToActorMotion.TryGetValue(clipName,out ActorMotion clipToMotion)){
          motion=clipToMotion;
          //Log.DebugMessage("MapAnimatorClipNameToActorMotion:return "+motion+" from mapped clipName "+clipName);
          return true;
         }
         if(clipName.Contains("MOTION_HIT_RIFLE")){
          motion=animatorClipNameToActorMotion[clipName]=ActorMotion.MOTION_HIT_RIFLE;
          //Log.DebugMessage("MapAnimatorClipNameToActorMotion:mapped "+clipName+" to MOTION_HIT_RIFLE");
          return true;
         }
         if(clipName.Contains("MOTION_HIT")){
          motion=animatorClipNameToActorMotion[clipName]=ActorMotion.MOTION_HIT;
          //Log.DebugMessage("MapAnimatorClipNameToActorMotion:mapped "+clipName+" to MOTION_HIT");
          return true;
         }
         if(clipName.Contains("MOTION_ATTACK")){
          motion=animatorClipNameToActorMotion[clipName]=ActorMotion.MOTION_ATTACK;
          //Log.DebugMessage("MapAnimatorClipNameToActorMotion:mapped "+clipName+" to MOTION_ATTACK");
          return true;
         }
         if(clipName.Contains("MOTION_STAND_FIRING_RIFLE")){
          motion=animatorClipNameToActorMotion[clipName]=ActorMotion.MOTION_STAND_FIRING_RIFLE;
          //Log.DebugMessage("MapAnimatorClipNameToActorMotion:mapped "+clipName+" to MOTION_STAND_FIRING_RIFLE");
          return true;
         }
         return false;
        }
    }
}