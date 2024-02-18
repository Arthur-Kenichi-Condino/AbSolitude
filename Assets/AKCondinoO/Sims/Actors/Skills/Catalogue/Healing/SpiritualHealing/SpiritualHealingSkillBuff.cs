#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal class SpiritualHealingSkillBuff:SkillBuff{
        internal SpiritualHealingSkillBuff():base(){
         Log.DebugMessage("SpiritualHealingEffect ctor");
         spiritualHealingEffect=new SpiritualHealingEffect(this);
        }
     internal SpiritualHealingEffect spiritualHealingEffect{
      get{
       return(SpiritualHealingEffect)effect;
      }
      private set{
       effect=value;
      }
     }
        internal class SpiritualHealingEffect:Effect{
            internal SpiritualHealingEffect(SkillBuff buff):base(buff){
            }
         internal float socialMotiveRecoveryPercent;
         internal float sanityStatRecoveryFlatValue;
         internal float  focusStatRecoveryFlatValue;
            internal override void Apply(SimObject.Stats stats){
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