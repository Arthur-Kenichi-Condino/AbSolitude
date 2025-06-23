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
         //
         #region Sanity
         /// <summary>
         ///  Sanidade mental ou no��o existencial atual para o objeto Sim: se este valor chegar a 0f, o Sim ir� desmaiar ou perder o controle das a��es
         /// </summary>
         [NonSerialized]protected float sanity_value;
          internal float SanityGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return sanity_value;
          }
           internal void SanitySet(float value,SimObject statsSim=null,bool forceRefresh=false){
            sanity_value=value;
            updatedSanity=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            [NonSerialized]protected bool updatedSanity;
         #region MaxSanity
         /// <summary>
         ///  Sanidade mental ou no��o existencial m�xima do objeto Sim: valor calculado com base em outros atributos
         /// </summary>
         [NonSerialized]protected float maxSanity_value_stats;
         [NonSerialized]protected float maxSanity_value_set;
         [NonSerialized]protected float maxSanity_value_buffs;
          internal float MaxSanityGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return maxSanity_value_stats+maxSanity_value_set;
          }
           internal void MaxSanitySet(float value,SimObject statsSim=null,bool forceRefresh=false){
            OnRefresh(statsSim);
            maxSanity_value_set=value-maxSanity_value_stats;
            updatedMaxSanity=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            [NonSerialized]protected bool updatedMaxSanity;
         #region Focus
         /// <summary>
         ///  Energia de concentra��o da mente dispon�vel para realizar a��es de valor mental, ou m�gicas
         /// </summary>
         [NonSerialized]protected float focus_value;
          internal float FocusGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return focus_value;
          }
           internal void FocusSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            focus_value=value;
            updatedFocus=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            [NonSerialized]protected bool updatedFocus;
         #region MaxFocus
         /// <summary>
         ///  Energia de concentra��o da mente m�xima para realizar a��es de valor mental, ou m�gicas: valor calculado com base em outros atributos
         /// </summary>
         [NonSerialized]protected float maxFocus_value_stats;
         [NonSerialized]protected float maxFocus_value_set;
         [NonSerialized]protected float maxFocus_value_buffs;
          internal float MaxFocusGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return maxFocus_value_stats+maxFocus_value_set;
          }
           internal void MaxFocusSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            OnRefresh(statsSim);
            maxFocus_value_set=value-maxFocus_value_stats;
            updatedMaxFocus=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            [NonSerialized]protected bool updatedMaxFocus;
         #endregion
         #endregion
         #endregion
         #endregion
        }
    }
}