#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
        internal virtual bool IsMonster(){
         return false;
        }
    }
}