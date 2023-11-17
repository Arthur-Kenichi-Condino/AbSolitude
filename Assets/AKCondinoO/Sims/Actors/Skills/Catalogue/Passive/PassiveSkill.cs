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
    internal class PassiveSkill:Skill{
     internal const float passiveAddBuffCooldown=1.0f;
     protected readonly List<Type>buffsToApply=new List<Type>();
        protected override void Awake(){
        }
        internal override void OnSpawned(){
        }
        internal override void OnPool(){
         cooldown=0f;
        }
        internal override bool IsAvailable(SimObject target,int useLevel){
         if(!base.IsAvailable(target,useLevel)){
          return false;
         }
         return true;
        }
        internal override bool DoSkill(SimObject target,int useLevel){
         if(doing||done||revoked){
          return false;
         }
         if(!IsAvailable(target,useLevel)){
          return false;
         }
         Log.DebugMessage("DoSkill:"+this);
         this.useLevel=useLevel;
         this.target=target;
         doing=true;
         return true;
        }
        internal override bool DoSkillImmediate(SimObject target,int useLevel){
         bool result=DoSkill(target,useLevel);
         if(result){
          OnUpdate();
         }
         return result;
        }
        protected override void Invoke(){
         OnInvokeSetCooldown();
         invoked=true;
        }
        protected override void OnInvokeSetCooldown(){
         cooldown=GetOnInvokeCooldown();
        }
        internal override float GetOnInvokeCooldown(){
         return passiveAddBuffCooldown;
        }
        protected override void Revoke(){
         invoked=false;
         doing=false;
        }
        protected override void Update(){
         OnUpdate();
        }
        protected override void OnUpdate(){
         if(doing){
          if(!IsAvailable(target,useLevel)){
           Revoke();
           return;
          }
          if(cooldown>0f){
           cooldown-=Time.deltaTime;
          }
          if(cooldown<=0f){
           Invoke();
           OnInvoked();
          }
         }
        }
        protected override void OnInvoked(){
         Log.DebugMessage("skill "+this+" was cast gracefully");
        }
    }
}