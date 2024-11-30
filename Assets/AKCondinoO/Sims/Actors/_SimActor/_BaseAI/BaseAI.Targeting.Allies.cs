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
         if(target!=null){
          Log.DebugMessage("OnAllyAskingForHelp:target:"+target);
          ApplyAggressionModeForThenAddTarget(target,ally,false);
          SetTargetToBeRemoved(target);
         }else{
          Log.Warning("TO DO: move to ally and search for a target");
         }
        }
    }
}