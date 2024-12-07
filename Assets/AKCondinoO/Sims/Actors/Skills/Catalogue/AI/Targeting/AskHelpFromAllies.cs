#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AKCondinoO.Sims.Actors.BaseAI;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class AskHelpFromAllies:AICommandSkill{
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
          return true;
         }
         //  the skill cannot be used!
         return false;
        }
     [NonSerialized]readonly Collider[]findAlliesColliders=new Collider[8];
      QueryTriggerInteraction findAlliesQueryTrigger=QueryTriggerInteraction.Ignore;
        protected override void Invoke(){
         //  do more skill initialization here / or use this as main call of the skill
         Log.DebugMessage("AskHelpFromAllies:Invoke");
         if(actor!=null){
          if(actor.masterId!=null&&SimObjectManager.singleton.active.TryGetValue(actor.masterId.Value,out SimObject masterSimObject)){
           if(masterSimObject is BaseAI masterAI){
            masterAI.OnAllyAskingForHelp(actor,actor.enemy);
           }
          }
          if(actor.slaves.Count>0){
           foreach(var slaveId in actor.slaves){
            if(SimObjectManager.singleton.active.TryGetValue(slaveId,out SimObject slaveSimObject)){
             if(slaveSimObject is BaseAI slaveAI){
              slaveAI.OnAllyAskingForHelp(actor,actor.enemy);
             }
            }
           }
          }
          int mask=PhysUtil.simActorLayer;
          //int collidersCount=Physics.OverlapSphereNonAlloc(actor.transform.position,96,findAlliesColliders,mask,findAlliesQueryTrigger);
          //for(int i=0;i<collidersCount;++i){
          // Collider col=findAlliesColliders[i];
          // SimActor sim=col.transform.root.GetComponentInChildren<SimActor>();
          // if(sim is BaseAI ai){
          //  if(ai.aggression!=AggressionMode.AggressiveToAll){
          //   if(ai.id!=null&&actor.id!=null){
          //    if(ai.id.Value.simObjectType==actor.id.Value.simObjectType){
          //     ai.OnAllyAskingForHelp(actor,actor.enemy);
          //    }
          //   }
          //  }
          // }
          //}
         }
         base.Invoke();//  the invoked flag is set here
        }
        protected override void OnInvokeSetCooldown(){
         //Log.DebugMessage("ChaoticBlessing cooldown:"+cooldown);
         base.OnInvokeSetCooldown();
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