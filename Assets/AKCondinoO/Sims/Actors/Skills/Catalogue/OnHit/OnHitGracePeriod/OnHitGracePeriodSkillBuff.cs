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
     internal float onHitSetMotionVulnerablePeriodDuration;
      internal float onHitResetMotionGracePeriodDuration;
       internal float onHitSetMotionGracePeriodDuration;
        internal class OnHitGracePeriodEffect:Effect{
         internal float onHitSetMotionVulnerablePeriodDuration;
          internal float onHitResetMotionGracePeriodDuration;
           internal float onHitSetMotionGracePeriodDuration;
         internal float deltaTime;
         internal float onHitSetMotionVulnerablePeriod;
          internal float onHitResetMotionGracePeriod;
           internal float onHitSetMotionGracePeriod;
            internal override void Apply(SimObject.Stats stats){
             Log.DebugMessage("OnHitGracePeriodEffect:Apply");
            }
            internal override bool ApplyRepeating(SimObject.Stats stats){
             //Log.DebugMessage("OnHitGracePeriodEffect:ApplyRepeating");
             if(onHitResetMotionGracePeriod>0f){
              //Log.DebugMessage("onHitResetMotionGracePeriod=="+onHitResetMotionGracePeriod);
              onHitResetMotionGracePeriod-=deltaTime;
             }
             if(onHitSetMotionGracePeriod>0f){
              //Log.DebugMessage("onHitSetMotionGracePeriod=="+onHitSetMotionGracePeriod);
              onHitSetMotionGracePeriod-=deltaTime;
             }
             if(onHitSetMotionVulnerablePeriod>0f){
              //Log.DebugMessage("onHitSetMotionVulnerablePeriod=="+onHitSetMotionVulnerablePeriod);
              onHitSetMotionVulnerablePeriod-=deltaTime;
              if(onHitSetMotionVulnerablePeriod<=0f){
               onHitSetMotionGracePeriod=onHitSetMotionGracePeriodDuration;
               Log.DebugMessage("onHitSetMotionGracePeriod="+onHitSetMotionGracePeriod);
              }
             }
             return false;
            }
            internal override void Unapply(SimObject.Stats stats){
             onHitSetMotionVulnerablePeriod=0f;
              onHitResetMotionGracePeriod=0f;
               onHitSetMotionGracePeriod=0f;
            }
        }
        internal override void OnApply(bool gameExiting=false){
         onHitGracePeriodEffect.deltaTime=this.deltaTime;
         if(!applied){
          //  apply buff effects here
          onHitGracePeriodEffect.onHitSetMotionVulnerablePeriodDuration=onHitSetMotionVulnerablePeriodDuration;
           onHitGracePeriodEffect.onHitResetMotionGracePeriodDuration=onHitResetMotionGracePeriodDuration;
            onHitGracePeriodEffect.onHitSetMotionGracePeriodDuration=onHitSetMotionGracePeriodDuration;
         }
         base.OnApply(gameExiting);
        }
    }
}