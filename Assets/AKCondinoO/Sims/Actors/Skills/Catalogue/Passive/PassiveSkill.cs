#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class PassiveSkill:Skill{
     internal const float passiveAddBuffCooldown=10.0f;
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
         foreach(Type buffType in buffsToApply){
          if(!target.skillBuffs.Contains(buffType,out List<SkillBuff>buffs)||
           !buffs.Any(
            (buff)=>{
             return buff.skill==this;
            }
           )
          ){
           SkillBuff buff=SkillBuff.Dequeue(buffType);
           if(buff!=null){
            Log.DebugMessage("skill "+this+":add buff of type:"+buffType);
            buff.duration=passiveAddBuffCooldown*2f;
            buff.delay=0f;
            OnBuffDequeued(buff);
            target.skillBuffs.Add(buff,this);
           }
          }else{
           foreach(SkillBuff buff in buffs){
            if(buff.skill==this){
             Log.DebugMessage("skill "+this+":renew buff of type:"+buffType);
             buff.duration=buff.elapsedTime+passiveAddBuffCooldown*2f;
            }
           }
          }
         }
         OnInvokeSetCooldown();
         invoked=true;
        }
        protected virtual void OnBuffDequeued(SkillBuff buff){
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