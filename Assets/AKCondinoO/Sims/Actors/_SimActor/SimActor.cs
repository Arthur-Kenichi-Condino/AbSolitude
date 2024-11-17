#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.GameMode;
using static AKCondinoO.InputHandler;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal abstract partial class SimActor:SimObject{
     [SerializeField]GameObject simUMAPrefab;
     internal DynamicCharacterAvatar simUMA;
      [SerializeField]GameObject goToSimUMA;
      internal Vector3 simUMAPosOffset;
        protected override void Awake(){
         if(simUMAPrefab!=null){
          simUMAPosOffset=simUMAPrefab.transform.localPosition;
          simUMA=Instantiate(simUMAPrefab,this.transform).GetComponentInChildren<DynamicCharacterAvatar>();
          //Log.DebugMessage("simUMAPosOffset:"+simUMAPosOffset);
          simUMA.CharacterUpdated.AddAction(OnUMACharacterUpdated);
          goToSimUMA=simUMA.gameObject;
         }
         base.Awake();
        }
     internal readonly Dictionary<string,Transform>nameToBodyPart=new Dictionary<string,Transform>();
        protected virtual void OnUMACharacterUpdated(UMAData simUMAData){
        }
        public override void OnDestroy(){
         if(simUMA!=null){
          DestroyImmediate(simUMA.gameObject);
         }
         base.OnDestroy();
        }
        protected void SetBodyPart(string name,string transformChildName,out Transform bodyPart){
         if(!nameToBodyPart.TryGetValue(name,out bodyPart)||bodyPart==null){
          bodyPart=Util.FindChildRecursively(simUMA.transform,transformChildName);
          if(bodyPart!=null){
           nameToBodyPart[name]=bodyPart;
          }else{
           nameToBodyPart.Remove(name);
          }
         }
         Log.DebugMessage("SetBodyPart:"+name+":"+bodyPart);
        }
        internal override void OnLoadingPool(){
         base.OnLoadingPool();
        }
        internal override void OnActivated(){
         base.OnActivated();
        }
        internal override void OnDeactivated(){
         base.OnDeactivated();
        }
        protected override void EnableInteractions(){
         Log.Warning("implementation incomplete");
         interactionsEnabled=true;
        }
        protected override void DisableInteractions(){
         Log.Warning("implementation incomplete");
         interactionsEnabled=false;
        }
    }
}