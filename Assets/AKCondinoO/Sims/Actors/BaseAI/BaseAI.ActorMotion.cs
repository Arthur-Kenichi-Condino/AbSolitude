#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal enum ActorMotion:int{
         MOTION_STAND =0,
         MOTION_STAND_RIFLE =50,
         MOTION_MOVE  =1,
         MOTION_MOVE_RIFLE  =51,
         MOTION_ATTACK=2,
         MOTION_ATTACK_RIFLE=52,
         MOTION_HIT   =4,
         MOTION_HIT_RIFLE   =54,
        }
        internal virtual void UpdateMotion(bool fromAI){
         if(fromAI){
          if(onHitSetMotion){
              onDoAttackSetMotion=false;
              if(MyWeaponType==WeaponTypes.SniperRifle){
               MyMotion=ActorMotion.MOTION_HIT_RIFLE;
              }else{
               MyMotion=ActorMotion.MOTION_HIT;
              }
          }else{
              if(onDoAttackSetMotion){
                  if(MyWeaponType==WeaponTypes.SniperRifle){
                   MyMotion=ActorMotion.MOTION_ATTACK_RIFLE;
                  }else{
                   MyMotion=ActorMotion.MOTION_ATTACK;
                  }
              }else{
                  if(MyPathfinding==PathfindingResult.TRAVELLING){
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
         return false;
        }
        internal virtual void OnShouldSetNextMotionAnimatorAnimationLooped(AnimatorStateInfo animatorState,int layerIndex,string currentClipName){
         //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped");
         if(onHitSetMotion){
          //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:onHitSetMotion:currentClipName:"+currentClipName);
          if      (MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_HIT){
           onHitSetMotion=false;
            onHitResetMotion=false;
           MyMotion=ActorMotion.MOTION_STAND;
          }else if(MapAnimatorClipNameToActorMotion(currentClipName,out             motion)&&motion.Value==ActorMotion.MOTION_HIT_RIFLE){
           onHitSetMotion=false;
            onHitResetMotion=false;
           MyMotion=ActorMotion.MOTION_STAND_RIFLE;
          }
         }
         if(onDoAttackSetMotion){
          //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:onDoAttackSetMotion:currentClipName:"+currentClipName);
          if(MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_ATTACK){
           //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:motion.Value:"+motion.Value);
           onDoAttackSetMotion=false;
           MyMotion=ActorMotion.MOTION_STAND;
          }
         }
        }
        internal virtual void OnShouldSetNextMotionAnimatorAnimationChanged(AnimatorStateInfo animatorState,int layerIndex,string lastClipName,string currentClipName){
        }
        internal virtual void OnShouldSetNextMotionAnimatorAnimationIsPlaying(AnimatorStateInfo animatorState,int layerIndex,string currentClipName){
         if(onHitSetMotion){
          Log.DebugMessage("onHitResetMotion=="+onHitResetMotion);
          if(onHitResetMotion){
           if      (MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_HIT){
            onHitResetMotion=false;
           }else if(MapAnimatorClipNameToActorMotion(currentClipName,out             motion)&&motion.Value==ActorMotion.MOTION_HIT_RIFLE){
            string fullPath=simActorAnimatorController.GetFullPath(layerIndex,currentClipName);
            Log.DebugMessage("fullPath:"+fullPath);
            simActorAnimatorController.animator.Play(fullPath,layerIndex,0f);
            onHitResetMotion=false;
           }
          }
         }
        }
    }
}