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
     protected Skill MySkill=null;internal Skill skillToUse{get{return MySkill;}}
        protected virtual void DoSkill(){
         SimObject target=this;//  TO DO: use best my skill target
         MySkill.DoSkill(target,MySkill.level);
        }
    }
}