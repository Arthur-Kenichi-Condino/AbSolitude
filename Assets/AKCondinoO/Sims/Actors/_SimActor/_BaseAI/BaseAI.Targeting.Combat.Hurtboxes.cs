#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.GameMode;
using static AKCondinoO.InputHandler;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     [SerializeField]protected HurtboxesPrefabsList hurtboxesPrefabs;
      protected readonly List<Hurtboxes>hurtboxes=new List<Hurtboxes>();
        protected virtual void OnCreateHurtboxes(DynamicCharacterAvatar simUMA,UMAData simUMAData){
         foreach(Hurtboxes hurtbox in hurtboxes){
          if(hurtbox!=null){
           DestroyImmediate(hurtbox);
          }
         }
         hurtboxes.Clear();
         if(hurtboxesPrefabs!=null&&hurtboxesPrefabs.prefabs.Length>0){
          foreach(Hurtboxes hurtboxPrefab in hurtboxesPrefabs.prefabs){
           if(nameToBodyPart.TryGetValue(hurtboxPrefab.name,out Transform bodyPart)){
            Log.DebugMessage("create Hurtbox from hurtboxPrefab.name:"+hurtboxPrefab.name);
            Vector3 offset=hurtboxPrefab.transform.localPosition;
            Quaternion rotation=hurtboxPrefab.transform.localRotation;
            Hurtboxes hurtbox=Instantiate(hurtboxPrefab);
            hurtbox.transform.SetParent(bodyPart,false);
            offset=Vector3.Scale(offset,hurtbox.transform.lossyScale);
            hurtbox.transform.localPosition=offset;
            hurtbox.transform.localRotation=rotation;
            hurtbox.kinematicRigidbody=hurtbox.gameObject.AddComponent<Rigidbody>();
            hurtbox.kinematicRigidbody.isKinematic=true;
            hurtbox.actor=this;
            hurtboxes.Add(hurtbox);
           }
          }
         }
        }
    }
}