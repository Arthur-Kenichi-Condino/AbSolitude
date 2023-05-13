using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal class SpiritualHealingSkillBuff:SkillBuff{
        internal SpiritualHealingSkillBuff():base(){
        }
        internal class SpiritualHealingEffect:Effect{
         internal float socialMotiveRecoveryPercent;
         internal float sanityStatRecoveryFlatValue;
         internal float  focusStatRecoveryFlatValue;
            internal override void Apply(){
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