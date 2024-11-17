#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal override void OnSkillUsed(Skill skill,bool done,bool revoked){
         base.OnSkillUsed(skill,done,revoked);
         //Log.DebugMessage("OnSkillUsed:"+skill);
         if(ai==null){
          return;
         }
         if(ai.MySkill==skill){
          //Log.DebugMessage("OnSkillUsed:MySkill==skill:clear used skill");
          ai.MySkill=null;
          if(revoked){
          }
          if(done){
          }
         }
        }
    }
}