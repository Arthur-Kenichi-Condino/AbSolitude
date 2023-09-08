#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class CallHomunculus:CallSlaveSkill{
     (GameObject skillGameObject,Teleport skill)?teleport;
        internal override void OnSpawned(){
         base.OnSpawned();
         (GameObject skillGameObject,Skill skill)spawnedSkill=SkillsManager.singleton.SpawnSkillGameObject(typeof(Teleport),level,actor);
         teleport=(spawnedSkill.skillGameObject,(Teleport)spawnedSkill.skill);
        }
        internal override void OnPool(){
         if(teleport!=null){
          SkillsManager.singleton.Pool(teleport.Value.skill.GetType(),teleport.Value.skill);
          teleport=null;
         }
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
     readonly SpawnData spawnData=new SpawnData();
        internal override bool DoSkill(SimObject target,int useLevel){
         if(base.DoSkill(target,useLevel)){
          //  do any other skill setting needed here
          SetHomunToBeGenerated(actor,spawnData);
          return true;
         }
         //  the skill cannot be used!
         return false;
        }
        protected override void Invoke(){
         //  do more skill initialization here / or use this as main call of the skill
         spawnData.dequeued=false;
         SimObjectSpawner.singleton.OnSpecificSpawnRequestAt(spawnData);
         base.Invoke();//  the invoked flag is set here
        }
        protected override void OnInvokeSetCooldown(){
         //Log.DebugMessage("SpiritualHealing cooldown:"+cooldown);
         base.OnInvokeSetCooldown();
        }
        protected override void Revoke(){
         //  do deinitialization here, and clear important variables
         if(!spawnData.dequeued){
          //  (can't cancel because spawner is processing the data)
          return;//  this skill cannot be cancelled
         }
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
        protected override void OnInvoked(){
         if(spawnData.dequeued){//  spawner generated the homunculi
          //  do any other tests here
          base.OnInvoked();
         }
        }
    }
}