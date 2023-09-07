#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     protected Skill MySkill=null;internal Skill skillToUse{get{return MySkill;}}
      internal readonly HashSet<Skill>skillsToUse=new HashSet<Skill>();
       readonly HashSet<Skill>skillsToUseIterator=new HashSet<Skill>();
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
           }else{
            if(this.skills.TryGetValue(typeof(CallHomunculus),out Skill skill)){
             Log.DebugMessage("GetBest:CallHomunculus");
             skills.Add(skill);
            }
           }
           break;
          }
         }
        }
        protected virtual void SetBestSkillToUse(Skill.SkillUseContext context,bool fromDerived=false){
         //  TO DO: skillsToUse.Clear() ao trocar de estado da AI ou em situações específicas, como depois de um delay
         if(!fromDerived){
          if(MySkill==null&&skillsToUse.Count<=0){
           GetBest(context,skillsToUse);
          }
         }
         if(MySkill==null){
          skillsToUseIterator.Clear();
          skillsToUseIterator.UnionWith(skillsToUse);
          foreach(Skill skill in skillsToUseIterator){
           Type skillType=skill.GetType();
           if(skills.TryGetValue(skillType,out Skill skillToGet)&&skillToGet==skill){
            SimObject target=this;//  TO DO: set best my skill target
            if(skill.IsAvailable(target,skill.level)){
             if(ReflectionUtil.IsTypeDerivedFrom(skillType,typeof(GenerateHomunculus))){
              if(requiredSlaves.Count>0){
               MySkill=skill;
               Log.DebugMessage("check skillsToUse.Count:"+skillsToUse.Count+";should use:"+skillType);
              }
             }else{
              MySkill=skill;
              Log.DebugMessage("check skillsToUse.Count:"+skillsToUse.Count+";should use:"+skillType);
             }
            }
            skillsToUse.Remove(skill);
           }
           if(MySkill!=null){
            break;
           }
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