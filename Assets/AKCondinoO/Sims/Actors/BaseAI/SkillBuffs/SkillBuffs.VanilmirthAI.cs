#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Homunculi.Vanilmirth{
    internal partial class VanilmirthAI{
        protected override void DoSkill(){
         if(MySkill is ChaoticBlessing chaoticBlessingSkill){
          chaoticBlessingSkill.DoSkill(this,chaoticBlessingSkill.level);
         }
         base.DoSkill();
        }
    }
}