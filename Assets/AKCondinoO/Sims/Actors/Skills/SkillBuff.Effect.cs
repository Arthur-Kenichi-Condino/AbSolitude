#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal partial class SkillBuff{
     protected Effect effect;
        internal abstract class Effect{
            internal abstract void Apply(SimObject.Stats stats);
            internal abstract bool ApplyRepeating(SimObject.Stats stats);
            internal abstract void Unapply(SimObject.Stats stats);
        }
    }
}