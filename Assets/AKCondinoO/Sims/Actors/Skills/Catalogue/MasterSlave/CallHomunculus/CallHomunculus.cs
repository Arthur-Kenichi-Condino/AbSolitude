#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class CallHomunculus:CallSlaveSkill{
     readonly List<(GameObject skillGameObject,Teleport skill)>teleportSkills=new();
        internal override void OnSpawned(){
         base.OnSpawned();
        }
        internal override void OnPool(){
         for(int i=0;i<teleportSkills.Count;++i){
          Skill skill=teleportSkills[i].skill;
          SkillsManager.singleton.Pool(skill.GetType(),skill);
         }
         teleportSkills.Clear();
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
     readonly List<SimObject>toTeleport=new List<SimObject>();
        internal override bool DoSkill(SimObject target,int useLevel){
         if(base.DoSkill(target,useLevel)){
          //  do any other skill setting needed here
          toTeleport.Clear();
          SetHomunToBeGenerated(actor,spawnData,toTeleport);
          for(int i=0;i<toTeleport.Count;++i){
           if(i>=teleportSkills.Count){
            (GameObject skillGameObject,Skill skill)spawnedSkill=SkillsManager.singleton.SpawnSkillGameObject(typeof(Teleport),level,actor);
            teleportSkills.Add((spawnedSkill.skillGameObject,(Teleport)spawnedSkill.skill));
           }
          }
          return true;
         }
         //  the skill cannot be used!
         return false;
        }
        protected override void Invoke(){
         //  do more skill initialization here / or use this as main call of the skill
         spawnData.dequeued=false;
         SimObjectSpawner.singleton.OnSpecificSpawnRequestAt(spawnData);
         for(int i=0;i<toTeleport.Count;++i){
          SimObject simObject=toTeleport[i];
          (GameObject skillGameObject,Teleport skill)teleport=teleportSkills[i];
          teleport.skill.targetDest=actor.transform.position;
          teleport.skill.cooldown=0f;
          teleport.skill.DoSkill(simObject,useLevel);
         }
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