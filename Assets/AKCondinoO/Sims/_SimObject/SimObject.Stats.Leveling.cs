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
              ageLevel_value<=0
             ){
              IsTranscendentSet(math_random.CoinFlip(),statsSim,false);
              SimLevelSet(math_random.Next(1,201),statsSim,false);
              AgeLevelSet(math_random.Next(1,970),statsSim,false);
             }
             //Log.DebugMessage("statsSim:"+statsSim+":simLevel_value:"+simLevel_value);
            }
         #region IsTranscendent
         [NonSerialized]protected bool isTranscendent_value;
          internal bool IsTranscendentGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return isTranscendent_value;
          }
           internal void IsTranscendentSet(bool value,SimObject statsSim=null,bool forceRefresh=false){
            isTranscendent_value=value;
            updatedIsTranscendent=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            [NonSerialized]protected bool updatedIsTranscendent;
             internal void OnRefresh_IsTranscendent(SimObject statsSim=null){
              if(onGeneration||
                 updatedIsTranscendent
              ){
               refreshedIsTranscendent=true;
              }
             }
              [NonSerialized]protected bool refreshedIsTranscendent;
         #endregion
          #region Experience
          /// <summary>
          ///  Experience
          /// </summary>
          [NonSerialized]protected float experience_value;
           internal float ExperienceGet(SimObject statsSim=null){
            OnRefresh(statsSim);
            return experience_value;
           }
            internal void ExperienceSet(float value,SimObject statsSim=null,bool forceRefresh=false){
             experience_value=value;
             updatedExperience=true;
             SetPendingRefresh(statsSim,forceRefresh);
            }
             [NonSerialized]protected bool updatedExperience;
              internal void OnRefresh_Experience(SimObject statsSim=null){
               if(onGeneration||
                  updatedExperience||
                  refreshedIsTranscendent
               ){
                bool isTranscendent=isTranscendent_value;
                int curLevel=simLevel_value;
                int nextLevel=curLevel+1;
                float expForNextLevel=GetExpPointsForNextLevelFrom201To250(curLevel,isTranscendent);
                //Log.DebugMessage("curLevel:"+curLevel+";expForNextLevel:"+expForNextLevel);
                refreshedExperience=true;
               }
              }
               [NonSerialized]protected bool refreshedExperience;
         [NonSerialized]const float expPointsForFirstLevelNonTransc=548f;
          [NonSerialized]const float totalExpPointsNonTransc       =22532516f;
         [NonSerialized]const float expPointsForFirstLevelTransc   =658f;
          [NonSerialized]const float totalExpPointsTransc          =37047973f;
         [NonSerialized]static readonly Dictionary<(int currentLevel,bool transcendent),float>totalExpPointsForNextLevel=new Dictionary<(int,bool),float>();
            internal static float GetExpPointsForNextLevelFrom1To99(int currentLevel,bool transcendent){
             lock(totalExpPointsForNextLevel){
              if(totalExpPointsForNextLevel.TryGetValue((currentLevel,transcendent),out float cached)){
               return cached;
              }
             }
             if(currentLevel>=99){
              return GetExpPointsForNextLevelFrom100To150(currentLevel,transcendent);
             }
             float result=(!transcendent?expPointsForFirstLevelNonTransc:expPointsForFirstLevelTransc);
             if(currentLevel>1){
              if(!transcendent){
               result=expPointsForFirstLevelNonTransc*Mathf.Pow(Mathf.Pow((totalExpPointsNonTransc/expPointsForFirstLevelNonTransc),1f/99f),Math.Min(currentLevel,99));
              }else{
               result=expPointsForFirstLevelTransc   *Mathf.Pow(Mathf.Pow((totalExpPointsTransc   /expPointsForFirstLevelTransc   ),1f/99f),Math.Min(currentLevel,99));
              }
             }
             lock(totalExpPointsForNextLevel){
              totalExpPointsForNextLevel[(currentLevel,transcendent)]=result;
             }
             return result;
            }
         [NonSerialized]const float expPointsForLevel100NonTransc       =1272747f;
          [NonSerialized]const float totalExpPointsFrom100To150NonTransc=596863680f;
         [NonSerialized]const float expPointsForLevel100Transc          =1528225f;
          [NonSerialized]const float totalExpPointsFrom100To150Transc   =981363532f;
            internal static float GetExpPointsForNextLevelFrom100To150(int currentLevel,bool transcendent){
             lock(totalExpPointsForNextLevel){
              if(totalExpPointsForNextLevel.TryGetValue((currentLevel,transcendent),out float cached)){
               return cached;
              }
             }
             if(currentLevel<99){
              return GetExpPointsForNextLevelFrom1To99   (currentLevel,transcendent);
             }
             if(currentLevel>=150){
              return GetExpPointsForNextLevelFrom151To200(currentLevel,transcendent);
             }
             float result=(!transcendent?expPointsForLevel100NonTransc:expPointsForLevel100Transc);
             if(currentLevel>99){
              if(!transcendent){
               result=expPointsForLevel100NonTransc*Mathf.Pow(Mathf.Pow((totalExpPointsFrom100To150NonTransc/expPointsForLevel100NonTransc),1f/50f),Math.Min(currentLevel-100,150-100));
              }else{
               result=expPointsForLevel100Transc   *Mathf.Pow(Mathf.Pow((totalExpPointsFrom100To150Transc   /expPointsForLevel100Transc   ),1f/50f),Math.Min(currentLevel-100,150-100));
              }
             }
             lock(totalExpPointsForNextLevel){
              totalExpPointsForNextLevel[(currentLevel,transcendent)]=result;
             }
             return result;
            }
         [NonSerialized]const float expPointsForLevel151NonTransc       =645371884f;
          [NonSerialized]const float totalExpPointsFrom151To200NonTransc=15846309875f;
         [NonSerialized]const float expPointsForLevel151Transc          =774446260f;
          [NonSerialized]const float totalExpPointsFrom151To200Transc   =19015571850f;
            internal static float GetExpPointsForNextLevelFrom151To200(int currentLevel,bool transcendent){
             lock(totalExpPointsForNextLevel){
              if(totalExpPointsForNextLevel.TryGetValue((currentLevel,transcendent),out float cached)){
               return cached;
              }
             }
             if(currentLevel<150){
              return GetExpPointsForNextLevelFrom100To150(currentLevel,transcendent);
             }
             if(currentLevel>=200){
              return GetExpPointsForNextLevelFrom201To250(currentLevel,transcendent);
             }
             float result=(!transcendent?expPointsForLevel151NonTransc:expPointsForLevel151Transc);
             if(currentLevel>150){
              if(!transcendent){
               result=expPointsForLevel151NonTransc*Mathf.Pow(Mathf.Pow((totalExpPointsFrom151To200NonTransc/expPointsForLevel151NonTransc),1f/50f),Math.Min(currentLevel-150,200-150));
              }else{
               result=expPointsForLevel151Transc   *Mathf.Pow(Mathf.Pow((totalExpPointsFrom151To200Transc   /expPointsForLevel151Transc   ),1f/50f),Math.Min(currentLevel-150,200-150));
              }
             }
             lock(totalExpPointsForNextLevel){
              totalExpPointsForNextLevel[(currentLevel,transcendent)]=result;
             }
             return result;
            }
         [NonSerialized]const float expPointsForLevel201       =653047446f;
          [NonSerialized]const float totalExpPointsFrom201To250=1503757559122f;
            internal static float GetExpPointsForNextLevelFrom201To250(int currentLevel,bool transcendent){
             lock(totalExpPointsForNextLevel){
              if(totalExpPointsForNextLevel.TryGetValue((currentLevel,transcendent),out float cached)){
               return cached;
              }
             }
             if(currentLevel<200){
              return GetExpPointsForNextLevelFrom151To200(currentLevel,transcendent);
             }
             float result=expPointsForLevel201;
             if(currentLevel>200){
              result=expPointsForLevel201*Mathf.Pow(Mathf.Pow((totalExpPointsFrom201To250/expPointsForLevel201),1f/50f),Math.Min(currentLevel-200,250-200));
             }
             lock(totalExpPointsForNextLevel){
              totalExpPointsForNextLevel[(currentLevel,transcendent)]=result;
             }
             return result;
            }
          #endregion
           #region SimLevel
           /// <summary>
           ///  Based on all character's interactions (which gives Exp)
           /// </summary>
           [NonSerialized]protected int simLevel_value;
            [NonSerialized]protected int totalStatPoints_value;
            [NonSerialized]protected int statPointsSpent_value;
            internal int SimLevelGet(SimObject statsSim=null){
             OnRefresh(statsSim);
             return simLevel_value;
            }
             internal void SimLevelSet(int value,SimObject statsSim=null,bool forceRefresh=false){
              simLevel_value=value;
              updatedSimLevel=true;
              SetPendingRefresh(statsSim,forceRefresh);
             }
              [NonSerialized]protected bool updatedSimLevel;
               internal void OnRefresh_SimLevel(SimObject statsSim=null){
                if(onGeneration||
                   updatedSimLevel||
                   refreshedIsTranscendent
                ){
                 totalStatPoints_value=AddStatPointsFrom201To250(simLevel_value,isTranscendent_value);
                 //Log.DebugMessage("statsSim:"+statsSim+":totalStatPoints_value:"+totalStatPoints_value);
                 refreshedSimLevel=true;
                }
               }
                [NonSerialized]protected bool refreshedSimLevel;
         [NonSerialized]static readonly Dictionary<(int currentLevel,bool transcendent),int>totalStatPointsAtLevel=new Dictionary<(int,bool),int>();
            internal static int AddStatPointsFrom1To99(int currentLevel,bool transcendent){
             lock(totalStatPointsAtLevel){
              if(totalStatPointsAtLevel.TryGetValue((currentLevel,transcendent),out int cached)){
               return cached;
              }
             }
             int statPoints;
             if(transcendent){
              statPoints=100;
             }else{
              statPoints=48;
             }
             for(int level=2;level<=Math.Min(currentLevel,99);level++){
              statPoints+=Mathf.FloorToInt(
               (transcendent?((float)expPointsForFirstLevelTransc/(float)expPointsForFirstLevelNonTransc):1)*
               (((level-1)/5f)+3)
              );
              lock(totalStatPointsAtLevel){
               totalStatPointsAtLevel[(level,transcendent)]=statPoints;
              }
             }
             return statPoints;
            }
            internal static int AddStatPointsFrom100To150(int currentLevel,bool transcendent){
             lock(totalStatPointsAtLevel){
              if(totalStatPointsAtLevel.TryGetValue((currentLevel,transcendent),out int cached)){
               return cached;
              }
             }
             int statPoints=AddStatPointsFrom1To99(currentLevel,transcendent);
             for(int level=100;level<=Math.Min(currentLevel,150);level++){
              statPoints+=Mathf.FloorToInt(
               (transcendent?((float)expPointsForFirstLevelTransc/(float)expPointsForFirstLevelNonTransc):1)*
               (Mathf.FloorToInt((level-1)/10f)+13)
              );
              lock(totalStatPointsAtLevel){
               totalStatPointsAtLevel[(level,transcendent)]=statPoints;
              }
             }
             return statPoints;
            }
            internal static int AddStatPointsFrom151To200(int currentLevel,bool transcendent){
             lock(totalStatPointsAtLevel){
              if(totalStatPointsAtLevel.TryGetValue((currentLevel,transcendent),out int cached)){
               return cached;
              }
             }
             int statPoints=AddStatPointsFrom100To150(currentLevel,transcendent);
             for(int level=151;level<=Math.Min(currentLevel,200);level++){
              statPoints+=Mathf.FloorToInt(
               (transcendent?((float)expPointsForFirstLevelTransc/(float)expPointsForFirstLevelNonTransc):1)*
               (Mathf.FloorToInt(((level-1)-150)/7f)+28)
              );
              lock(totalStatPointsAtLevel){
               totalStatPointsAtLevel[(level,transcendent)]=statPoints;
              }
             }
             return statPoints;
            }
            internal static int AddStatPointsFrom201To250(int currentLevel,bool transcendent){
             lock(totalStatPointsAtLevel){
              if(totalStatPointsAtLevel.TryGetValue((currentLevel,transcendent),out int cached)){
               return cached;
              }
             }
             int statPoints=AddStatPointsFrom151To200(currentLevel,transcendent);
             for(int level=201;level<=Math.Min(currentLevel,250);level++){
              statPoints+=Mathf.FloorToInt(((level-1)-150)/7f)+28;
              lock(totalStatPointsAtLevel){
               totalStatPointsAtLevel[(level,transcendent)]=statPoints;
              }
             }
             return statPoints;
            }
           #endregion
            #region AgeLevel
            /// <summary>
            ///  Based on character's existence time (which raises Age)
            /// </summary>
            [NonSerialized]protected int ageLevel_value;
            internal int AgeLevelGet(SimObject statsSim=null){
             OnRefresh(statsSim);
             return ageLevel_value;
            }
             internal void AgeLevelSet(int value,SimObject statsSim=null,bool forceRefresh=false){
              ageLevel_value=value;
              updatedAgeLevel=true;
              SetPendingRefresh(statsSim,forceRefresh);
             }
              [NonSerialized]protected bool updatedAgeLevel;
               internal void OnRefresh_AgeLevel(SimObject statsSim=null){
                if(onGeneration||
                   updatedAgeLevel
                ){
                 refreshedAgeLevel=true;
                }
               }
                [NonSerialized]protected bool refreshedAgeLevel;
            #endregion
        }
    }
}