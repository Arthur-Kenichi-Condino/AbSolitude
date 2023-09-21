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
    }
}