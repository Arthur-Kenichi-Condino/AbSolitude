#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class AbSolitude:PassiveSkill{
        protected override void Awake(){
         buffsToApply.Add(typeof(AbSolitudeSkillBuff));
         base.Awake();
        }
        protected override void Invoke(){
         Log.DebugMessage("AbSolitude:Invoke()");
         base.Invoke();
        }
    }
}