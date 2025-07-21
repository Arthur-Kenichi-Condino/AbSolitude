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
         public int atLevel;
         public float simLevelExpGivenOnDeath;
         public float jobLevelExpGivenOnDeath;
            internal ExperiencePoints(float simLevelExpGivenOnDeath,float jobLevelExpGivenOnDeath){
             this.atLevel=0;
             this.simLevelExpGivenOnDeath=simLevelExpGivenOnDeath;
             this.jobLevelExpGivenOnDeath=jobLevelExpGivenOnDeath;
            }
        }
        internal virtual void ProcessExpPointsGiven(Stats stats){
         Log.DebugMessage(this.name+":ProcessExpPointsGiven");
         if(stats==null){
          return;
         }
         int level=stats.SimLevelGet(this);
         if(level!=expPoints.atLevel){
          Log.DebugMessage(this.name+":ProcessExpPointsGiven:'add level compensation'");
          float simExpPerLevel;
          float jobExpPerLevel;
          if(expPoints.atLevel>0){
           simExpPerLevel=expPoints.simLevelExpGivenOnDeath/expPoints.atLevel;
           jobExpPerLevel=expPoints.jobLevelExpGivenOnDeath/expPoints.atLevel;
          }else if(level>0){
           simExpPerLevel=expPoints.simLevelExpGivenOnDeath/level;
           jobExpPerLevel=expPoints.jobLevelExpGivenOnDeath/level;
          }else{
           simExpPerLevel=expPoints.simLevelExpGivenOnDeath;
           jobExpPerLevel=expPoints.jobLevelExpGivenOnDeath;
          }
          expPoints.simLevelExpGivenOnDeath=simExpPerLevel*level;
          expPoints.jobLevelExpGivenOnDeath=jobExpPerLevel*level;
          expPoints.atLevel=level;
         }
        }
        internal virtual void ReceiveExp(SimObject fromDeadSim,float simExp){
         if(stats!=null){
          Stats.ProcessExpPoints(this,fromDeadSim,simExp);
         }
        }
    }
}