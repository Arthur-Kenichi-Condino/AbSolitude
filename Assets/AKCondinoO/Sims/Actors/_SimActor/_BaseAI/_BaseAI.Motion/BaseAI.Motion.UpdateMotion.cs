#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal virtual void UpdateMotion(bool fromAI){
         if(fromAI){
          if(motionFlagForHitAnimation){
              motionFlagForAttackAnimation=false;
              if(MyWeaponType==WeaponTypes.SniperRifle){
               MyMotion=ActorMotion.MOTION_HIT_RIFLE;
              }else{
               MyMotion=ActorMotion.MOTION_HIT;
              }
              OnMotionHitSet();
          }else{
              if(motionFlagForAttackAnimation){
                  if(MyWeaponType==WeaponTypes.SniperRifle){
                   MyMotion=ActorMotion.MOTION_ATTACK_RIFLE;
                  }else{
                   MyMotion=ActorMotion.MOTION_ATTACK;
                  }
              }else{
                  if((moveVelocityFlattened!=0f||moveStrafeVelocityFlattened!=0f)&&
                     MyPathfinding!=PathfindingResult.REACHED&&
                     MyPathfinding!=PathfindingResult.IDLE&&
                     MyPathfinding!=PathfindingResult.TRAVELLING_BUT_NO_SPEED
                  ){
                      if(MyWeaponType==WeaponTypes.SniperRifle){
                       MyMotion=ActorMotion.MOTION_MOVE_RIFLE;
                      }else{
                       MyMotion=ActorMotion.MOTION_MOVE;
                      }
                  }else{
                      if(MyWeaponType==WeaponTypes.SniperRifle){
                       MyMotion=ActorMotion.MOTION_STAND_RIFLE;
                      }else{
                       MyMotion=ActorMotion.MOTION_STAND;
                      }
                  }
              }
          }
         }else{
          if(motionFlagForHitAnimation){
              motionFlagForAttackAnimation=false;
              if(MyWeaponType==WeaponTypes.SniperRifle){
               MyMotion=ActorMotion.MOTION_HIT_RIFLE;
              }else{
               MyMotion=ActorMotion.MOTION_HIT;
              }
              OnMotionHitSet();
          }else{
              if(moveVelocityFlattened!=0f||moveStrafeVelocityFlattened!=0f){
                  if(MyWeaponType==WeaponTypes.SniperRifle){
                   MyMotion=ActorMotion.MOTION_MOVE_RIFLE;
                  }else{
                   MyMotion=ActorMotion.MOTION_MOVE;
                  }
              }else{
                  if(MyWeaponType==WeaponTypes.SniperRifle){
                   MyMotion=ActorMotion.MOTION_STAND_RIFLE;
                  }else{
                   MyMotion=ActorMotion.MOTION_STAND;
                  }
              }
          }
         }
        }
        protected virtual void OnResetMotion(){
         motionFlagForHitAnimation=false;
          motionFlagForHitResetAnimation=false;
         motionFlagForAttackAnimation=false;
         MyMotion=ActorMotion.MOTION_STAND;
         navMeshAgentShouldBeStopped=false;
         if(characterController!=null){
          characterController.isStopped=false;
         }
        }
        internal virtual void OnShouldSetNextMotionAnimatorAnimationLooped(AnimatorStateInfo animatorState,int layerIndex,string currentClipName){
         //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:"+currentClipName);
         if(motionFlagForShootingAnimation){
          if      (MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_STAND_FIRING_RIFLE){
           motionFlagForShootingAnimation=false;
           Log.DebugMessage("onDoShootingSetMotion=false");
          }
          Log.DebugMessage("MapAnimatorClipNameToActorMotion:"+motion);
         }
         if(motionFlagForHitAnimation){
          //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:onHitSetMotion:currentClipName:"+currentClipName);
          if      (MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_HIT){
           motionFlagForHitAnimation=false;
            motionFlagForHitResetAnimation=false;
           MyMotion=ActorMotion.MOTION_STAND;
           OnMotionHitAnimationEnd();
          }else if(MapAnimatorClipNameToActorMotion(currentClipName,out             motion)&&motion.Value==ActorMotion.MOTION_HIT_RIFLE){
           motionFlagForHitAnimation=false;
            motionFlagForHitResetAnimation=false;
           MyMotion=ActorMotion.MOTION_STAND_RIFLE;
           OnMotionHitAnimationEnd();
          }
         }else{
          if(motionFlagForAttackAnimation){
           //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:onDoAttackSetMotion:currentClipName:"+currentClipName);
           if(MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_ATTACK){
            //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:motion.Value:"+motion.Value);
            motionFlagForAttackAnimation=false;
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
         if(motionFlagForHitAnimation){
          //Log.DebugMessage("onHitResetMotion=="+onHitResetMotion);
          if(motionFlagForHitResetAnimation){
           if      (MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_HIT){
            string fullPath=animatorController.GetFullPath(layerIndex,currentClipName);
            Log.DebugMessage("fullPath:"+fullPath);
            animatorController.animator.Play(fullPath,layerIndex,0f);
            motionFlagForHitResetAnimation=false;
            OnMotionHitReset();
           }else if(MapAnimatorClipNameToActorMotion(currentClipName,out             motion)&&motion.Value==ActorMotion.MOTION_HIT_RIFLE){
            string fullPath=animatorController.GetFullPath(layerIndex,currentClipName);
            Log.DebugMessage("fullPath:"+fullPath);
            animatorController.animator.Play(fullPath,layerIndex,0f);
            motionFlagForHitResetAnimation=false;
            OnMotionHitReset();
           }
          }
         }
        }
        protected virtual void OnMotionHitSet(){
         navMeshAgentShouldBeStopped=true;
         if(characterController!=null){
          characterController.isStopped=true;
         }
        }
        protected virtual void OnMotionHitReset(){
         navMeshAgentShouldBeStopped=true;
         if(characterController!=null){
          characterController.isStopped=true;
         }
        }
        protected virtual void OnMotionHitAnimationEnd(){
         navMeshAgentShouldBeStopped=false;
         if(characterController!=null){
          characterController.isStopped=false;
         }
        }
     internal static readonly Dictionary<string,ActorMotion>animatorClipNameToActorMotion=new Dictionary<string,ActorMotion>();
        internal bool MapAnimatorClipNameToActorMotion(string clipName,out ActorMotion?motion){
         //Log.DebugMessage("MapAnimatorClipNameToActorMotion:clipName:"+clipName);
         motion=null;
         if(animatorClipNameToActorMotion.TryGetValue(clipName,out ActorMotion clipToMotion)){
          motion=clipToMotion;
          Log.DebugMessage("MapAnimatorClipNameToActorMotion:return "+motion+" from mapped clipName "+clipName);
          return true;
         }
         if(clipName.Contains("MOTION_HIT_RIFLE")){
          motion=animatorClipNameToActorMotion[clipName]=ActorMotion.MOTION_HIT_RIFLE;
          Log.DebugMessage("MapAnimatorClipNameToActorMotion:mapped "+clipName+" to MOTION_HIT_RIFLE");
          return true;
         }
         if(clipName.Contains("MOTION_HIT")){
          motion=animatorClipNameToActorMotion[clipName]=ActorMotion.MOTION_HIT;
          Log.DebugMessage("MapAnimatorClipNameToActorMotion:mapped "+clipName+" to MOTION_HIT");
          return true;
         }
         if(clipName.Contains("MOTION_ATTACK")){
          motion=animatorClipNameToActorMotion[clipName]=ActorMotion.MOTION_ATTACK;
          Log.DebugMessage("MapAnimatorClipNameToActorMotion:mapped "+clipName+" to MOTION_ATTACK");
          return true;
         }
         if(clipName.Contains("MOTION_STAND_FIRING_RIFLE")){
          motion=animatorClipNameToActorMotion[clipName]=ActorMotion.MOTION_STAND_FIRING_RIFLE;
          Log.DebugMessage("MapAnimatorClipNameToActorMotion:mapped "+clipName+" to MOTION_STAND_FIRING_RIFLE");
          return true;
         }
         return false;
        }
    }
}