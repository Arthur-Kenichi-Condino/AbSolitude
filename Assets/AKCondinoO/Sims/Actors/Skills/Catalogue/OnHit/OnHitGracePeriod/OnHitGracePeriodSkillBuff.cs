#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal class OnHitGracePeriodSkillBuff:SkillBuff{
        internal OnHitGracePeriodSkillBuff():base(){
         Log.DebugMessage("OnHitGracePeriodEffect ctor");
         onHitGracePeriodEffect=new OnHitGracePeriodEffect();
        }
     internal OnHitGracePeriodEffect onHitGracePeriodEffect{
      get{
       return(OnHitGracePeriodEffect)effect;
      }
      private set{
       effect=value;
      }
     }
     internal float vulnerabilityDuration;
      internal float gracePeriodForResetAnimation;
       internal float gracePeriod;
        internal class OnHitGracePeriodEffect:Effect{
         internal float hitCanTriggerAnimationDuration;
         internal float hitCanTriggerAnimation;
          internal float hitCantTriggerResetAnimationDuration;
          internal float hitCantTriggerResetAnimation;
           internal float hitCantTriggerAnimationDuration;
           internal float hitCantTriggerAnimation;
         internal float deltaTime;
            internal override void Apply(SimObject.Stats stats){
             Log.DebugMessage("OnHitGracePeriodEffect:Apply");
            }
            internal override bool ApplyRepeating(SimObject.Stats stats){
             //Log.DebugMessage("OnHitGracePeriodEffect:ApplyRepeating");
             if(hitCantTriggerResetAnimation>0f){
              //Log.DebugMessage("hitCantTriggerResetAnimation=="+hitCantTriggerResetAnimation);
              hitCantTriggerResetAnimation-=deltaTime;
             }
             if(hitCantTriggerAnimation>0f){
              //Log.DebugMessage("hitCantTriggerAnimation=="+hitCantTriggerAnimation);
              hitCantTriggerAnimation-=deltaTime;
             }
             if(hitCanTriggerAnimation>0f){
              //Log.DebugMessage("hitCanTriggerAnimation=="+hitCanTriggerAnimation);
              hitCanTriggerAnimation-=deltaTime;
              if(hitCanTriggerAnimation<=0f){
               hitCantTriggerAnimation=hitCantTriggerAnimationDuration;
               Log.DebugMessage("hitCantTriggerAnimation="+hitCantTriggerAnimation);
              }
             }
             return false;
            }
            internal override void Unapply(SimObject.Stats stats){
             hitCanTriggerAnimation=0f;
              hitCantTriggerResetAnimation=0f;
               hitCantTriggerAnimation=0f;
            }
        }
        internal override void OnApply(bool gameExiting=false){
         onHitGracePeriodEffect.deltaTime=this.deltaTime;
         if(!applied){
          //  apply buff effects here
          onHitGracePeriodEffect.hitCanTriggerAnimationDuration=vulnerabilityDuration;
           onHitGracePeriodEffect.hitCantTriggerResetAnimationDuration=gracePeriodForResetAnimation;
            onHitGracePeriodEffect.hitCantTriggerAnimationDuration=gracePeriod;
         }
         base.OnApply(gameExiting);
        }
    }
}