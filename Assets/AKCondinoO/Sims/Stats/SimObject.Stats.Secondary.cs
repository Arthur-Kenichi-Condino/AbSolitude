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
         #region Agility
         /// <summary>
         ///  Secondary Stats AGI
         /// </summary>
         [NonSerialized]protected float agility_value_stats;
         [NonSerialized]protected float agility_value_set;
         [NonSerialized]protected float agility_value_buffs;
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
            [NonSerialized]protected bool updatedAgility;
             internal void OnRefresh_Agility(SimObject statsSim=null){
              if(onGeneration||
                 updatedBodily_kinesthetic  ||
                 updatedInterpersonal       ||
                 updatedLinguistic          ||
                 updatedLogical_mathematical||
                 updatedMusical             ||
                 updatedSpatial             
              ){
               agility_value_stats=
                ((  bodily_kinesthetic_value*1f)/13f)+
                ((       interpersonal_value*1f)/13f)+
                ((          linguistic_value*1f)/13f)+
                ((logical_mathematical_value*1f)/13f)+
                ((             musical_value*4f)/13f)+
                ((             spatial_value*5f)/13f)
               ;
               refreshedAgility=true;
              }
              if(updatedAgility
              ){
               refreshedAgility=true;
              }
             }
              [NonSerialized]protected bool refreshedAgility;
         #endregion
         #region Dexterity
         /// <summary>
         ///  Secondary Stats DEX
         /// </summary>
         [NonSerialized]protected float dexterity_value_stats;
         [NonSerialized]protected float dexterity_value_set;
         [NonSerialized]protected float dexterity_value_buffs;
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
            [NonSerialized]protected bool updatedDexterity;
             internal void OnRefresh_Dexterity(SimObject statsSim=null){
              if(onGeneration||
                 updatedBodily_kinesthetic  ||
                 updatedInterpersonal       ||
                 updatedLinguistic          ||
                 updatedLogical_mathematical||
                 updatedMusical             ||
                 updatedSpatial             
              ){
               dexterity_value_stats=
                ((  bodily_kinesthetic_value*2f)/20f)+
                ((       interpersonal_value*3f)/20f)+
                ((          linguistic_value*3f)/20f)+
                ((logical_mathematical_value*3f)/20f)+
                ((             musical_value*4f)/20f)+
                ((             spatial_value*5f)/20f)
               ;
               refreshedDexterity=true;
              }
              if(updatedDexterity
              ){
               refreshedDexterity=true;
              }
             }
              [NonSerialized]protected bool refreshedDexterity;
         #endregion
         #region Intelligence
         /// <summary>
         ///  Secondary Stats INT
         /// </summary>
         [NonSerialized]protected float intelligence_value_stats;
         [NonSerialized]protected float intelligence_value_set;
         [NonSerialized]protected float intelligence_value_buffs;
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
            [NonSerialized]protected bool updatedIntelligence;
             internal void OnRefresh_Intelligence(SimObject statsSim=null){
              if(onGeneration||
                 updatedInterpersonal       ||
                 updatedIntrapersonal       ||
                 updatedLinguistic          ||
                 updatedLogical_mathematical||
                 updatedMusical             ||
                 updatedNaturalistic        
              ){
               intelligence_value_stats=
                ((       interpersonal_value*6f)/28f)+
                ((       intrapersonal_value*6f)/28f)+
                ((          linguistic_value*6f)/28f)+
                ((logical_mathematical_value*6f)/28f)+
                ((             musical_value*2f)/28f)+
                ((        naturalistic_value*2f)/28f)
               ;
               refreshedIntelligence=true;
              }
              if(updatedIntelligence
              ){
               refreshedIntelligence=true;
              }
             }
              [NonSerialized]protected bool refreshedIntelligence;
         #endregion
         #region Strength
         /// <summary>
         ///  Secondary Stats STR
         /// </summary>
         [NonSerialized]protected float strength_value_stats;
         [NonSerialized]protected float strength_value_set;
         [NonSerialized]protected float strength_value_buffs;
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
            [NonSerialized]protected bool updatedStrength;
             internal void OnRefresh_Strength(SimObject statsSim=null){
              if(onGeneration||
                 updatedBodily_kinesthetic  
              ){
               strength_value_stats=
                ((  bodily_kinesthetic_value*4f)/4f)
               ;
               refreshedStrength=true;
              }
              if(updatedStrength
              ){
               refreshedStrength=true;
              }
             }
              [NonSerialized]protected bool refreshedStrength;
         #endregion
         #region Vitality
         /// <summary>
         ///  Secondary Stats VIT
         /// </summary>
         [NonSerialized]protected float vitality_value_stats;
         [NonSerialized]protected float vitality_value_set;
         [NonSerialized]protected float vitality_value_buffs;
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
            [NonSerialized]protected bool updatedVitality;
             internal void OnRefresh_Vitality(SimObject statsSim=null){
              if(onGeneration||
                 updatedBodily_kinesthetic  ||
                 updatedIntrapersonal       ||
                 updatedNaturalistic        
              ){
               vitality_value_stats=
                ((  bodily_kinesthetic_value*3f)/15f)+
                ((       intrapersonal_value*4f)/15f)+
                ((        naturalistic_value*8f)/15f)
               ;
               refreshedVitality=true;
              }
              if(updatedVitality
              ){
               refreshedVitality=true;
              }
             }
              [NonSerialized]protected bool refreshedVitality;
         #endregion
        }
    }
}