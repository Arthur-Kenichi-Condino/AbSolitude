#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using AKCondinoO.Sims.Actors.Skills.SkillVisualEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal partial class QuagmireSkillAoE:SkillAoE{
        protected override void SpawnSkillVFXs(){
         (GameObject skillVisualEffectGameObject,SkillVisualEffect skillVisualEffect)skillVFX=SkillVisualEffectsManager.singleton.SpawnSkillVisualEffectGameObject(typeof(QuagmireSkillVisualEffect),skill,this);
         if(!skillVFXs.TryGetValue(skillVFX.skillVisualEffect.GetType(),out List<SkillVisualEffect>skillVFXsOfType)){
          skillVFXs.Add(skillVFX.skillVisualEffect.GetType(),skillVFXsOfType=new List<SkillVisualEffect>());
         }
         Log.DebugMessage("skillVFXs.Count:"+skillVFXs.Count);
         skillVFXsOfType.Add(skillVFX.skillVisualEffect);
         Log.DebugMessage("skillVFXsOfType.Count:"+skillVFXsOfType.Count);
         skillVFX.skillVisualEffect.ActivateAt(targetPos.Value,null,0f,1);
        }
    }
}