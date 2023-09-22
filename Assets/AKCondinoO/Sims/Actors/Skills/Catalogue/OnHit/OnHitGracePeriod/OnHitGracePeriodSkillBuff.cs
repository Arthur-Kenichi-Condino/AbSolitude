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
     internal float hitCanTriggerAnimationIsVulnerablePeriodDuration;
      internal float hitCantTriggerAnimationResetIsOnGracePeriodDuration;
       internal float hitCantTriggerAnimationIsOnGracePeriodDuration;
        internal class OnHitGracePeriodEffect:Effect{
         internal float hitCanTriggerAnimationIsVulnerablePeriodDuration;
          internal float hitCantTriggerAnimationResetIsOnGracePeriodDuration;
           internal float hitCantTriggerAnimationIsOnGracePeriodDuration;
         internal float deltaTime;
         internal float hitCanTriggerAnimationIsVulnerablePeriod;
          internal float hitCantTriggerResetAnimationIsOnGracePeriod;
           internal float hitCantTriggerAnimationIsOnGracePeriod;
            internal override void Apply(SimObject.Stats stats){
             Log.DebugMessage("OnHitGracePeriodEffect:Apply");
            }
            internal override bool ApplyRepeating(SimObject.Stats stats){
             //Log.DebugMessage("OnHitGracePeriodEffect:ApplyRepeating");
             if(hitCantTriggerResetAnimationIsOnGracePeriod>0f){
              //Log.DebugMessage("hitCantTriggerAnimationResetIsOnGracePeriod=="+hitCantTriggerAnimationResetIsOnGracePeriod);
              hitCantTriggerResetAnimationIsOnGracePeriod-=deltaTime;
             }
             if(hitCantTriggerAnimationIsOnGracePeriod>0f){
              //Log.DebugMessage("hitCantTriggerAnimationIsOnGracePeriod=="+hitCantTriggerAnimationIsOnGracePeriod);
              hitCantTriggerAnimationIsOnGracePeriod-=deltaTime;
             }
             if(hitCanTriggerAnimationIsVulnerablePeriod>0f){
              //Log.DebugMessage("hitCanTriggerAnimationVulnerablePeriod=="+hitCanTriggerAnimationVulnerablePeriod);
              hitCanTriggerAnimationIsVulnerablePeriod-=deltaTime;
              if(hitCanTriggerAnimationIsVulnerablePeriod<=0f){
               hitCantTriggerAnimationIsOnGracePeriod=hitCantTriggerAnimationIsOnGracePeriodDuration;
               Log.DebugMessage("hitCantTriggerAnimationIsOnGracePeriod="+hitCantTriggerAnimationIsOnGracePeriod);
              }
             }
             return false;
            }
            internal override void Unapply(SimObject.Stats stats){
             hitCanTriggerAnimationIsVulnerablePeriod=0f;
              hitCantTriggerResetAnimationIsOnGracePeriod=0f;
               hitCantTriggerAnimationIsOnGracePeriod=0f;
            }
        }
        internal override void OnApply(bool gameExiting=false){
         onHitGracePeriodEffect.deltaTime=this.deltaTime;
         if(!applied){
          //  apply buff effects here
          onHitGracePeriodEffect.hitCanTriggerAnimationIsVulnerablePeriodDuration=hitCanTriggerAnimationIsVulnerablePeriodDuration;
           onHitGracePeriodEffect.hitCantTriggerAnimationResetIsOnGracePeriodDuration=hitCantTriggerAnimationResetIsOnGracePeriodDuration;
            onHitGracePeriodEffect.hitCantTriggerAnimationIsOnGracePeriodDuration=hitCantTriggerAnimationIsOnGracePeriodDuration;
         }
         base.OnApply(gameExiting);
        }
    }
}