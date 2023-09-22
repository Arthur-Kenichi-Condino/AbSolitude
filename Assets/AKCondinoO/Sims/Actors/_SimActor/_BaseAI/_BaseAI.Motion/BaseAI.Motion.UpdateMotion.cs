#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal virtual void UpdateMotion(bool fromAI){
         if(fromAI){
          if(onHitSetMotion){
              onDoAttackSetMotion=false;
              if(MyWeaponType==WeaponTypes.SniperRifle){
               MyMotion=ActorMotion.MOTION_HIT_RIFLE;
              }else{
               MyMotion=ActorMotion.MOTION_HIT;
              }
              OnMotionHitSet();
          }else{
              if(onDoAttackSetMotion){
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
          if(onHitSetMotion){
              onDoAttackSetMotion=false;
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
         onHitSetMotion=false;
          onHitResetMotion=false;
         onDoAttackSetMotion=false;
         MyMotion=ActorMotion.MOTION_STAND;
         navMeshAgentShouldBeStopped=false;
         if(characterController!=null){
          characterController.isStopped=false;
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
    }
}