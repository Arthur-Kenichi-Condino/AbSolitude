#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActor{
        internal override void OnSkillUsed(Skill skill,bool done,bool revoked){
         base.OnSkillUsed(skill,done,revoked);
        }
    }
}