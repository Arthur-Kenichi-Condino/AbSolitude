#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid{
    internal partial class DisfiguringHomunculusAI{
        internal override void RenewStats(){
         //Log.DebugMessage("RenewStats");
         if(stats==null){
          if(statsPool.TryGetValue(typeof(DisfiguringHomunculusAIStats),out Queue<Stats>pool)&&pool.Count>0){
           stats=pool.Dequeue();
          }else{
           stats=new DisfiguringHomunculusAIStats();
          }
         }
        }
        internal partial class DisfiguringHomunculusAIStats:HumanoidAIStats{
        }
    }
}