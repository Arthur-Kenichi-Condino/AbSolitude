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
            internal override void Generate(SimObject statsSim=null,bool reset=true){
             //Log.DebugMessage("Stats Generate");
             if(reset){
              OnReset(statsSim);
             }
             IsTranscendentSet(false,statsSim,false);
             SimLevelSet(54,statsSim,false);
             AgeLevelSet(1000,statsSim,false);
             OnRefresh(statsSim);
               Bodily_kinestheticSet(68f,statsSim,false);Log.DebugMessage(  "Bodily_kinestheticSet to:"+  bodily_kinesthetic_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                    InterpersonalSet(2f ,statsSim,false);Log.DebugMessage(       "InterpersonalSet to:"+       interpersonal_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                    IntrapersonalSet(2f ,statsSim,false);Log.DebugMessage(       "IntrapersonalSet to:"+       intrapersonal_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                       LinguisticSet(1f ,statsSim,false);Log.DebugMessage(          "LinguisticSet to:"+          linguistic_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
             Logical_mathematicalSet(3f ,statsSim,false);Log.DebugMessage("Logical_mathematicalSet to:"+logical_mathematical_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                          MusicalSet(1f ,statsSim,false);Log.DebugMessage(             "MusicalSet to:"+             musical_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                     NaturalisticSet(5f ,statsSim,false);Log.DebugMessage(        "NaturalisticSet to:"+        naturalistic_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                          SpatialSet(28f,statsSim,false);Log.DebugMessage(             "SpatialSet to:"+             spatial_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
             base.Generate(statsSim,false);
            }
        }
    }
}