#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
        internal partial class Stats{
         //
         #region Integrity
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