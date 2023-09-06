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
        protected virtual void SetBestSkillToUse(){
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