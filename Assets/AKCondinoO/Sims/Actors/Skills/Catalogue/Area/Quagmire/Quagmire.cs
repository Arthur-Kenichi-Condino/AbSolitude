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
        protected override void Invoke(){
         //  do more skill initialization here / or use this as main call of the skill
         (GameObject skillVisualEffectGameObject,SkillVisualEffect skillVisualEffect)skillVFX=SkillVisualEffectsManager.singleton.SpawnSkillVisualEffectGameObject(typeof(QuagmireSkillVisualEffect),this);
         skillVFX.skillVisualEffect.ActivateAt(targetPos.Value,null,0f,1);
         base.Invoke();//  the invoked flag is set here
        }
    }
}