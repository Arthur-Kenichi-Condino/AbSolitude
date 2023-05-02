#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class ChaoticBlessing:SkillBase{
        internal readonly System.Random dice=new System.Random();
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
          GetRandomTarget();
          return true;
         }
         //  the skill cannot be used!
         return false;
        }
     SimActor simActorTarget;
     SimObject simObjectTarget;
        internal void GetRandomTarget(){
         simActorTarget=actor;
         simObjectTarget=null;
         float random=Mathf.Clamp01((float)dice.NextDouble());
         if(random<0.25f){
          simObjectTarget=SimObjectManager.singleton.active.ElementAt(dice.Next(0,SimObjectManager.singleton.active.Count)).Value;
         }else if(random<0.50f){
          simActorTarget=SimObjectManager.singleton.activeActor.ElementAt(dice.Next(0,SimObjectManager.singleton.activeActor.Count)).Value;
         }else if(random<0.75f&&actor.master!=null&&SimObjectManager.singleton.active.TryGetValue(actor.master.Value,out SimObject master)){
          if(master is SimActor simActorMaster){
           simActorTarget=simActorMaster;
          }else{
           simObjectTarget=master;
          }
         }
         Log.DebugMessage("ChaoticBlessing GetRandomTarget:"+(simObjectTarget!=null?simObjectTarget:simActorTarget));
        }
        protected override void Invoke(){
         //  do more skill initialization here / or use this as main call of the skill
         //
         base.Invoke();//  the invoked flag is set here
        }
        protected override void Revoke(){
         //  do deinitialization here, and clear important variables
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
           done=true;
           doing=false;
          }
         }
        }
    }
}