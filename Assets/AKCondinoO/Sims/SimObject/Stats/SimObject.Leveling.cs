#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
        internal partial class Stats{
            protected virtual void OnGenerateValidation_Level(SimObject statsSim=null,bool reset=true){
             OnRefresh(statsSim);
             if(
              reset||
              simLevel_value<=0||
              (!isTranscendent_value&&simLevel_value>99)
             ){
              IsTranscendentSet(math_random.CoinFlip(),statsSim,false);
              if(isTranscendent_value){
               SimLevelSet(math_random.Next(1,201),statsSim,false);
              }else{
               SimLevelSet(math_random.Next(1,100),statsSim,false);
              }
             }
             Log.DebugMessage("statsSim:"+statsSim+":simLevel_value:"+simLevel_value);
            }
         #region SimLevel
         protected int simLevel_value;
          protected int totalStatPoints_value;
          protected int statPointsSpent_value;
          internal int SimLevelGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return simLevel_value;
          }
           internal void SimLevelSet(int value,SimObject statsSim=null,bool forceRefresh=false){
            simLevel_value=value;
            updatedSimLevel=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedSimLevel;
         internal void OnRefresh_SimLevel(SimObject statsSim=null){
          if(updatedSimLevel||
             refreshedIsTranscendent
          ){
           totalStatPoints_value=AddStatPointsFrom1To99(simLevel_value,isTranscendent_value);
           totalStatPoints_value+=AddStatPointsFrom100To150(simLevel_value);
           totalStatPoints_value+=AddStatPointsFrom151To200(simLevel_value);
           Log.DebugMessage("statsSim:"+statsSim+":totalStatPoints_value:"+totalStatPoints_value);
          }
         }
          int AddStatPointsFrom1To99(int currentLevel,bool transcendent){
           int statPoints;
           if(transcendent){
            statPoints=100;
           }else{
            statPoints=48;
           }
           for(int level=2;level<=Math.Min(currentLevel,99);level++){
            statPoints+=Mathf.FloorToInt((level-1)/5f)+3;
           }
           return statPoints;
          }
          int AddStatPointsFrom100To150(int currentLevel){
           int statPoints=0;
           for(int level=100;level<=Math.Min(currentLevel,150);level++){
            statPoints+=Mathf.FloorToInt((level-1)/10f)+13;
           }
           return statPoints;
          }
          int AddStatPointsFrom151To200(int currentLevel){
           int statPoints=0;
           for(int level=151;level<=Math.Min(currentLevel,200);level++){
            statPoints+=Mathf.FloorToInt(((level-1)-150)/7f)+28;
           }
           return statPoints;
          }
          protected bool refreshedSimLevel;
         #endregion
          #region IsTranscendent
          protected bool isTranscendent_value;
           internal bool IsTranscendentGet(SimObject statsSim=null){
            OnRefresh(statsSim);
            return isTranscendent_value;
           }
            internal void IsTranscendentSet(bool value,SimObject statsSim=null,bool forceRefresh=false){
             isTranscendent_value=value;
             updatedIsTranscendent=true;
             SetPendingRefresh(statsSim,forceRefresh);
            }
             protected bool updatedIsTranscendent;
          internal void OnRefresh_IsTranscendent(SimObject statsSim=null){
           if(updatedIsTranscendent){
            if(!isTranscendent_value&&simLevel_value>99){
             simLevel_value=99;
             refreshedIsTranscendent=true;
            }
           }
          }
           protected bool refreshedIsTranscendent;
          #endregion
        }
    }
}