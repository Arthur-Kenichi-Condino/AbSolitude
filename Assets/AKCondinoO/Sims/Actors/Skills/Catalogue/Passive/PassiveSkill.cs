#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class PassiveSkill:Skill{
     internal const float passiveAddBuffCooldown=1.0f;
     protected readonly List<Type>buffsToApply=new List<Type>();
        internal override float GetOnInvokeCooldown(){
         return passiveAddBuffCooldown;
        }
        protected override void Update(){
         OnUpdate();
        }
        protected override void OnUpdate(){
        }
    }
}