#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class SpiritualHealing:HealingSkill{
        internal override bool DoSkill(SimObject target,int useLevel){
         if(base.DoSkill(target,useLevel)){
          target.OnTargetedBySkill(this,actor);
          return true;
         }
         //  the skill cannot be used!
         return false;
        }
        protected override void Invoke(){
         SkillBuff buff=SkillBuff.Dequeue(typeof(SpiritualHealingSkillBuff));
         if(buff!=null){
          buff.duration=0f;
          buff.delay=0f;
          target.skillBuffs.Add(buff,this);
         }
         target.OnHitByTargetedSkill(this,actor);
         base.Invoke();//  the invoked flag is set here
        }
        protected override void Update(){
         base.Update();
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