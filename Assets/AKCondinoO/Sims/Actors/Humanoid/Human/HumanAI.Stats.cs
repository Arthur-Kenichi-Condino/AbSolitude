#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid.Human{
    internal partial class HumanAI{
        internal partial class HumanAIStats:HumanoidAIStats{
            internal override void Generate(SimObject statsSim=null){
             //Log.DebugMessage("Stats Generate");
               bodily_kinesthetic_value=(float)math_random.NextDouble(1d,100d);updatedBodily_kinesthetic  =true;
                    interpersonal_value=(float)math_random.NextDouble(1d,100d);updatedInterpersonal       =true;
                    intrapersonal_value=(float)math_random.NextDouble(1d,100d);updatedIntrapersonal       =true;
                       linguistic_value=(float)math_random.NextDouble(1d,100d);updatedLinguistic          =true;
             logical_mathematical_value=(float)math_random.NextDouble(1d,100d);updatedLogical_mathematical=true;
                          musical_value=(float)math_random.NextDouble(1d,100d);updatedMusical             =true;
                     naturalistic_value=(float)math_random.NextDouble(1d,100d);updatedNaturalistic        =true;
                          spatial_value=(float)math_random.NextDouble(1d,100d);updatedSpatial             =true;
             SetPendingRefresh(statsSim,false);
            }
        }
    }
}