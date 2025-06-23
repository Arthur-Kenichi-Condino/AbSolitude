#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     internal ExperiencePoints expPointsData;
        internal struct ExperiencePoints{
         public float SimLevelExpGivenOnDeath;
        }
        internal virtual void SetExpGiven(Stats stats){
         //Log.DebugMessage(this.name+":SetExpGiven");
         if(stats==null){
          return;
         }
        }
    }
}