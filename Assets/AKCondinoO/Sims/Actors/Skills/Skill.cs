#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class Skill:MonoBehaviour{
        public enum SkillUseContext{
         OnCallSlaves,
         OnWillTakeDamage,
         OnTookDamage,
         OnIdle,
        }
        internal static void GetBest(BaseAI actor,SkillUseContext context,HashSet<Skill>skills){
         switch(context){
          case SkillUseContext.OnCallSlaves:{
           if(actor.skills.TryGetValue(typeof(GenerateHomunculus),out Skill skill)){
            skills.Add(skill);
           }
           break;
          }
         }
        }
     internal SimActor actor;
     internal int level=1;
     internal bool doing;
      internal int useLevel;
      internal BaseAI target;
      internal bool invoked;
      internal bool revoked;
      internal bool done;
        internal virtual bool IsAvailable(BaseAI target,int useLevel){
         if(actor.id==null){
          return false;
         }
         if(target!=null&&target.id==null){
          return false;
         }
         if(!doing||!invoked){
          //  if the skill has not been cast yet then some other tests can still be done here, like focus points required to use the skill
         }
         return true;
        }
        /// <summary>
        ///  Set initialization to happen in Invoke() at Update()
        /// </summary>
        /// <param name="target"></param>
        /// <param name="useLevel"></param>
        /// <returns></returns>
        internal virtual bool DoSkill(BaseAI target,int useLevel){
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
        /// <summary>
        ///  Can be the "initialization" or skill "main call"
        /// </summary>
        protected virtual void Invoke(){
         invoked=true;
        }
        /// <summary>
        ///  Something went wrong, so cancel
        /// </summary>
        protected virtual void Revoke(){
         revoked=true;
         doing=false;
        }
        protected virtual void Update(){
         if(!doing){
          if(revoked||done){
           actor.OnSkillUsed(this);
           revoked=false;
           done=false;
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
          }
         }
        }
    }
}