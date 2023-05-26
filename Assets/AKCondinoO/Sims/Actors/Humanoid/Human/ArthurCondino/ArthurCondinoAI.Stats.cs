#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino{
    internal partial class ArthurCondinoAI{
        internal override void RenewStats(){
         //Log.DebugMessage("RenewStats");
         if(stats==null){
          if(statsPool.TryGetValue(typeof(ArthurCondinoAIStats),out Queue<Stats>pool)&&pool.Count>0){
           stats=pool.Dequeue();
          }else{
           stats=new ArthurCondinoAIStats();
          }
         }
        }
        internal partial class ArthurCondinoAIStats:HumanAIStats{
            internal override void Generate(SimObject statsSim=null){
             //Log.DebugMessage("Stats Generate");
            }
        }
    }
}