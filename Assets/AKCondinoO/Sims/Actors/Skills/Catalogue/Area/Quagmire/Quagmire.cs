#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using AKCondinoO.Sims.Actors.Skills.SkillVisualEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class Quagmire:AreaSkill{
        protected override void Awake(){
         maxAoECount=1;
        }
        protected override void Invoke(){
         //  do more skill initialization here / or use this as main call of the skill
         (GameObject skillAoEGameObject,SkillAoE skillAoE)aoe=SkillAoEManager.singleton.SpawnSkillAoEGameObject(typeof(QuagmireSkillAoE),this);
         activeAoE.Add(aoe.skillAoE);
         float duration=useLevel*5f;
         Log.DebugMessage("Quagmire duration:"+duration);
         aoeActiveCooldowns.Add(aoe.skillAoE,duration);
         Log.DebugMessage("activeAoE.Count:"+activeAoE.Count);
         aoe.skillAoE.ActivateAt(targetPos.Value,null,0f,1,duration);
         base.Invoke();//  the invoked flag is set here
        }
    }
}