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
           }
          }
         }
        }
    }
}