#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal partial class Skill:MonoBehaviour{
        public enum SkillUseContext{
         OnCallSlaves,
         OnWillTakeDamage,
         OnTookDamage,
         OnIdle,
        }
     /// <summary>
     ///  After-cast Delay
     /// </summary>
     internal float castDelay{
      get{
       return castDelay_value;
      }
      set{
       castDelay_value=value;
      }
     }
     protected float castDelay_value=1.0f;
     internal LinkedListNode<Skill>pooled=null;
     internal SimActor actor;
     internal int level=1;
        protected virtual void Awake(){
        }
        internal virtual void OnSpawned(){
        }
        internal virtual void OnPool(){
         cooldown=0f;
        }
     internal bool doing;
      internal int useLevel;
      internal SimObject target;
      internal bool invoked;
      internal bool revoked;
      internal bool done;
     internal float cooldown;
        internal virtual bool IsAvailable(SimObject target,int useLevel){
         if(actor==null||actor.id==null){
          return false;
         }
         if(target!=null&&target.id==null){
          return false;
         }
         if(!doing||!invoked){
          //  if the skill has not been cast yet then some other tests can still be done here, like focus points required to use the skill
          if(cooldown>0f){
           return false;
          }
         }
         return true;
        }
        /// <summary>
        ///  Set initialization to happen in Invoke() at Update()
        /// </summary>
        /// <param name="target"></param>
        /// <param name="useLevel"></param>
        /// <returns></returns>
        internal virtual bool DoSkill(SimObject target,int useLevel){
         if(doing||done||revoked){
          return false;
         }
         if(!IsAvailable(target,useLevel)){
          return false;
         }
         Log.DebugMessage("DoSkill:"+this);
         this.useLevel=useLevel;
         this.target=target;
         invoked=false;
         doing=true;
         return true;
        }
        internal virtual bool DoSkillImmediate(SimObject target,int useLevel){
         bool result=DoSkill(target,useLevel);
         OnUpdate();
         return result;
        }
        /// <summary>
        ///  Can be the "initialization" or skill "main call"
        /// </summary>
        protected virtual void Invoke(){
         OnInvokeSetCooldown();
         invoked=true;
        }
        protected virtual void OnInvokeSetCooldown(){
        }
        internal virtual float GetOnInvokeCooldown(){
         return castDelay;
        }
        /// <summary>
        ///  Something went wrong, so cancel
        /// </summary>
        protected virtual void Revoke(){
         revoked=true;
         doing=false;
        }
        protected virtual void Update(){
         OnUpdate();
        }
        protected virtual void OnUpdate(){
         if(!doing){
          if(revoked||done){
           if(actor!=null){
            actor.OnSkillUsed(this,done,revoked);
           }
           revoked=false;
           done=false;
          }
          if(cooldown>0f){
           cooldown-=Time.deltaTime;
          }
          return;
         }
         if(doing){
          if(!IsAvailable(target,useLevel)){
           Revoke();
           if(revoked){
            return;
           }
          }
          if(!invoked){
           Invoke();
          }
          if(invoked){//  skill cast
           //  run more skill code here; set doing flag to false when finished
           OnInvoked();
          }
         }
        }
        protected virtual void OnInvoked(){
         Log.DebugMessage("skill "+this+" was cast gracefully");
         done=true;
         doing=false;
        }
    }
}