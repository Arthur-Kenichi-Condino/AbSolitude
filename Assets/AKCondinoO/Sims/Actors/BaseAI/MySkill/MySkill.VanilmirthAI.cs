#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Homunculi.Vanilmirth{
    internal partial class VanilmirthAI{
        protected override void SetBestSkillToUse(Skill.SkillUseContext context){
         if(MySkill==null&&skillsToUse.Count<=0){
          GetBest(context,skillsToUse);
         }
         if(MySkill==null){
          if(skills.TryGetValue(typeof(GenerateHomunculus),out Skill skillToGet)&&skillsToUse.TryGetValue(skillToGet,out Skill skill)){
           GenerateHomunculus generateHomunculusSkill=(GenerateHomunculus)skill;
           if(generateHomunculusSkill.IsAvailable(this,generateHomunculusSkill.level)){
            if(requiredSlaves.Count>0){
             MySkill=generateHomunculusSkill;
             Log.DebugMessage("check skillsToUse.Count:"+skillsToUse.Count+";should use generateHomunculusSkill");
            }
           }
           skillsToUse.Remove(skill);
          }
         }
         if(MySkill==null){
          if(skills.TryGetValue(typeof(ChaoticBlessing),out Skill skillToGet)&&skillsToUse.TryGetValue(skillToGet,out Skill skill)){
           ChaoticBlessing chaoticBlessingSkill=(ChaoticBlessing)skill;
           if(chaoticBlessingSkill.IsAvailable(this,chaoticBlessingSkill.level)){
            MySkill=chaoticBlessingSkill;
            Log.DebugMessage("check skillsToUse.Count:"+skillsToUse.Count+";should use chaoticBlessingSkill");
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
        internal override void GetBest(Skill.SkillUseContext context,HashSet<Skill>skills){
         switch(context){
          case Skill.SkillUseContext.OnIdle:{
           if(this.skills.TryGetValue(typeof(ChaoticBlessing),out Skill skill)){
            skills.Add(skill);
           }
           break;
          }
         }
         base.GetBest(context,skills);
        }
    }
}