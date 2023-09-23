#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     protected ActorMotion MyMotion=ActorMotion.MOTION_STAND;internal ActorMotion motion{get{return MyMotion;}}
        internal enum ActorMotion:int{
         MOTION_STAND =0,
         MOTION_MOVE  =1,
         MOTION_ATTACK=2,
         MOTION_HIT   =4,
         MOTION_STAND_RIFLE =50,
         MOTION_MOVE_RIFLE  =51,
         MOTION_ATTACK_RIFLE=52,
         MOTION_HIT_RIFLE   =54,
        }
        internal enum ActorWeaponLayerMotion:int{
         MOTION_STAND_FIRING_RIFLE=501,
        }
    }
}