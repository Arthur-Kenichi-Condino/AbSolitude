#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
        internal virtual void OnSkillUsed(Skill skill,bool done,bool revoked){
        }
     protected SkillBuffEffectsState skillBuffs;
        internal bool OnTargetedBySkill(Skill skill,SimObject caster){
         return false;
        }
        internal bool OnHitByTargetedSkill(Skill skill,SimObject caster){
         return false;
        }
    }
}