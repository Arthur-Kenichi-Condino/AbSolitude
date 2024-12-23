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
            internal override void Generate(SimObject statsSim=null,bool reset=true){
             //Log.DebugMessage("Stats Generate");
             if(reset){
              OnReset(statsSim);
             }
             IsTranscendentSet(true,statsSim,false);
             SimLevelSet(200,statsSim,false);
             AgeLevelSet(30,statsSim,false);
             OnRefresh(statsSim);
             Log.DebugMessage("total stat points:"+totalStatPoints_value);
               Bodily_kinestheticSet(88f ,statsSim,false);//Log.DebugMessage(  "Bodily_kinestheticSet to:"+  bodily_kinesthetic_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                    InterpersonalSet(51f ,statsSim,false);//Log.DebugMessage(       "InterpersonalSet to:"+       interpersonal_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                    IntrapersonalSet(130f,statsSim,false);//Log.DebugMessage(       "IntrapersonalSet to:"+       intrapersonal_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                       LinguisticSet(88f ,statsSim,false);//Log.DebugMessage(          "LinguisticSet to:"+          linguistic_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
             Logical_mathematicalSet(88f ,statsSim,false);//Log.DebugMessage("Logical_mathematicalSet to:"+logical_mathematical_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                          MusicalSet(70f ,statsSim,false);//Log.DebugMessage(             "MusicalSet to:"+             musical_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                     NaturalisticSet(88f ,statsSim,false);//Log.DebugMessage(        "NaturalisticSet to:"+        naturalistic_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                          SpatialSet(88f ,statsSim,false);//Log.DebugMessage(             "SpatialSet to:"+             spatial_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
             Log.DebugMessage("remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
             base.Generate(statsSim,false);
            }
        }
    }
}