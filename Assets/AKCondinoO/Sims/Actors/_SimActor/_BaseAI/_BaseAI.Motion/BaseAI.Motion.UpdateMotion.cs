#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     internal readonly Dictionary<ActorMotion,int>motionMappedToLayerIndex=new Dictionary<ActorMotion,int>();
      internal Dictionary<int,bool>currentAnimationMapsToMotion;
       internal Dictionary<int,float>currentMotionAnimationTime;
        internal virtual void UpdateMotion(bool fromAI){
         if(motionFlagForReloadingAnimation){
             if(MyWeaponLayerMotion==ActorWeaponLayerMotion.MOTION_STAND_RIFLE_AIMING||
                MyWeaponLayerMotion==ActorWeaponLayerMotion.NONE
             ){
              MyWeaponLayerMotion=ActorWeaponLayerMotion.MOTION_STAND_RIFLE_RELOADING;
             }
             if(MyWeaponLayerMotion==ActorWeaponLayerMotion.MOTION_STAND_RIFLE_RELOADING
             ){
              if(motionFlagForShootingAnimation){
               motionFlagForShootingAnimation=false;
               if(animatorController.animator!=null){
                if(animatorController.animationEventsHandler!=null){
                 animatorController.animationEventsHandler.CancelShootEvent();
                }
               }
              }
             }
         }else{
             if(motionFlagForShootingAnimation){
                 if(MyWeaponLayerMotion==ActorWeaponLayerMotion.MOTION_STAND_RIFLE_AIMING||
                    MyWeaponLayerMotion==ActorWeaponLayerMotion.NONE
                 ){
                  MyWeaponLayerMotion=ActorWeaponLayerMotion.MOTION_STAND_RIFLE_FIRING;
                 }
             }else{
                 if(isAiming){
                  MyWeaponLayerMotion=ActorWeaponLayerMotion.MOTION_STAND_RIFLE_AIMING;
                 }else{
                  MyWeaponLayerMotion=ActorWeaponLayerMotion.NONE;
                 }
             }
         }
         //Log.DebugMessage("MyWeaponLayerMotion:"+MyWeaponLayerMotion);
         if(motionFlagForDeathAnimation){
             ActorMotion lastMotion=MyMotion;
             if(MyMotion!=ActorMotion.MOTION_DEAD&&
                MyMotion!=ActorMotion.MOTION_DEAD_RIFLE
             ){
              if(MyWeaponType==WeaponTypes.SniperRifle){
               MyMotion=ActorMotion.MOTION_DEAD_RIFLE;
              }else{
               MyMotion=ActorMotion.MOTION_DEAD;
              }
             }
             if(MyMotion==ActorMotion.MOTION_DEAD||
                MyMotion==ActorMotion.MOTION_DEAD_RIFLE
             ){
              if(motionFlagForHitAnimation){
               motionFlagForHitAnimation=false;
                motionFlagForHitResetAnimation=false;
              }
              if(lastMotion==ActorMotion.MOTION_HIT||
                 lastMotion==ActorMotion.MOTION_HIT_RIFLE
              ){
               OnMotionHitInterrupt();
              }
              if(motionFlagForAttackAnimation){
               motionFlagForAttackAnimation=false;
              }
              if(lastMotion==ActorMotion.MOTION_ATTACK||
                 lastMotion==ActorMotion.MOTION_ATTACK_RIFLE
              ){
               OnMotionAttackInterrupt();
              }
              OnMotionDeadSet();
             }
         }else{
             if(motionFlagForHitAnimation){
                 ActorMotion lastMotion=MyMotion;
                 if(MyMotion!=ActorMotion.MOTION_HIT&&
                    MyMotion!=ActorMotion.MOTION_HIT_RIFLE
                 ){
                  if(MyWeaponType==WeaponTypes.SniperRifle){
                   MyMotion=ActorMotion.MOTION_HIT_RIFLE;
                  }else{
                   MyMotion=ActorMotion.MOTION_HIT;
                  }
                 }
                 if(MyMotion==ActorMotion.MOTION_HIT||
                    MyMotion==ActorMotion.MOTION_HIT_RIFLE
                 ){
                  if(motionFlagForAttackAnimation){
                   motionFlagForAttackAnimation=false;
                  }
                  if(lastMotion==ActorMotion.MOTION_ATTACK||
                     lastMotion==ActorMotion.MOTION_ATTACK_RIFLE
                  ){
                   OnMotionAttackInterrupt();
                  }
                  OnMotionHitSet();
                 }
             }else{
                 if(motionFlagForAttackAnimation){
                     if(MyMotion!=ActorMotion.MOTION_ATTACK&&
                        MyMotion!=ActorMotion.MOTION_ATTACK_RIFLE
                     ){
                      if(MyWeaponType==WeaponTypes.SniperRifle){
                       MyMotion=ActorMotion.MOTION_ATTACK_RIFLE;
                      }else{
                       MyMotion=ActorMotion.MOTION_ATTACK;
                      }
                     }
                     if(MyMotion==ActorMotion.MOTION_ATTACK||
                        MyMotion==ActorMotion.MOTION_ATTACK_RIFLE
                     ){
                      OnMotionAttackSet();
                     }
                 }else{
                     if(
                      (moveVelocityFlattened!=0f||moveStrafeVelocityFlattened!=0f)&&
                      (
                       (!fromAI)||
                       (ai!=null&&
                        ai.MyPathfinding!=PathfindingResult.REACHED&&
                        ai.MyPathfinding!=PathfindingResult.IDLE&&
                        ai.MyPathfinding!=PathfindingResult.TRAVELLING_BUT_NO_SPEED
                       )
                      )
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
         }
        }
        protected virtual void OnResetMotion(){
         motionFlagForAttackAnimation=false;
          motionFlagForReloadingAnimation=false;
           motionFlagForShootingAnimation=false;
         MyWeaponLayerMotion=ActorWeaponLayerMotion.MOTION_STAND_RIFLE_AIMING;
         motionFlagForDeathAnimation=false;
          motionFlagForDeathInstantAnimationJumpToEnd=false;
         motionFlagForHitAnimation=false;
          motionFlagForHitResetAnimation=false;
         MyMotion=ActorMotion.MOTION_STAND;
         OnMotionShouldStopMovement(false);
        }
        internal virtual void OnShouldSetNextMotionAnimatorAnimationLooped(AnimatorStateInfo animatorState,int layerIndex,string currentClipName){
         //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:"+currentClipName);
         if(motionFlagForReloadingAnimation){
          if      (MapAnimatorClipNameToActorWeaponLayerMotion(currentClipName,out ActorWeaponLayerMotion?wMotion)&&wMotion.Value==ActorWeaponLayerMotion.MOTION_STAND_RIFLE_RELOADING){
           motionFlagForReloadingAnimation=false;
           MyWeaponLayerMotion=ActorWeaponLayerMotion.MOTION_STAND_RIFLE_AIMING;
           if(animatorController.animator!=null){
            if(animatorController.animationEventsHandler!=null){
             animatorController.animationEventsHandler.CancelReloadEvent();
            }
           }
           Log.DebugMessage("motionFlagForReloadingAnimation=false");
          }
          Log.DebugMessage("MapAnimatorClipNameToActorWeaponLayerMotion:"+wMotion);
         }else{
          if(motionFlagForShootingAnimation){
           if      (MapAnimatorClipNameToActorWeaponLayerMotion(currentClipName,out ActorWeaponLayerMotion?wMotion)&&wMotion.Value==ActorWeaponLayerMotion.MOTION_STAND_RIFLE_FIRING){
            motionFlagForShootingAnimation=false;
            MyWeaponLayerMotion=ActorWeaponLayerMotion.MOTION_STAND_RIFLE_AIMING;
            if(animatorController.animator!=null){
             if(animatorController.animationEventsHandler!=null){
              animatorController.animationEventsHandler.CancelShootEvent();
             }
            }
            Log.DebugMessage("motionFlagForShootingAnimation=false");
           }
           Log.DebugMessage("MapAnimatorClipNameToActorWeaponLayerMotion:"+wMotion);
          }
         }
         if(motionFlagForHitAnimation){
          //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:onHitSetMotion:currentClipName:"+currentClipName);
          if      (               MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion) &&motion.Value==ActorMotion.MOTION_HIT){
           motionFlagForHitAnimation=false;
            motionFlagForHitResetAnimation=false;
           MyMotion=ActorMotion.MOTION_STAND;
           OnMotionHitAnimationEnd();
          }else if((motion!=null||MapAnimatorClipNameToActorMotion(currentClipName,out             motion))&&motion.Value==ActorMotion.MOTION_HIT_RIFLE){
           motionFlagForHitAnimation=false;
            motionFlagForHitResetAnimation=false;
           MyMotion=ActorMotion.MOTION_STAND_RIFLE;
           OnMotionHitAnimationEnd();
          }
         }else{
          if(motionFlagForAttackAnimation){
           //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:onDoAttackSetMotion:currentClipName:"+currentClipName);
           if      (               MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion) &&motion.Value==ActorMotion.MOTION_ATTACK){
            //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:motion.Value:"+motion.Value);
            motionFlagForAttackAnimation=false;
            MyMotion=ActorMotion.MOTION_STAND;
            OnMotionAttackAnimationEnd();
           }else if((motion!=null||MapAnimatorClipNameToActorMotion(currentClipName,out             motion))&&motion.Value==ActorMotion.MOTION_ATTACK_RIFLE){
            //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationLooped:motion.Value:"+motion.Value);
            motionFlagForAttackAnimation=false;
            MyMotion=ActorMotion.MOTION_STAND_RIFLE;
            OnMotionAttackAnimationEnd();
           }
          }
         }
        }
        internal virtual void OnShouldSetNextMotionAnimatorAnimationChanged(AnimatorStateInfo animatorState,int layerIndex,string lastClipName,string currentClipName){
         //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationChanged:currentClipName:"+currentClipName+",lastClipName:"+lastClipName);
         bool mapped=MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?curMotion);
         if(mapped){
          motionMappedToLayerIndex[curMotion.Value]=layerIndex;
         }
         currentAnimationMapsToMotion[layerIndex]=(mapped&&curMotion==MyMotion);
         if(motionFlagForReloadingAnimation){
          if      (MapAnimatorClipNameToActorWeaponLayerMotion(lastClipName,out ActorWeaponLayerMotion?wMotion)&&wMotion.Value==ActorWeaponLayerMotion.MOTION_STAND_RIFLE_RELOADING){
           motionFlagForReloadingAnimation=false;
           MyWeaponLayerMotion=ActorWeaponLayerMotion.MOTION_STAND_RIFLE_AIMING;
           if(animatorController.animator!=null){
            if(animatorController.animationEventsHandler!=null){
             animatorController.animationEventsHandler.CancelReloadEvent();
            }
           }
           Log.DebugMessage("motionFlagForReloadingAnimation=false");
          }
          //Log.DebugMessage("MapAnimatorClipNameToActorWeaponLayerMotion:"+motion);
         }else{
          if(motionFlagForShootingAnimation){
           if      (MapAnimatorClipNameToActorWeaponLayerMotion(lastClipName,out ActorWeaponLayerMotion?wMotion)&&wMotion.Value==ActorWeaponLayerMotion.MOTION_STAND_RIFLE_FIRING){
            motionFlagForShootingAnimation=false;
            MyWeaponLayerMotion=ActorWeaponLayerMotion.MOTION_STAND_RIFLE_AIMING;
            if(animatorController.animator!=null){
             if(animatorController.animationEventsHandler!=null){
              animatorController.animationEventsHandler.CancelShootEvent();
             }
            }
            Log.DebugMessage("motionFlagForShootingAnimation=false");
           }
           Log.DebugMessage("MapAnimatorClipNameToActorWeaponLayerMotion:"+wMotion);
          }else{
          }
         }
        }
        internal virtual void OnShouldSetNextMotionAnimatorAnimationIsPlaying(AnimatorStateInfo animatorState,int layerIndex,string currentClipName){
         //Log.DebugMessage("OnShouldSetNextMotionAnimatorAnimationIsPlaying:"+currentClipName);
         if(motionFlagForHitAnimation){
          //Log.DebugMessage("onHitResetMotion=="+onHitResetMotion);
          if(motionFlagForHitResetAnimation){
           if      (MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_HIT){
            //string fullPath=animatorController.GetFullPath(layerIndex,currentClipName);
            //Log.DebugMessage("fullPath:"+fullPath);
            //animatorController.animator.Play(fullPath,layerIndex,0f);
            animatorController.animator.SetTrigger("MOTION_HIT_RESET");
            motionFlagForHitResetAnimation=false;
            OnMotionHitReset();
           }else if(MapAnimatorClipNameToActorMotion(currentClipName,out             motion)&&motion.Value==ActorMotion.MOTION_HIT_RIFLE){
            //string fullPath=animatorController.GetFullPath(layerIndex,currentClipName);
            //Log.DebugMessage("fullPath:"+fullPath);
            //animatorController.animator.Play(fullPath,layerIndex,0f);
            animatorController.animator.SetTrigger("MOTION_HIT_RIFLE_RESET");
            motionFlagForHitResetAnimation=false;
            OnMotionHitReset();
           }
          }
         }else{
          if(motionFlagForDeathAnimation){
           if(motionFlagForDeathInstantAnimationJumpToEnd){
            if      (MapAnimatorClipNameToActorMotion(currentClipName,out ActorMotion?motion)&&motion.Value==ActorMotion.MOTION_DEAD){
             Log.DebugMessage("motionFlagForDeathInstantAnimationJumpToEnd");
             animatorController.SetMotionTime(1f);
             motionFlagForDeathInstantAnimationJumpToEnd=false;
            }else if(MapAnimatorClipNameToActorMotion(currentClipName,out             motion)&&motion.Value==ActorMotion.MOTION_DEAD_RIFLE){
             Log.DebugMessage("motionFlagForDeathInstantAnimationJumpToEnd");
             animatorController.SetMotionTime(1f);
             motionFlagForDeathInstantAnimationJumpToEnd=false;
            }
           }
          }
         }
        }
        protected virtual void OnMotionAttackSet(){
         OnMotionShouldStopMovement(true);
        }
        protected virtual void OnMotionAttackAnimationEnd(){
         OnMotionShouldStopMovement(false);
        }
        protected virtual void OnMotionAttackInterrupt(){
         OnMotionShouldStopMovement(false);
         if(animatorController.animationEventsHandler!=null){
          animatorController.animationEventsHandler.OnCantDamageAnimationEvent("");
         }
        }
        protected virtual void OnMotionHitSet(){
         OnMotionShouldStopMovement(true);
        }
        protected virtual void OnMotionHitReset(){
         OnMotionShouldStopMovement(true);
        }
        protected virtual void OnMotionHitAnimationEnd(){
         OnMotionShouldStopMovement(false);
        }
        protected virtual void OnMotionHitInterrupt(){
         OnMotionShouldStopMovement(false);
        }
        protected virtual void OnMotionDeadSet(){
         OnMotionShouldStopMovement(true);
        }
        protected virtual void OnMotionShouldStopMovement(bool stop){
         navMeshAgentShouldBeStopped=stop;
         if(characterController!=null){
          characterController.isStopped=stop;
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
         if(Map("MOTION_ATTACK_RIFLE",ActorMotion.MOTION_ATTACK_RIFLE,out motion))return true;
         if(Map("MOTION_ATTACK"      ,ActorMotion.MOTION_ATTACK      ,out motion))return true;
         if(Map("MOTION_DEAD_RIFLE"  ,ActorMotion.MOTION_DEAD_RIFLE  ,out motion))return true;
         if(Map("MOTION_DEAD"        ,ActorMotion.MOTION_DEAD        ,out motion))return true;
         if(Map("MOTION_HIT_RIFLE"   ,ActorMotion.MOTION_HIT_RIFLE   ,out motion))return true;
         if(Map("MOTION_HIT"         ,ActorMotion.MOTION_HIT         ,out motion))return true;
         bool Map(string motionName,ActorMotion toMotion,out ActorMotion?motionMapped){
          motionMapped=null;
          if(clipName.Contains(motionName)){
           motionMapped=animatorClipNameToActorMotion[clipName]=toMotion;
           Log.DebugMessage("MapAnimatorClipNameToActorMotion:mapped "+clipName+" to "+motionName);
           return true;
          }
          return false;
         }
         return false;
        }
     internal static readonly Dictionary<string,ActorWeaponLayerMotion>animatorClipNameToActorWeaponLayerMotion=new Dictionary<string,ActorWeaponLayerMotion>();
        internal bool MapAnimatorClipNameToActorWeaponLayerMotion(string clipName,out ActorWeaponLayerMotion?motion){
         motion=null;
         if(clipName.Contains("MOTION_STAND_RIFLE_RELOADING")){
          motion=animatorClipNameToActorWeaponLayerMotion[clipName]=ActorWeaponLayerMotion.MOTION_STAND_RIFLE_RELOADING;
          Log.DebugMessage("MapAnimatorClipNameToActorWeaponLayerMotion:mapped "+clipName+" to MOTION_STAND_RIFLE_RELOADING");
          return true;
         }
         if(clipName.Contains("MOTION_STAND_RIFLE_FIRING")){
          motion=animatorClipNameToActorWeaponLayerMotion[clipName]=ActorWeaponLayerMotion.MOTION_STAND_RIFLE_FIRING;
          Log.DebugMessage("MapAnimatorClipNameToActorWeaponLayerMotion:mapped "+clipName+" to MOTION_STAND_RIFLE_FIRING");
          return true;
         }
         return false;
        }
    }
}