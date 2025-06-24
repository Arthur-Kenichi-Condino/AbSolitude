#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     internal ExperiencePoints expPoints;
        internal struct ExperiencePoints{
         public float simLevelExpGivenOnDeath;
         public float jobLevelExpGivenOnDeath;
            internal ExperiencePoints(float simLevelExpGivenOnDeath,float jobLevelExpGivenOnDeath){
             this.simLevelExpGivenOnDeath=simLevelExpGivenOnDeath;
             this.jobLevelExpGivenOnDeath=jobLevelExpGivenOnDeath;
            }
        }
        internal virtual void ProcessExpPointsGiven(Stats stats){
         //Log.DebugMessage(this.name+":SetExpGiven");
         if(stats==null){
          return;
         }
        }
    }
}