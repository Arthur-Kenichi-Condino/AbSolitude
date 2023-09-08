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
        internal class OnHitGracePeriodEffect:Effect{
            internal override void Apply(SimObject.Stats stats){
             Log.DebugMessage("OnHitGracePeriodEffect:Apply");
            }
            internal override bool ApplyRepeating(SimObject.Stats stats){
             return false;
            }
            internal override void Unapply(SimObject.Stats stats){
            }
        }
        internal override void OnApply(bool gameExiting=false){
         if(!applied){
          //  apply buff effects here
         }
         base.OnApply(gameExiting);
        }
    }
}