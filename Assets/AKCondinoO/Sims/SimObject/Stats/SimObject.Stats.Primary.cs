#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
        internal partial class Stats{
            protected virtual void OnGenerate_Stats(SimObject statsSim=null,bool reset=true){
             if(
              reset||
                bodily_kinesthetic_value<=0||
                     interpersonal_value<=0||
                     intrapersonal_value<=0||
                        linguistic_value<=0||
              logical_mathematical_value<=0||
                           musical_value<=0||
                      naturalistic_value<=0||
                           spatial_value<=0
             ){
              //  bodily_kinesthetic_value=(float)math_random.Next(1,131);updatedBodily_kinesthetic  =true;
              //       interpersonal_value=(float)math_random.Next(1,131);updatedInterpersonal       =true;
              //       intrapersonal_value=(float)math_random.Next(1,131);updatedIntrapersonal       =true;
              //          linguistic_value=(float)math_random.Next(1,131);updatedLinguistic          =true;
              //logical_mathematical_value=(float)math_random.Next(1,131);updatedLogical_mathematical=true;
              //             musical_value=(float)math_random.Next(1,131);updatedMusical             =true;
              //        naturalistic_value=(float)math_random.Next(1,131);updatedNaturalistic        =true;
              //             spatial_value=(float)math_random.Next(1,131);updatedSpatial             =true;
             }
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
            bodily_kinesthetic_value=value;
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
            interpersonal_value=value;
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
            intrapersonal_value=value;
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
            linguistic_value=value;
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
            logical_mathematical_value=value;
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
            musical_value=value;
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
            naturalistic_value=value;
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
            spatial_value=value;
            updatedSpatial=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedSpatial;
         #endregion
        }
    }
}