#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActor{
     [SerializeField]protected HitboxesPrefabsList hitboxesPrefabs;
      [SerializeField]protected HurtboxesPrefabsList hurtboxesPrefabs;
     protected readonly List<Hitboxes>hitboxes=new List<Hitboxes>();
      protected readonly List<Hurtboxes>hurtboxes=new List<Hurtboxes>();
        protected virtual void OnCreateHitHurtBoxes(DynamicCharacterAvatar simUMA,UMAData simUMAData){
        }
    }
}