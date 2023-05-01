#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal partial class Skill{
        static void VanilmirthGetBest(VanilmirthAI vanilmirth,SkillUseContext context,HashSet<Skill>skills){
         switch(context){
          case SkillUseContext.OnIdle:{
           if(vanilmirth.skills.TryGetValue(typeof(ChaoticBlessing),out Skill skill)){
            skills.Add(skill);
           }
           break;
          }
         }
        }
    }
}