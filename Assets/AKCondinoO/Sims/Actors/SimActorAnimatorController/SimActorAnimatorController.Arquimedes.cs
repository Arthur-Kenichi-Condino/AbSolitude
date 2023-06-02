#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActorAnimatorController{
        internal void UpdateArquimedesAIAnimatorWeaponLayer(BaseAI baseAI,ArquimedesAI arquimedesAI){
         UpdateVanilmirthAIAnimatorWeaponLayer(baseAI,arquimedesAI);
        }
        internal void UpdateArquimedesAIAnimatorMotionValue(BaseAI baseAI,ArquimedesAI arquimedesAI){
         UpdateVanilmirthAIAnimatorMotionValue(baseAI,arquimedesAI);
        }
    }
}