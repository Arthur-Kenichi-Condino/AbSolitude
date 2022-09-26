#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class GenerateHomunculus:CallSlaveSkill{
        internal override bool IsAvailable(BaseAI target,int useLevel){
         if(base.IsAvailable(target,useLevel)){
          //  do more tests here
          return true;
         }
         //  oops, it's not the time to use the skill, and no more tests required
         return false;
        }
        internal override bool DoSkill(BaseAI target,int useLevel){
         if(base.DoSkill(target,useLevel)){
          //  do any other skill setting needed here
          SetHomunToBeGenerated();
          return true;
         }
         //  the skill cannot be used!
         return false;
        }
     readonly SpawnData spawnData=new SpawnData();
        protected void SetHomunToBeGenerated(){
         spawnData.Clear();
         //  add data to spawn
         if(actor is ArthurCondinoAI arthurCondino){
          foreach(var requiredSlavesList in arthurCondino.requiredSlaves){
           Type requiredSlaveType=requiredSlavesList.Key;
           List<SlaveData>requiredSlavesOfType=requiredSlavesList.Value;
           foreach(SlaveData requiredSlaveOfType in requiredSlavesOfType){
            spawnData.at.Add((actor.transform.position,actor.transform.rotation.eulerAngles,Vector3.one,requiredSlaveType,null,new SimObject.PersistentData()));
            //  TO DO: fill SimActorPersistentData
            spawnData.masters[spawnData.at.Count-1]=actor.id.Value;
           }
          }
         }
         actor.requiredSlaves.Clear();
        }
        protected override void Invoke(){
         //  do more skill initialization here / or use to as main call of the skill
         spawnData.dequeued=false;
         SimObjectSpawner.singleton.OnSpecificSpawnRequestAt(spawnData);
         base.Invoke();//  the invoked flag is set here
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
         if(doing){
          if(revoked){//  something went wrong
           return;
          }
          if(invoked){//  skill cast
           //  run more skill code here; set doing flag to false when finished
           if(spawnData.dequeued){//  spawner generated the homunculi
            //  do any other tests here
            Log.DebugMessage("skill "+this+" was cast gracefully");
            done=true;
            doing=false;
           }
          }
         }
        }
 }
}