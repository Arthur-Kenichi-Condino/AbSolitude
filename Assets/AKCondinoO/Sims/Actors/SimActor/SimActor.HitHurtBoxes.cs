#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActor{
        protected virtual void OnCreateHitHurtBoxes(DynamicCharacterAvatar simUMA,UMAData simUMAData){
        }
    }
}