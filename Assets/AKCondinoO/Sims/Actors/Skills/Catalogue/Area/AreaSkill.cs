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
     internal readonly List<SkillAoE>activeAoE=new List<SkillAoE>();
     internal Vector3?targetPos;
     internal readonly Dictionary<SkillAoE,float>aoeActiveCooldowns=new Dictionary<SkillAoE,float>();
     protected int maxAoECount_value=1;
      internal int maxAoECount{
       get{
        return maxAoECount_value;
       }
       set{
        maxAoECount_value=value;
       }
      }
        protected override void Awake(){
        }
        internal override void OnSpawned(){
        }
        internal override void OnPool(){
         foreach(SkillAoE aoe in activeAoE){
          aoe.skill=null;
         }
         activeAoE.Clear();
         aoeActiveCooldowns.Clear();
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
          if(!doing||!invoked){
           //  if the skill has not been cast yet then some other tests can still be done here, like focus points required to use the skill
           if(activeAoE.Count>=maxAoECount){
            return false;
           }
           if(aoeActiveCooldowns.Count>=maxAoECount){
            return false;
           }
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
        readonly List<SkillAoE>aoeActiveCooldownsToUpdate=new List<SkillAoE>();
        protected override void Update(){
         aoeActiveCooldownsToUpdate.AddRange(aoeActiveCooldowns.Keys);
         foreach(var aoe in aoeActiveCooldownsToUpdate){
          float cooldown=aoeActiveCooldowns[aoe];
          cooldown-=Time.deltaTime;
          if(cooldown>0f){
           aoeActiveCooldowns[aoe]=cooldown;
          }else{
           aoeActiveCooldowns.Remove(aoe);
          }
         }
         aoeActiveCooldownsToUpdate.Clear();
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