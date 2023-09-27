#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class OnHitGracePeriod:OnHitSkill{
     [SerializeField]internal float vulnerabilityDuration=2f;
      [SerializeField]internal float gracePeriodForResetAnimation=.2f;
       [SerializeField]internal float gracePeriod=5f;
        internal override bool DoSkill(SimObject target,int useLevel){
         if(base.DoSkill(target,useLevel)){
          return true;
         }
         //  the skill cannot be used!
         return false;
        }
        protected override void Invoke(){
         Log.DebugMessage("OnHitGracePeriod:Invoke()");
         bool containsBuff=false;
         if(target.skillBuffs.Contains(typeof(OnHitGracePeriodSkillBuff),out List<SkillBuff>activeOnHitGracePeriodSkillBuffs)){
          Log.DebugMessage("OnHitGracePeriod:containsBuff=true");
          SkillBuff buff=activeOnHitGracePeriodSkillBuffs[0];
          buff.duration=buff.elapsedTime+vulnerabilityDuration+gracePeriod;
          containsBuff=true;
         }
         if(!containsBuff){
          SkillBuff buff=SkillBuff.Dequeue(typeof(OnHitGracePeriodSkillBuff));
          if(buff!=null){
           OnHitGracePeriodSkillBuff onHitGracePeriodSkillBuff=(OnHitGracePeriodSkillBuff)buff;
           onHitGracePeriodSkillBuff.vulnerabilityDuration=vulnerabilityDuration;
            onHitGracePeriodSkillBuff.gracePeriodForResetAnimation=gracePeriodForResetAnimation;
             onHitGracePeriodSkillBuff.gracePeriod=gracePeriod;
           buff.duration=vulnerabilityDuration+gracePeriod;
           buff.delay=0f;
           target.skillBuffs.Add(buff,this);
           Log.DebugMessage("OnHitGracePeriod:added buff");
           buff.ManualUpdate(0f);
          }
         }
         base.Invoke();//  the invoked flag is set here
        }
        protected override void OnInvokeSetCooldown(){
         //Log.DebugMessage("SpiritualHealing cooldown:"+cooldown);
         base.OnInvokeSetCooldown();
        }
        internal override float GetOnInvokeCooldown(){
         return 0f;
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