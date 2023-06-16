#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     protected Skill MySkill=null;internal Skill skillToUse{get{return MySkill;}}
      internal readonly HashSet<Skill>skillsToUse=new HashSet<Skill>();
        protected virtual void OnIDLE_ST_SetMySkill(){
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
    }
}
namespace AKCondinoO.Sims.Actors.Homunculi.Vanilmirth{
    internal partial class VanilmirthAI{
        protected override void OnIDLE_ST_SetMySkill(){
         if(MySkill==null&&skillsToUse.Count<=0){
          Skill.GetBest(this,Skill.SkillUseContext.OnCallSlaves,skillsToUse);
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
         if(MySkill!=null){
          return;
         }
         if(MySkill==null&&skillsToUse.Count<=0){
          Skill.GetBest(this,Skill.SkillUseContext.OnIdle      ,skillsToUse);
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
    }
}
namespace AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino{
    internal partial class ArthurCondinoAI{
        protected override void OnIDLE_ST_SetMySkill(){
         if(MySkill==null&&skillsToUse.Count<=0){
          Skill.GetBest(this,Skill.SkillUseContext.OnCallSlaves,skillsToUse);
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