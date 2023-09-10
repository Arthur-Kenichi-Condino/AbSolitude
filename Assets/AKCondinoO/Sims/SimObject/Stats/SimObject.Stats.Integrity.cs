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
         #region Integrity
         protected float scaleIntegrity;
         /// <summary>
         ///  Integridade física atual para o objeto Sim: se este valor chegar a 0f, o Sim irá morrer ou ser destruído
         /// </summary>
         protected float integrity_value;
          internal float IntegrityGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return integrity_value;
          }
           internal void IntegritySet(float value,SimObject statsSim=null,bool forceRefresh=false){
            integrity_value=value;
            updatedIntegrity=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedIntegrity;
             internal void OnRefresh_Integrity(SimObject statsSim=null){
               if(refreshedMaxIntegrity
               ){
                //Log.DebugMessage("OnRefresh_Integrity");
                refreshedIntegrity=true;
               }
               if(updatedIntegrity){
                refreshedIntegrity=true;
               }
             }
              protected bool refreshedIntegrity;
          #region MaxIntegrity
          /// <summary>
          ///  Integridade física máxima do Sim: valor calculado com base em outros atributos
          /// </summary>
          protected float maxIntegrity_value_stats;
          protected float maxIntegrity_value_set;
          protected float maxIntegrity_value_buffs;
           internal float MaxIntegrityGet(SimObject statsSim=null){
            OnRefresh(statsSim);
            return maxIntegrity_value_stats+maxIntegrity_value_set;
           }
            internal void MaxIntegritySet(float value,SimObject statsSim=null,bool forceRefresh=false){
             OnRefresh(statsSim);
             maxIntegrity_value_set=value-maxIntegrity_value_stats;
             updatedMaxIntegrity=true;
             SetPendingRefresh(statsSim,forceRefresh);
            }
            internal void MaxIntegrityReset(float value,SimObject statsSim=null,bool forceRefresh=false){
             OnRefresh(statsSim);
             maxIntegrity_value_set=0f;
             updatedMaxIntegrity=true;
             SetPendingRefresh(statsSim,forceRefresh);
            }
             protected bool updatedMaxIntegrity;
              internal void OnRefresh_MaxIntegrity(SimObject statsSim=null){
               if(refreshedSimLevel||
                  refreshedAgeLevel||
                  updatedBodily_kinesthetic||
                  refreshedVitality
               ){
                float maxIntegrity=35f;
                float maxIntegrityModifierB=5f;
                maxIntegrityModifierB=Math.Max(maxIntegrityModifierB,bodily_kinesthetic_value*.035f);
                maxIntegrity+=(simLevel_value*maxIntegrityModifierB);
                float maxIntegrityModifierA=(bodily_kinesthetic_value/200f)+((simLevel_value/200f)*.1f);
                for(int level=2;level<=Mathf.FloorToInt(simLevel_value);++level){
                 maxIntegrity+=(level*maxIntegrityModifierA);
                }
                maxIntegrity*=(1f+(vitality_value_stats+vitality_value_set)*.01f);
                if(isTranscendent_value){
                 maxIntegrity*=1.25f;
                }
                //Log.DebugMessage(statsSim+":maxIntegrity:"+maxIntegrity);
                float previousMaxIntegrity=maxIntegrity_value_stats;
                if(previousMaxIntegrity<=0f){
                 scaleIntegrity=-1f;
                }else{
                 scaleIntegrity=maxIntegrity/previousMaxIntegrity;
                }
                refreshedMaxIntegrity=true;
               }
               if(updatedMaxIntegrity){
                refreshedMaxIntegrity=true;
               }
              }
               protected bool refreshedMaxIntegrity;
           #region Stamina
           /// <summary>
           ///  Energia física disponível para realizar ações
           /// </summary>
           protected float stamina_value;
            internal float StaminaGet(SimObject statsSim=null){
             OnRefresh(statsSim);
             return stamina_value;
            }
             internal void StaminaSet(float value,SimObject statsSim=null,bool forceRefresh=false){
              stamina_value=value;
              updatedStamina=true;
              SetPendingRefresh(statsSim,forceRefresh);
             }
              protected bool updatedStamina;
            #region MaxStamina
            /// <summary>
            ///  Energia física máxima para o Sim realizar ações: valor calculado com base em outros atributos
            /// </summary>
            protected float maxStamina_value_stats;
            protected float maxStamina_value_set;
            protected float maxStamina_value_buffs;
             internal float MaxStaminaGet(SimObject statsSim=null){
              OnRefresh(statsSim);
              return maxStamina_value_stats+maxStamina_value_set;
             }
              internal void MaxStaminaSet(float value,SimObject statsSim=null,bool forceRefresh=false){
               OnRefresh(statsSim);
               maxStamina_value_set=value-maxStamina_value_stats;
               updatedMaxStamina=true;
               SetPendingRefresh(statsSim,forceRefresh);
              }
               protected bool updatedMaxStamina;
            #endregion
           #endregion
          #endregion
         #endregion
        }
    }
}