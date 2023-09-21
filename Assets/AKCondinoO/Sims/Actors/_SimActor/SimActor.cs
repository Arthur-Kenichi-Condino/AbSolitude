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
     internal SimActorCharacterController characterController;
      internal float height;
       internal float heightCrouching;
     internal SimActorAnimatorController animatorController;
     internal AISensor aiSensor;
        protected override void Awake(){
         if(simUMAPrefab!=null){
          simUMAPosOffset=simUMAPrefab.transform.localPosition;
          simUMA=Instantiate(simUMAPrefab,this.transform).GetComponentInChildren<DynamicCharacterAvatar>();
          Log.DebugMessage("simUMAPosOffset:"+simUMAPosOffset);
          simUMA.CharacterUpdated.AddAction(OnUMACharacterUpdated);
         }
         base.Awake();
        }
     internal readonly Dictionary<string,Transform>nameToBodyPart=new Dictionary<string,Transform>();
     protected bool canSense;
        protected virtual void OnUMACharacterUpdated(UMAData simUMAData){
         if(aiSensor){
          if(head||leftEye||rightEye){
           Log.DebugMessage("aiSensor found, sync with actor's \"head's\" and/or \"eyes'\" transforms for providing eyesight to AI");
           canSense=true;
          }
         }
         OnCreateHitHurtBoxes(simUMA,simUMAData);
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
        public override void OnDestroy(){
         if(simUMA!=null){
          DestroyImmediate(simUMA.gameObject);
         }
         base.OnDestroy();
        }
        internal override void OnLoadingPool(){
         base.OnLoadingPool();
        }
     internal readonly Dictionary<Type,SkillData>requiredSkills=new Dictionary<Type,SkillData>();
      internal readonly Dictionary<Type,Skill>skills=new Dictionary<Type,Skill>();
     internal readonly Dictionary<Type,List<SlaveData>>requiredSlaves=new Dictionary<Type,List<SlaveData>>();
      internal readonly HashSet<(Type simObjectType,ulong idNumber)>slaves=new HashSet<(Type,ulong)>();
        internal override void OnActivated(){
         base.OnActivated();
         requiredSkills.Add(typeof(OnHitGracePeriod),new SkillData(){skill=typeof(OnHitGracePeriod),level=10,});
         lastForward=transform.forward;
         //  load skills from file here:
         persistentSimActorData.skills.Reset();
         while(persistentSimActorData.skills.MoveNext()){
          SkillData skillData=persistentSimActorData.skills.Current;
          if(!ReflectionUtil.IsTypeDerivedFrom(skillData.skill,typeof(Skill))){
           Log.Warning("invalid skill type:"+skillData.skill);
           continue;
          }
          (GameObject skillGameObject,Skill skill)spawnedSkill=SkillsManager.singleton.SpawnSkillGameObject(skillData.skill,skillData.level,this);
          skills.Add(skillData.skill,spawnedSkill.skill);
         }
         foreach(var skill in skills){
          if(requiredSkills.TryGetValue(skill.Key,out SkillData requiredSkill)){
           if(skill.Value.level<requiredSkill.level){
            skill.Value.level=requiredSkill.level;
           }
          }
          requiredSkills.Remove(skill.Key);
         }
         if(requiredSkills.Count>0){
          Log.DebugMessage("required skills missing");
          foreach(var requiredSkill in requiredSkills){
           if(!ReflectionUtil.IsTypeDerivedFrom(requiredSkill.Key,typeof(Skill))){
            Log.Warning("invalid skill type:"+requiredSkill.Key);
            continue;
           }
           (GameObject skillGameObject,Skill skill)spawnedSkill=SkillsManager.singleton.SpawnSkillGameObject(requiredSkill.Key,requiredSkill.Value.level,this);
           skills.Add(requiredSkill.Key,spawnedSkill.skill);
          }
         }
         requiredSkills.Clear();
         slaves.Clear();
         //  load slaves from file here:
         persistentSimActorData.slaves.Reset();
         while(persistentSimActorData.slaves.MoveNext()){
          SlaveData slaveData=persistentSimActorData.slaves.Current;
          slaves.Add((slaveData.simObjectType,slaveData.idNumber));
         }
         foreach(var slave in slaves){
          if(requiredSlaves.TryGetValue(slave.simObjectType,out List<SlaveData>requiredSlavesForType)){
           //  TO DO: do some checks and set variables here
           requiredSlaves.Remove(slave.simObjectType);
          }
         }
         persistentSimActorData.UpdateData(this);
        }
        internal override void OnDeactivated(){
         Log.DebugMessage("sim actor:OnDeactivated:id:"+id);
         if(aiSensor){
          aiSensor.Deactivate();
         }
         foreach(var skill in skills){
          SkillsManager.singleton.Pool(skill.Key,skill.Value);
         }
         skills.Clear();//  to do: pool skills before clearing the list
         base.OnDeactivated();
        }
        protected override void SetSlave(SimObject slave){
         slaves.Add(slave.id.Value);
         base.SetSlave(slave);
         persistentSimActorData.UpdateData(this);
        }
        protected override void EnableInteractions(){
         interactionsEnabled=true;
        }
        protected override void DisableInteractions(){
         interactionsEnabled=false;
        }
        internal void OnThirdPersonCamFollow(){
         Log.DebugMessage("OnThirdPersonCamFollow()");
         MainCamera.singleton.toFollowActor=this;
         GameMode.singleton.OnGameModeChangeTo(GameModesEnum.ThirdPerson);
        }
     internal bool isUsingAI=true;
     protected Vector3 lastForward=Vector3.forward;
     internal bool crouching{
      get{
       return crouching_v;
      }
     }protected bool crouching_v=false;
        protected virtual void OnToggleCrouching(){
         if(height>heightCrouching){//  can crouch
          if(!crouching_v){
           crouching_v=true;
           characterController.character.height=heightCrouching;
           characterController.character.center=new Vector3(0,-((height/2f)-(heightCrouching/2f)),0);
          }else{
           crouching_v=false;
           characterController.character.height=height;
           characterController.character.center=new Vector3(0,0,0);
          }
         }
        }
        internal Vector3 GetHeadPosition(bool fromAnimator){
         Vector3 headPos;
         if(fromAnimator&&animatorController!=null&&animatorController.animator!=null){
          headPos=animatorController.animator.transform.position+animatorController.animator.transform.rotation*(new Vector3(0f,characterController.character.height/2f+characterController.character.radius,0f)+characterController.headOffset);
         }else{
          headPos=characterController.character.transform.position+characterController.character.transform.rotation*characterController.headOffset;
         }
         return headPos;
        }
     protected Collider[]collidersTouchingUpper=new Collider[8];
      protected int collidersTouchingUpperCount=0;
     internal SimCollisionsChildTrigger simCollisionsTouchingUpper;
     protected Collider[]collidersTouchingMiddle=new Collider[8];
      protected int collidersTouchingMiddleCount=0;
     internal SimCollisionsChildTrigger simCollisionsTouchingMiddle;
        protected override void GetCollidersTouchingNonAlloc(bool instantCheck){
         gotCollidersTouchingFromInstantCheck=false;
         if(!instantCheck){
          if(
           simCollisions&&
           simCollisionsTouchingUpper &&
           simCollisionsTouchingMiddle
          ){
           return;
          }
         }
         if(characterController!=null){
          gotCollidersTouchingFromInstantCheck=true;
          var upperMiddleLowerValues=simCollisions.GetCapsuleValuesForUpperMiddleLowerCollisionTesting(characterController.character,transform.root,height,characterController.center);
          if(upperMiddleLowerValues!=null){
           var point0=upperMiddleLowerValues.Value.upperValues.point0;
           var point1=upperMiddleLowerValues.Value.upperValues.point1;
           float radius=upperMiddleLowerValues.Value.upperValues.radius;
           _GetUpperColliders:{
            collidersTouchingUpperCount=Physics.OverlapCapsuleNonAlloc(
             point0,
             point1,
             radius,
             collidersTouchingUpper
            );
           }
           if(collidersTouchingUpperCount>0){
            if(collidersTouchingUpperCount>=collidersTouchingUpper.Length){
             Array.Resize(ref collidersTouchingUpper,collidersTouchingUpperCount*2);
             goto _GetUpperColliders;
            }
           }
           point0=upperMiddleLowerValues.Value.middleValues.point0;
           point1=upperMiddleLowerValues.Value.middleValues.point1;
           _GetMiddleColliders:{
            collidersTouchingMiddleCount=Physics.OverlapCapsuleNonAlloc(
             point0,
             point1,
             radius,
             collidersTouchingMiddle
            );
           }
           if(collidersTouchingMiddleCount>0){
            if(collidersTouchingMiddleCount>=collidersTouchingMiddle.Length){
             Array.Resize(ref collidersTouchingMiddle,collidersTouchingMiddleCount*2);
             goto _GetMiddleColliders;
            }
           }
          }
         }
        }
    }
}