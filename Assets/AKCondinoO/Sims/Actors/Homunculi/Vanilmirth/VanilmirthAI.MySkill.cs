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
         if(ai==null){
          return;
         }
         if(ReflectionUtil.IsTypeDerivedFrom(ai.MySkill.GetType(),typeof(ChaoticBlessing))){
          //  TO DO: if its a special skill, then do special stuff too
          ChaoticBlessing chaoticBlessingSkill=(ChaoticBlessing)ai.MySkill;
          SimObject target=this;//  TO DO: use best my skill target
          chaoticBlessingSkill.DoSkill(target,chaoticBlessingSkill.level);
          return;
         }
         base.DoSkill();
        }
    }
}