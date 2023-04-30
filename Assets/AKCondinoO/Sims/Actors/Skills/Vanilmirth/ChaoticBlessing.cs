#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class ChaoticBlessing:SkillBase{
        internal readonly System.Random random=new System.Random();
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
         //SimObjectManager.singleton.active
        }
    }
}