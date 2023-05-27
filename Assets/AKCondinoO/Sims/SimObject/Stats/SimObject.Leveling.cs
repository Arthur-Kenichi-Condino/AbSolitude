#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
        internal partial class Stats{
         protected int simLevel_value;
          protected bool isTranscendent_value;
          protected int totalStatPoints_value;
          protected int statPointsSpent_value;
            protected virtual void OnGenerateValidation_Level(SimObject statsSim=null,bool reset=true){
             if(
              reset||
              simLevel_value<=0
             ){
              isTranscendent_value=math_random.CoinFlip();
             }
            }
        }
    }
}