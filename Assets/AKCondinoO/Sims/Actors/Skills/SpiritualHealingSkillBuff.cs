using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal class SpiritualHealingSkillBuff:SkillBuff{
        internal SpiritualHealingSkillBuff():base(){
        }
     internal float socialMotiveRecoveryPercent;
     internal float sanityStatRecoveryFlatValue;
     internal float  focusStatRecoveryFlatValue;
        internal override void OnApply(){
         if(!applied){
          //  apply buff effects here
         }
         base.OnApply();
        }
    }
}