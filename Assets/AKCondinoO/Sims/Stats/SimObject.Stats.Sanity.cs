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
         ///  Sanidade mental ou noção existencial atual para o objeto Sim: se este valor chegar a 0f, o Sim irá desmaiar ou perder o controle das ações
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
         ///  Sanidade mental ou noção existencial máxima do objeto Sim: valor calculado com base em outros atributos
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
         ///  Energia de concentração da mente disponível para realizar ações de valor mental, ou mágicas
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
         ///  Energia de concentração da mente máxima para realizar ações de valor mental, ou mágicas: valor calculado com base em outros atributos
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