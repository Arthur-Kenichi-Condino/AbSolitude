#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{}
#region ArquimedesAI
namespace AKCondinoO.Sims.Actors.Homunculi.Vanilmirth{
    internal partial class ArquimedesAI{
        internal partial class ArquimedesAIStats:VanilmirthAIStats{
            internal override void Generate(SimObject statsSim=null,bool reset=true){
             //Log.DebugMessage("Stats Generate");
             if(reset){
              OnReset(statsSim);
             }
             IsTranscendentSet(false,statsSim,false);
             SimLevelSet(200,statsSim,false);
             AgeLevelSet(15,statsSim,false);
             OnRefresh(statsSim);
             Log.DebugMessage("total stat points:"+totalStatPoints_value);
               Bodily_kinestheticSet(88f ,statsSim,false);//Log.DebugMessage(  "Bodily_kinestheticSet to:"+  bodily_kinesthetic_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                    InterpersonalSet(34f ,statsSim,false);//Log.DebugMessage(       "InterpersonalSet to:"+       interpersonal_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                    IntrapersonalSet(130f,statsSim,false);//Log.DebugMessage(       "IntrapersonalSet to:"+       intrapersonal_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                       LinguisticSet(70f ,statsSim,false);//Log.DebugMessage(          "LinguisticSet to:"+          linguistic_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
             Logical_mathematicalSet(88f ,statsSim,false);//Log.DebugMessage("Logical_mathematicalSet to:"+logical_mathematical_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                          MusicalSet(48f ,statsSim,false);//Log.DebugMessage(             "MusicalSet to:"+             musical_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                     NaturalisticSet(88f ,statsSim,false);//Log.DebugMessage(        "NaturalisticSet to:"+        naturalistic_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                          SpatialSet(88f ,statsSim,false);//Log.DebugMessage(             "SpatialSet to:"+             spatial_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
             Log.DebugMessage("remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
             base.Generate(statsSim,false);
            }
        }
    }
}
#endregion 
#region ArthurCondinoAI
namespace AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino{
    internal partial class ArthurCondinoAI{
        internal partial class ArthurCondinoAIStats{
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
#endregion 
#region DisfiguringHomunculusAI
namespace AKCondinoO.Sims.Actors.Humanoid{
    internal partial class DisfiguringHomunculusAI{
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
               Bodily_kinestheticSet(68f,statsSim,false);//Log.DebugMessage(  "Bodily_kinestheticSet to:"+  bodily_kinesthetic_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                    InterpersonalSet(2f ,statsSim,false);//Log.DebugMessage(       "InterpersonalSet to:"+       interpersonal_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                    IntrapersonalSet(2f ,statsSim,false);//Log.DebugMessage(       "IntrapersonalSet to:"+       intrapersonal_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                       LinguisticSet(1f ,statsSim,false);//Log.DebugMessage(          "LinguisticSet to:"+          linguistic_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
             Logical_mathematicalSet(3f ,statsSim,false);//Log.DebugMessage("Logical_mathematicalSet to:"+logical_mathematical_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                          MusicalSet(1f ,statsSim,false);//Log.DebugMessage(             "MusicalSet to:"+             musical_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                     NaturalisticSet(5f ,statsSim,false);//Log.DebugMessage(        "NaturalisticSet to:"+        naturalistic_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
                          SpatialSet(28f,statsSim,false);//Log.DebugMessage(             "SpatialSet to:"+             spatial_value+";remaining stat points:"+(totalStatPoints_value-statPointsSpent_value));
             base.Generate(statsSim,false);
            }
        }
    }
}
#endregion 
#region HumanAI
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
#endregion 