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
            protected virtual void OnGenerateValidation_Stats(SimObject statsSim=null,bool reset=true){
             OnRefresh(statsSim);
             if(
              reset||
              statPointsSpent_value>totalStatPoints_value||
                bodily_kinesthetic_value<=0||
                     interpersonal_value<=0||
                     intrapersonal_value<=0||
                        linguistic_value<=0||
              logical_mathematical_value<=0||
                           musical_value<=0||
                      naturalistic_value<=0||
                           spatial_value<=0||
              (GetStatPointsSpentFor(  bodily_kinesthetic_value)+
               GetStatPointsSpentFor(       interpersonal_value)+
               GetStatPointsSpentFor(       intrapersonal_value)+
               GetStatPointsSpentFor(          linguistic_value)+
               GetStatPointsSpentFor(logical_mathematical_value)+
               GetStatPointsSpentFor(             musical_value)+
               GetStatPointsSpentFor(        naturalistic_value)+
               GetStatPointsSpentFor(             spatial_value)
               !=statPointsSpent_value)
             ){
              statPointsSpent_value=0;
                 bodily_kinesthetic_value=0f;
                      interpersonal_value=0f;
                      intrapersonal_value=0f;
                         linguistic_value=0f;
               logical_mathematical_value=0f;
                            musical_value=0f;
                       naturalistic_value=0f;
                            spatial_value=0f;
                Bodily_kinestheticSet((float)math_random.Next(1,131),statsSim,false);
                     InterpersonalSet((float)math_random.Next(1,131),statsSim,false);
                     IntrapersonalSet((float)math_random.Next(1,131),statsSim,false);
                        LinguisticSet((float)math_random.Next(1,131),statsSim,false);
              Logical_mathematicalSet((float)math_random.Next(1,131),statsSim,false);
                           MusicalSet((float)math_random.Next(1,131),statsSim,false);
                      NaturalisticSet((float)math_random.Next(1,131),statsSim,false);
                           SpatialSet((float)math_random.Next(1,131),statsSim,false);
             }
            }
            protected static void TryRaiseStatLevelTo(ref float stat,int toLevel,ref int statPointsSpent,int totalStatPoints){
             toLevel=Math.Min(toLevel,130);
             if(stat>=130){
              return;
             }
             if(toLevel<=stat){
              return;
             }
             int availablePoints=totalStatPoints-statPointsSpent;
             if(availablePoints<GetStatPointsRequired(1,2)){
              return;
             }
             if(availablePoints<GetStatPointsRequired(stat,stat+1)){
              return;
             }
             int statPointsAlreadySpentOnStat=GetStatPointsSpentFor(stat);
             int raisingStatToLevelWillResultOnThisMuchTotalStatPointsSpent=GetStatPointsSpentFor(toLevel);
             int statPointsTheStatWillConsumeOnRaisingStatToLevel=raisingStatToLevelWillResultOnThisMuchTotalStatPointsSpent-statPointsAlreadySpentOnStat;
             if(statPointsTheStatWillConsumeOnRaisingStatToLevel<=availablePoints){
              stat=toLevel;
              statPointsSpent+=statPointsTheStatWillConsumeOnRaisingStatToLevel;
              return;
             }
             int totalStatPointsThatAreSpentOnTryRaiseStatToLevel=statPointsAlreadySpentOnStat+Math.Min(statPointsTheStatWillConsumeOnRaisingStatToLevel,availablePoints);
             int statPointsFor130=GetStatPointsSpentFor(130);
             if(totalStatPointsThatAreSpentOnTryRaiseStatToLevel>=statPointsFor130){
              stat=130;
              int statPointsConsumed=statPointsFor130-statPointsAlreadySpentOnStat;
              statPointsSpent+=statPointsConsumed;
              return;
             }
             bool cached;
             int cachedLevel;
             lock(maxStatLevelByStatPointsAvailable){
              cached=maxStatLevelByStatPointsAvailable.TryGetValue(totalStatPointsThatAreSpentOnTryRaiseStatToLevel,out cachedLevel);
             }
             if(cached){
              stat=cachedLevel;
              int statPointsConsumed=GetStatPointsSpentFor(stat)-statPointsAlreadySpentOnStat;
              statPointsSpent+=statPointsConsumed;
              return;
             }
             int statFloor=Mathf.FloorToInt(stat);
             for(int level=statFloor+1;level<=toLevel;level++){
              int consumed=GetStatPointsRequired(level-1,level);
              if(consumed>availablePoints){
               return;
              }
              stat=level;
              statPointsSpent+=consumed;
             }
            }
            protected static void SpendAllRemainingPointsOn(ref float stat,ref int statPointsSpent,int totalStatPoints){
             TryRaiseStatLevelTo(ref stat,130,ref statPointsSpent,totalStatPoints);
            }
         static readonly Dictionary<(int fromStatLevel,int toStatLevel),int>statPointsRequired=new Dictionary<(int,int),int>();
            internal static int GetStatPointsRequired(float fromStatLevel,float toStatLevel){
             int fromStatLevelFloor=Mathf.FloorToInt(fromStatLevel);
             int   toStatLevelFloor=Mathf.FloorToInt(  toStatLevel);
             lock(statPointsRequired){
              if(statPointsRequired.TryGetValue((fromStatLevelFloor,toStatLevelFloor),out int cached)){
               return cached;
              }
             }
             int statPoints=0;
             for(int level=fromStatLevelFloor+1;level<=toStatLevelFloor;level++){
              if(level<=99){
               statPoints+=RaiseStatPointInFrom1To99Interval(level);
              }else{
               statPoints+=RaiseStatPointInFrom100To130Interval(level);
              }
              lock(statPointsRequired){
               statPointsRequired[(fromStatLevelFloor,toStatLevelFloor)]=statPoints;
              }
             }
             return statPoints;
            }
         static readonly Dictionary<float,int>totalStatPointsRequired=new Dictionary<float,int>();
          static readonly Dictionary<int,int>maxStatLevelByStatPointsAvailable=new Dictionary<int,int>();
            static void CacheMaxStatLevelByStatPointsAvailable(int level,int statPoints){
             lock(maxStatLevelByStatPointsAvailable){
              maxStatLevelByStatPointsAvailable[statPoints]=level;
             }
             int oneLevelLowerTotalStatPointsRequired;
             lock(totalStatPointsRequired){
              if(!totalStatPointsRequired.TryGetValue(level-1,out oneLevelLowerTotalStatPointsRequired)){
               return;
              }
             }
             lock(maxStatLevelByStatPointsAvailable){
              for(int sP=oneLevelLowerTotalStatPointsRequired;sP<statPoints;++sP){
               maxStatLevelByStatPointsAvailable[sP]=level-1;
              }
             }
            }
            internal static int RaiseStatPointInFrom1To99Interval(int level){
             return Mathf.FloorToInt(((level-1)-1)/10f)+2;
            }
            internal static int GetStatPointsRequiredFrom1To99(float statLevel){
             lock(totalStatPointsRequired){
              if(totalStatPointsRequired.TryGetValue(statLevel,out int cached)){
               return cached;
              }
             }
             int statPoints=0;
             for(int level=2;level<=Math.Min(statLevel,99);level++){
              statPoints+=RaiseStatPointInFrom1To99Interval(level);
              lock(totalStatPointsRequired){
               totalStatPointsRequired[level]=statPoints;
              }
              CacheMaxStatLevelByStatPointsAvailable(level,statPoints);
             }
             return statPoints;
            }
            internal static int RaiseStatPointInFrom100To130Interval(int level){
             if(level==100){
              return 11;
             }
             return 4*Mathf.FloorToInt(((level-1)-100)/5f)+16;
            }
            internal static int GetStatPointsRequiredFrom100To130(float statLevel){
             lock(totalStatPointsRequired){
              if(totalStatPointsRequired.TryGetValue(statLevel,out int cached)){
               return cached;
              }
             }
             int statPoints=GetStatPointsRequiredFrom1To99(statLevel);
             for(int level=100;level<=Math.Min(statLevel,130);level++){
              statPoints+=RaiseStatPointInFrom100To130Interval(level);
              lock(totalStatPointsRequired){
               totalStatPointsRequired[level]=statPoints;
              }
              CacheMaxStatLevelByStatPointsAvailable(level,statPoints);
             }
             return statPoints;
            }
            internal static int GetStatPointsSpentFor(float stat){
             int statPoints=GetStatPointsRequiredFrom100To130(stat);
             return statPoints;
            }
         //
         #region Bodily_kinesthetic
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float bodily_kinesthetic_value;
          internal float Bodily_kinestheticGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return bodily_kinesthetic_value;
          }
           internal void Bodily_kinestheticSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            TryRaiseStatLevelTo(ref bodily_kinesthetic_value,Mathf.FloorToInt(value),ref statPointsSpent_value,totalStatPoints_value);
            updatedBodily_kinesthetic=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedBodily_kinesthetic;
         #endregion
         #region Interpersonal
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float interpersonal_value;
          internal float InterpersonalGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return interpersonal_value;
          }
           internal void InterpersonalSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            TryRaiseStatLevelTo(ref interpersonal_value,Mathf.FloorToInt(value),ref statPointsSpent_value,totalStatPoints_value);
            updatedInterpersonal=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedInterpersonal;
         #endregion
         #region Intrapersonal
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float intrapersonal_value;
          internal float IntrapersonalGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return intrapersonal_value;
          }
           internal void IntrapersonalSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            TryRaiseStatLevelTo(ref intrapersonal_value,Mathf.FloorToInt(value),ref statPointsSpent_value,totalStatPoints_value);
            updatedIntrapersonal=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedIntrapersonal;
         #endregion
         #region Linguistic
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float linguistic_value;
          internal float LinguisticGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return linguistic_value;
          }
           internal void LinguisticSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            TryRaiseStatLevelTo(ref linguistic_value,Mathf.FloorToInt(value),ref statPointsSpent_value,totalStatPoints_value);
            updatedLinguistic=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedLinguistic;
         #endregion
         #region Logical_mathematical
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float logical_mathematical_value;
          internal float Logical_mathematicalGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return logical_mathematical_value;
          }
           internal void Logical_mathematicalSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            TryRaiseStatLevelTo(ref logical_mathematical_value,Mathf.FloorToInt(value),ref statPointsSpent_value,totalStatPoints_value);
            updatedLogical_mathematical=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedLogical_mathematical;
         #endregion
         #region Musical
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float musical_value;
          internal float MusicalGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return musical_value;
          }
           internal void MusicalSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            TryRaiseStatLevelTo(ref musical_value,Mathf.FloorToInt(value),ref statPointsSpent_value,totalStatPoints_value);
            updatedMusical=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedMusical;
         #endregion
         #region Naturalistic
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float naturalistic_value;
          internal float NaturalisticGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return naturalistic_value;
          }
           internal void NaturalisticSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            TryRaiseStatLevelTo(ref naturalistic_value,Mathf.FloorToInt(value),ref statPointsSpent_value,totalStatPoints_value);
            updatedNaturalistic=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedNaturalistic;
         #endregion
         #region Spatial
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float spatial_value;
          internal float SpatialGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return spatial_value;
          }
           internal void SpatialSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            TryRaiseStatLevelTo(ref spatial_value,Mathf.FloorToInt(value),ref statPointsSpent_value,totalStatPoints_value);
            updatedSpatial=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedSpatial;
         #endregion
        }
    }
}