#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
        internal partial class Stats{
             #region Agility
             /// <summary>
             ///  Secondary Stats AGI
             /// </summary>
             protected float agility_value_stats;
             protected float agility_value_set;
             protected float agility_value_buffs;
              internal float AgilityGet(SimObject statsSim=null){
               OnRefresh(statsSim);
               return agility_value_stats+agility_value_set;
              }
               internal void AgilitySet(float value,SimObject statsSim=null,bool forceRefresh=false){
                OnRefresh(statsSim);
                agility_value_set=value-agility_value_stats;
                updatedAgility=true;
                SetPendingRefresh(statsSim,forceRefresh);
               }
                protected bool updatedAgility;
             #endregion
             #region Dexterity
             /// <summary>
             ///  Secondary Stats DEX
             /// </summary>
             protected float dexterity_value_stats;
             protected float dexterity_value_set;
             protected float dexterity_value_buffs;
              internal float DexterityGet(SimObject statsSim=null){
               OnRefresh(statsSim);
               return dexterity_value_stats+dexterity_value_set;
              }
               internal void DexteritySet(float value,SimObject statsSim=null,bool forceRefresh=false){
                OnRefresh(statsSim);
                dexterity_value_set=value-dexterity_value_stats;
                updatedDexterity=true;
                SetPendingRefresh(statsSim,forceRefresh);
               }
                protected bool updatedDexterity;
             #endregion
             #region Intelligence
             /// <summary>
             ///  Secondary Stats INT
             /// </summary>
             protected float intelligence_value_stats;
             protected float intelligence_value_set;
             protected float intelligence_value_buffs;
              internal float IntelligenceGet(SimObject statsSim=null){
               OnRefresh(statsSim);
               return intelligence_value_stats+intelligence_value_set;
              }
               internal void IntelligenceSet(float value,SimObject statsSim=null,bool forceRefresh=false){
                OnRefresh(statsSim);
                intelligence_value_set=value-intelligence_value_stats;
                updatedIntelligence=true;
                SetPendingRefresh(statsSim,forceRefresh);
               }
                protected bool updatedIntelligence;
             #endregion
             #region Strength
             /// <summary>
             ///  Secondary Stats STR
             /// </summary>
             protected float strength_value_stats;
             protected float strength_value_set;
             protected float strength_value_buffs;
              internal float StrengthGet(SimObject statsSim=null){
               OnRefresh(statsSim);
               return strength_value_stats+strength_value_set;
              }
               internal void StrengthSet(float value,SimObject statsSim=null,bool forceRefresh=false){
                OnRefresh(statsSim);
                strength_value_set=value-strength_value_stats;
                updatedStrength=true;
                SetPendingRefresh(statsSim,forceRefresh);
               }
                protected bool updatedStrength;
             #endregion
             #region Vitality
             /// <summary>
             ///  Secondary Stats VIT
             /// </summary>
             protected float vitality_value_stats;
             protected float vitality_value_set;
             protected float vitality_value_buffs;
              internal float VitalityGet(SimObject statsSim=null){
               OnRefresh(statsSim);
               return vitality_value_stats+vitality_value_set;
              }
               internal void VitalitySet(float value,SimObject statsSim=null,bool forceRefresh=false){
                OnRefresh(statsSim);
                vitality_value_set=value-vitality_value_stats;
                updatedVitality=true;
                SetPendingRefresh(statsSim,forceRefresh);
               }
                protected bool updatedVitality;
             #endregion
        }
    }
}