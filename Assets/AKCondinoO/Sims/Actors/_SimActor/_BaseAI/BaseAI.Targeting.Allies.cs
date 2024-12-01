#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal virtual void OnAllyAskingForHelp(SimObject ally,SimObject target){
         if(target!=null&&target.id!=null){
          Log.DebugMessage("OnAllyAskingForHelp:target:"+target,this);
          ApplyAggressionModeForThenAddTarget(target,ally,false);
          SetTargetToBeRemoved(target);
          Log.DebugMessage("OnAllyAskingForHelp:'targetsByPriority.ContainsKey(target.id.Value)':"+targetsByPriority.ContainsKey(target.id.Value),this);
         }else{
         }
         alliesInTrouble[ally]=alliesTroubleForgetTimeout;
        }
    }
}