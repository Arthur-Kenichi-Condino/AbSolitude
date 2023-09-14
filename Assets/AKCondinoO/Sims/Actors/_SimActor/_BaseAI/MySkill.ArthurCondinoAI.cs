#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino{
    internal partial class ArthurCondinoAI{
        protected override void SetBestSkillToUse(Skill.SkillUseContext context,bool fromDerived=false){
         if(!fromDerived){
          if(MySkill==null&&skillsToUse.Count<=0){
           GetBest(context,skillsToUse);
          }
         }
         base.SetBestSkillToUse(context,true);
        }
    }
}