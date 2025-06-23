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
         #region Luck
         /// <summary>
         ///  Primary Stats LUK
         /// </summary>
         [NonSerialized]protected float luck_value;
          [NonSerialized]protected float lucky_stats_refresh_cooldown_value;
          internal float LuckGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return luck_value;
          }
           internal void LuckSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            luck_value=value;
            updatedLuck=true;
            lucky_stats_refresh_cooldown_value=0f;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            [NonSerialized]protected bool updatedLuck;
             internal void OnRefresh_Luck(SimObject statsSim=null){
              if(luck_value>0f){
              }
             }
               internal void OnRefresh_Luck_Late(SimObject statsSim=null){
                if(luck_value>0f){
                }
               }
         #endregion
        }
    }
}