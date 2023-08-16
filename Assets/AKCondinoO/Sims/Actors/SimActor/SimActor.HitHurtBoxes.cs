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
         foreach(Hitboxes hitbox in hitboxes){
          if(hitbox!=null){
           DestroyImmediate(hitbox);
          }
         }
         hitboxes.Clear();
         foreach(Hurtboxes hurtbox in hurtboxes){
          if(hurtbox!=null){
           DestroyImmediate(hurtbox);
          }
         }
         hurtboxes.Clear();
         if(hitboxesPrefabs!=null&&hitboxesPrefabs.prefabs.Length>0){
          foreach(Hitboxes hitboxPrefab in hitboxesPrefabs.prefabs){
           if(nameToBodyPart.TryGetValue(hitboxPrefab.name,out Transform bodyPart)){
            Log.DebugMessage("create Hitbox from hitboxPrefab.name:"+hitboxPrefab.name);
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
           }
          }
         }
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
     internal readonly Dictionary<Hitboxes,float>hitGracePeriod=new Dictionary<Hitboxes,float>();
      readonly List<Hitboxes>hits=new List<Hitboxes>();
        internal void HitHurtBoxesUpdate(){
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