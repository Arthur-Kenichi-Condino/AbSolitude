#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     protected Skill MySkill=null;internal Skill skillToUse{get{return MySkill;}}
      internal readonly HashSet<Skill>skillsToUse=new HashSet<Skill>();
        protected virtual void SetBestSkillToUse(Skill.SkillUseContext context){
         //  TO DO: skillsToUse.Clear() ao trocar de estado da AI ou em situações específicas, como depois de um delay
         if(MySkill==null){
          //  TO DO: get other skills
         }
         if(MySkill!=null){
          return;
         }
         if(MySkill==null){
          skillsToUse.Clear();
         }
        }
        internal virtual void GetBest(Skill.SkillUseContext context,HashSet<Skill>skills){
         switch(context){
          case Skill.SkillUseContext.OnIdle:{
           break;
          }
          case Skill.SkillUseContext.OnTookDamage:{
           break;
          }
          case Skill.SkillUseContext.OnWillTakeDamage:{
           break;
          }
          case Skill.SkillUseContext.OnCallSlaves:{
           if(requiredSlaves.Count>0){
            if(this.skills.TryGetValue(typeof(GenerateHomunculus),out Skill skill)){
             skills.Add(skill);
            }
           }
           break;
          }
         }
        }
    }
}