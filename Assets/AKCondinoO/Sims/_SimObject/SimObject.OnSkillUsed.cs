#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     [NonSerialized]internal SkillBuffEffectsState skillBuffs;
        internal virtual void OnSkillUsed(Skill skill,bool done,bool revoked){
        }
    }
}