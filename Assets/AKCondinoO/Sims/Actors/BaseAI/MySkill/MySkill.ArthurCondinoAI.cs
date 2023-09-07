#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino{
    internal partial class ArthurCondinoAI{
        protected override void SetBestSkillToUse(Skill.SkillUseContext context){
         if(MySkill==null&&skillsToUse.Count<=0){
          GetBest(context,skillsToUse);
         }
         if(MySkill==null){
          if(skills.TryGetValue(typeof(GenerateHomunculus),out Skill skillToGet)&&skillsToUse.TryGetValue(skillToGet,out Skill skill)){
           GenerateHomunculus generateHomunculusSkill=(GenerateHomunculus)skill;
           if(generateHomunculusSkill.IsAvailable(this,generateHomunculusSkill.level)){
            if(requiredSlaves.Count>0){//  should Arthur generate his "homunculi friends" now?
             MySkill=generateHomunculusSkill;
             Log.DebugMessage("check skillsToUse.Count:"+skillsToUse.Count+";should use generateHomunculusSkill");
            }
           }
           skillsToUse.Remove(skill);
          }
         }
         if(MySkill!=null){
          return;
         }
         if(MySkill==null){
          skillsToUse.Clear();
         }
        }
    }
}