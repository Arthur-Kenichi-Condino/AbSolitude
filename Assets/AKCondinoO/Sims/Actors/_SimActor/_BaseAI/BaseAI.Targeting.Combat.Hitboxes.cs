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
     [SerializeField]protected HitboxesPrefabsList hitboxesPrefabs;
     protected readonly List<Hitboxes>hitboxes=new List<Hitboxes>();
     internal readonly Dictionary<string,Hitboxes>nameToHitboxes=new Dictionary<string,Hitboxes>();
        protected virtual void OnCreateHitboxes(DynamicCharacterAvatar simUMA,UMAData simUMAData){
         foreach(Hitboxes hitbox in hitboxes){
          if(hitbox!=null){
           DestroyImmediate(hitbox);
          }
         }
         hitboxes.Clear();
         nameToHitboxes.Clear();
         if(hitboxesPrefabs!=null&&hitboxesPrefabs.prefabs.Length>0){
          foreach(Hitboxes hitboxPrefab in hitboxesPrefabs.prefabs){
           if(nameToBodyPart.TryGetValue(hitboxPrefab.name,out Transform bodyPart)){
            //Log.DebugMessage("create Hitbox from hitboxPrefab.name:"+hitboxPrefab.name);
            Vector3 offset=hitboxPrefab.transform.localPosition;
            Quaternion rotation=hitboxPrefab.transform.localRotation;
            Hitboxes hitbox=Instantiate(hitboxPrefab);
            hitbox.transform.SetParent(bodyPart,false);
            offset=Vector3.Scale(offset,hitbox.transform.lossyScale);
            hitbox.transform.localPosition=offset;
            hitbox.transform.localRotation=rotation;
            hitbox.kinematicRigidbody=hitbox.gameObject.AddComponent<Rigidbody>();
            hitbox.kinematicRigidbody.isKinematic=true;
            hitbox.actor=this;
            hitboxes.Add(hitbox);
            nameToHitboxes.Add(hitboxPrefab.name,hitbox);
           }
          }
         }
        }
     internal readonly Dictionary<Hitboxes,float>hitGracePeriod=new Dictionary<Hitboxes,float>();
      readonly List<Hitboxes>hits=new List<Hitboxes>();
        internal void UpdateHitboxesGracePeriod(){
         hits.AddRange(hitGracePeriod.Keys);
         foreach(Hitboxes hit in hits){
          float gracePeriod=hitGracePeriod[hit];
          gracePeriod-=Time.deltaTime;
          if(gracePeriod<=0f){
           hitGracePeriod.Remove(hit);
          }else{
           hitGracePeriod[hit]=gracePeriod;
          }
         }
         hits.Clear();
        }
    }
}