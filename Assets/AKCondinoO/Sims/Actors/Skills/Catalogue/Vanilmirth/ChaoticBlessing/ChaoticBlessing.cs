#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class ChaoticBlessing:Skill{
     internal readonly System.Random dice=new System.Random();
     internal readonly List<Type>possibleSkillTypes=new List<Type>(){
      typeof(SpiritualHealing),
     };
     internal readonly List<Skill>possibleSkills=new List<Skill>();
        protected override void Awake(){
         base.Awake();
         castDelay=3.5f;
        }
        internal override void OnSpawned(){
         base.OnSpawned();
         foreach(Type skillType in possibleSkillTypes){
          if(ReflectionUtil.IsTypeDerivedFrom(skillType,this.GetType())){
           Log.Warning("invalid skill type (prevent stack overflow):"+skillType);
           continue;
          }
          if(!ReflectionUtil.IsTypeDerivedFrom(skillType,typeof(Skill))){
           Log.Warning("invalid skill type:"+skillType);
           continue;
          }
          (GameObject skillGameObject,Skill skill)spawnedSkill=SkillsManager.singleton.SpawnSkillGameObject(skillType,level,actor);
          possibleSkills.Add(spawnedSkill.skill);
         }
        }
        internal override void OnPool(){
         foreach(Skill possibleSkill in possibleSkills){
          SkillsManager.singleton.Pool(possibleSkill.GetType(),possibleSkill);
         }
         possibleSkills.Clear();
         base.OnPool();
        }
        internal override bool IsAvailable(SimObject target,int useLevel){
         if(base.IsAvailable(target,useLevel)){
          //  do more tests here
          return true;
         }
         //  oops, it's not the time to use the skill, and no more tests required
         return false;
        }
        internal override bool DoSkill(SimObject target,int useLevel){
         if(base.DoSkill(target,useLevel)){
          //  do any other skill setting needed here
          GetRandomTarget();
          SimObject finalTarget;
          if(simObjectTarget!=null){
           finalTarget=simObjectTarget;
          }else{
           finalTarget=simActorTarget;
          }
          finalTarget.OnTargetedBySkill(this,actor);
          return true;
         }
         //  the skill cannot be used!
         return false;
        }
     SimActor   simActorTarget;
     SimObject simObjectTarget;
        internal void GetRandomTarget(){
         simActorTarget=actor;
         simObjectTarget=null;
         float random=Mathf.Clamp01((float)dice.NextDouble());
         if(random<0.25f){
          simObjectTarget=SimObjectManager.singleton.active.ElementAt(dice.Next(0,SimObjectManager.singleton.active.Count)).Value;
         }else if(random<0.50f){
          simActorTarget=SimObjectManager.singleton.activeActor.ElementAt(dice.Next(0,SimObjectManager.singleton.activeActor.Count)).Value;
         }else if(random<0.75f&&actor.masterId!=null&&SimObjectManager.singleton.active.TryGetValue(actor.masterId.Value,out SimObject masterSimObject)){
          if(masterSimObject is SimActor masterSimActor){
           simActorTarget=masterSimActor;
          }else{
           simObjectTarget=masterSimObject;
          }
         }
         Log.DebugMessage("ChaoticBlessing GetRandomTarget:"+(simObjectTarget!=null?simObjectTarget:simActorTarget));
        }
        protected override void Invoke(){
         //  do more skill initialization here / or use this as main call of the skill
         SimObject finalTarget;
         if(simObjectTarget!=null){
          finalTarget=simObjectTarget;
         }else{
          finalTarget=simActorTarget;
         }
         finalTarget.OnHitByTargetedSkill(this,actor);
         Skill randomSkillToUse=possibleSkills.ElementAt(dice.Next(0,possibleSkills.Count));
         randomSkillToUse.cooldown=0f;
         randomSkillToUse.DoSkill(finalTarget,useLevel);
         base.Invoke();//  the invoked flag is set here
        }
        protected override void OnInvokeSetCooldown(){
         //Log.DebugMessage("ChaoticBlessing cooldown:"+cooldown);
         base.OnInvokeSetCooldown();
        }
        internal override float GetOnInvokeCooldown(){
         float random=Mathf.Clamp01((float)dice.NextDouble());
         return castDelay+(random*useLevel);
        }
        protected override void Revoke(){
         //  do deinitialization here, and clear important variables
         base.Revoke();//  the revoked flag is set here
        }
        protected override void Update(){
         base.Update();
        }
        protected override void OnUpdate(){
         base.OnUpdate();
         if(doing){
          if(revoked){//  something went wrong
           return;
          }
          if(invoked){//  skill cast
           //  run more skill code here; set doing flag to false when finished
          }
         }
        }
    }
}