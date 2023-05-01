#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
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
     SimActor actorTarget;
        internal void GetRandomTarget(){
         actorTarget=target;
         float random=Mathf.Clamp01((float)dice.NextDouble());
         if(random<0.11f){
          //
         }
        }
        protected override void Invoke(){
         //  do more skill initialization here / or use to as main call of the skill
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