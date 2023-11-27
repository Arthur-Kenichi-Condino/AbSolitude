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
    internal class AreaSkill:Skill{
     internal Vector3?targetPos;
        protected override void Awake(){
        }
        internal override void OnSpawned(){
        }
        internal override void OnPool(){
         cooldown=0f;
        }
        internal override bool IsAvailable(SimObject target,int useLevel){
         if(base.IsAvailable(target,useLevel)){
          //  do more tests here
          if(targetPos==null){
           if(target!=null){
            targetPos=target.transform.position;
           }else{
            return false;
           }
          }
          if(Vector3.Distance(actor.transform.position,targetPos.Value)>range){
           return false;
          }
          return true;
         }
         //  oops, it's not the time to use the skill, and no more tests required
         return false;
        }
        internal override bool DoSkill(SimObject target,int useLevel){
         if(base.DoSkill(target,useLevel)){
          return true;
         }
         //  the skill cannot be used!
         return false;
        }
        protected override void Invoke(){
         //  do more skill initialization here / or use this as main call of the skill
         base.Invoke();//  the invoked flag is set here
        }
        protected override void OnInvokeSetCooldown(){
         //Log.DebugMessage("AreaSkill cooldown:"+cooldown);
         base.OnInvokeSetCooldown();
        }
        protected override void Revoke(){
         //  do deinitialization here, and clear important variables
         targetPos=null;
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
         base.OnInvoked();
         targetPos=null;
        }
    }
}