#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Homunculi.Vanilmirth{
    internal partial class VanilmirthAI{
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
        protected override void SetBestSkillToUse(Skill.SkillUseContext context,bool fromDerived=false){
         if(!fromDerived){
          if(MySkill==null&&skillsToUse.Count<=0){
           GetBest(context,skillsToUse);
          }
         }
         if(MySkill==null){
          if(skills.TryGetValue(typeof(ChaoticBlessing),out Skill skillToGet)&&skillsToUse.TryGetValue(skillToGet,out Skill skill)){
           //  TO DO: if its a special skill, then do special stuff too, also priority
           ChaoticBlessing chaoticBlessingSkill=(ChaoticBlessing)skill;
           SimObject target=this;//  TO DO: set best my skill target
           if(chaoticBlessingSkill.IsAvailable(target,chaoticBlessingSkill.level)){
            MySkill=chaoticBlessingSkill;
            Log.DebugMessage("check skillsToUse.Count:"+skillsToUse.Count+";should use:"+skill.GetType());
           }
           skillsToUse.Remove(skill);
          }
         }
         base.SetBestSkillToUse(context,true);
        }
    }
}