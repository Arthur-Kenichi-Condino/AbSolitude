#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActorAnimatorController{
        internal void UpdateArthurCondinoAIAnimatorWeaponLayer(BaseAI baseAI,ArthurCondinoAI arthurCondinoAI){
         UpdateHumanAIAnimatorWeaponLayer(baseAI,arthurCondinoAI);
        }
        internal void UpdateArthurCondinoAIAnimatorMotionValue(BaseAI baseAI,ArthurCondinoAI arthurCondinoAI){
         UpdateHumanAIAnimatorMotionValue(baseAI,arthurCondinoAI);
        }
    }
}