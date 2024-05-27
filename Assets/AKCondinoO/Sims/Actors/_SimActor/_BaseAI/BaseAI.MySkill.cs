#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     internal Skill skillToUse{get{return ai?.MySkill;}}
        internal partial class AI{
         internal Skill MySkill=null;
        }
        protected virtual void DoSkill(){
         if(ai==null){
          return;
         }
         SimObject target=this;//  TO DO: use best my skill target
         ai.MySkill.DoSkill(target,ai.MySkill.level);
        }
    }
}