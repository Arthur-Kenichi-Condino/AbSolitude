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
      internal readonly HashSet<Skill>skillsToUse=new HashSet<Skill>();
       readonly HashSet<Skill>skillsToUseIterator=new HashSet<Skill>();
        internal virtual void GetBest(Skill.SkillUseContext context,HashSet<Skill>skills){
         switch(context){
          case Skill.SkillUseContext.OnFollow:{
           break;
          }
          case Skill.SkillUseContext.OnIdle:{
           break;
          }
          case Skill.SkillUseContext.OnTookDamage:{
           break;
          }
          case Skill.SkillUseContext.OnWillTakeDamage:{
           if(this.skills.TryGetValue(typeof(OnHitGracePeriod ),out Skill skill)){
            //Log.DebugMessage("GetBest:OnHitGracePeriod" );
            skills.Add(skill);
           }
           if(this.skills.TryGetValue(typeof(AskHelpFromAllies),out       skill)){
            //Log.DebugMessage("GetBest:AskHelpFromAllies");
            skills.Add(skill);
           }
           break;
          }
          case Skill.SkillUseContext.OnCallSlaves:{
           if(requiredSlaves.Count>0){
            if(this.skills.TryGetValue(typeof(GenerateHomunculus),out Skill skill)){
             skills.Add(skill);
            }
           }else{
            if(this.skills.TryGetValue(typeof(CallHomunculus    ),out Skill skill)){
             //Log.DebugMessage("GetBest:CallHomunculus");
             skills.Add(skill);
            }
           }
           break;
          }
         }
        }
        protected virtual void SetBestSkillToUse(Skill.SkillUseContext context,bool fromDerived=false){
         if(ai==null){
          return;
         }
         //  TO DO: skillsToUse.Clear() ao trocar de estado da AI ou em situações específicas, como depois de um delay
         if(!fromDerived){
          if(ai.MySkill==null&&skillsToUse.Count<=0){
           GetBest(context,skillsToUse);
          }
         }
         if(ai.MySkill==null){
          skillsToUseIterator.Clear();
          skillsToUseIterator.UnionWith(skillsToUse);
          foreach(Skill skill in skillsToUseIterator){
           Type skillType=skill.GetType();
           if(skills.TryGetValue(skillType,out Skill skillToGet)&&skillToGet==skill){
            SimObject target=this;//  TO DO: set best my skill target
            if(skill.IsAvailable(target,skill.level)){
             if(ReflectionUtil.IsTypeDerivedFrom(skillType,typeof(GenerateHomunculus))){
              if(requiredSlaves.Count>0){
               ai.MySkill=skill;
               Log.DebugMessage("check skillsToUse.Count:"+skillsToUse.Count+";should use:"+skillType);
              }
             }else{
              ai.MySkill=skill;
              Log.DebugMessage("check skillsToUse.Count:"+skillsToUse.Count+";should use:"+skillType);
             }
            }
            skillsToUse.Remove(skill);
           }
           if(ai.MySkill!=null){
            break;
           }
          }
         }
         if(ai.MySkill!=null){
          return;
         }
         if(ai.MySkill==null){
          skillsToUse.Clear();
         }
        }
    }
}