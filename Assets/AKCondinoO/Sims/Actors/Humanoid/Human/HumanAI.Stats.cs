#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid.Human{
    internal partial class HumanAI{
        internal partial class HumanAIStats:HumanoidAIStats{
            internal override void Generate(SimObject statsSim=null,bool reset=true){
             //Log.DebugMessage("Stats Generate");
             if(reset){
              OnReset(statsSim);
             }
             base.Generate(statsSim,false);
            }
        }
    }
}