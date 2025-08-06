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
         [NonSerialized]protected float scaleIntegrity;
         /// <summary>
         ///  Integridade física atual para o objeto Sim: se este valor chegar a 0f, o Sim irá morrer ou ser destruído
         /// </summary>
         [NonSerialized]protected float integrity_value;
          internal float IntegrityGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return integrity_value;
          }
           internal void IntegritySet(float value,SimObject statsSim=null,bool forceRefresh=false){
            integrity_value=value;
            updatedIntegrity=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            [NonSerialized]protected bool updatedIntegrity;
             internal void OnRefresh_Integrity(SimObject statsSim=null){
              if(onGeneration||
                 refreshedMaxIntegrity
              ){
               //Log.DebugMessage("OnRefresh_Integrity:maxIntegrity_value_stats+maxIntegrity_value_set:"+(maxIntegrity_value_stats+maxIntegrity_value_set)+":scaleIntegrity:"+scaleIntegrity);
               if(scaleIntegrity<0f){
                integrity_value=maxIntegrity_value_stats+maxIntegrity_value_set;
               }else{
                integrity_value*=scaleIntegrity;
               }
               //Log.DebugMessage(statsSim+":integrity_value:"+integrity_value);
               refreshedIntegrity=true;
              }
              if(updatedIntegrity){
               refreshedIntegrity=true;
              }
              IntegrityRaiseEvent("OnRefresh");
             }
              [NonSerialized]protected bool refreshedIntegrity;
         internal event EventHandler IntegrityEvent;
            internal class IntegrityEventArgs:FloatEventArgs{
            }
            protected virtual void OnIntegrityEvent(IntegrityEventArgs e){
             IntegrityEvent?.Invoke(this,e);
            }
            internal virtual void IntegrityRaiseEvent(){
             IntegrityRaiseEvent("ManualRaise");
            }
            internal virtual void IntegrityRaiseEvent(string statEventFrom){
             var args=new IntegrityEventArgs(){
              statEventFrom=statEventFrom,
              value=integrity_value
             };
             OnIntegrityEvent(args);
            }
         #region MaxIntegrity
         /// <summary>
         ///  Integridade física máxima do Sim: valor calculado com base em outros atributos
         /// </summary>
         [NonSerialized]protected float maxIntegrity_value_stats;
         [NonSerialized]protected float maxIntegrity_value_set;
         [NonSerialized]protected float maxIntegrity_value_buffs;
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
            [NonSerialized]protected bool updatedMaxIntegrity;
             internal void OnRefresh_MaxIntegrity(SimObject statsSim=null){
              if(onGeneration||
                 refreshedSimLevel||
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
               maxIntegrity_value_stats=maxIntegrity;
               if(onGeneration||previousMaxIntegrity<=0f){
                scaleIntegrity=-1f;
               }else{
                scaleIntegrity=maxIntegrity/previousMaxIntegrity;
               }
               refreshedMaxIntegrity=true;
              }
              if(updatedMaxIntegrity){
               refreshedMaxIntegrity=true;
              }
              MaxIntegrityRaiseEvent("OnRefresh");
             }
              [NonSerialized]protected bool refreshedMaxIntegrity;
         internal event EventHandler MaxIntegrityEvent;
            internal class MaxIntegrityEventArgs:FloatEventArgs{
            }
            protected virtual void OnMaxIntegrityEvent(MaxIntegrityEventArgs e){
             MaxIntegrityEvent?.Invoke(this,e);
            }
            internal virtual void MaxIntegrityRaiseEvent(){
             MaxIntegrityRaiseEvent("ManualRaise");
            }
            internal virtual void MaxIntegrityRaiseEvent(string statEventFrom){
             var args=new MaxIntegrityEventArgs(){
              statEventFrom=statEventFrom,
              value=maxIntegrity_value_stats+maxIntegrity_value_set
             };
             OnMaxIntegrityEvent(args);
            }
         #region Stamina
         /// <summary>
         ///  Energia física disponível para realizar ações
         /// </summary>
         [NonSerialized]protected float stamina_value;
          internal float StaminaGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return stamina_value;
          }
           internal void StaminaSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            stamina_value=value;
            updatedStamina=true;
            SetPendingRefresh(statsSim,forceRefresh);
            StaminaRaiseEvent("Set");
           }
            [NonSerialized]protected bool updatedStamina;
         internal event EventHandler StaminaEvent;
            internal class StaminaEventArgs:FloatEventArgs{
            }
            protected virtual void OnStaminaEvent(StaminaEventArgs e){
             StaminaEvent?.Invoke(this,e);
            }
            internal virtual void StaminaRaiseEvent(){
             StaminaRaiseEvent("ManualRaise");
            }
            internal virtual void StaminaRaiseEvent(string statEventFrom){
             var args=new StaminaEventArgs(){
              statEventFrom=statEventFrom,
              value=stamina_value
             };
             OnStaminaEvent(args);
            }
         #region MaxStamina
         /// <summary>
         ///  Energia física máxima para o Sim realizar ações: valor calculado com base em outros atributos
         /// </summary>
         [NonSerialized]protected float maxStamina_value_stats;
         [NonSerialized]protected float maxStamina_value_set;
         [NonSerialized]protected float maxStamina_value_buffs;
          internal float MaxStaminaGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return maxStamina_value_stats+maxStamina_value_set;
          }
           internal void MaxStaminaSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            OnRefresh(statsSim);
            maxStamina_value_set=value-maxStamina_value_stats;
            updatedMaxStamina=true;
            SetPendingRefresh(statsSim,forceRefresh);
            MaxStaminaRaiseEvent("Set");
           }
            [NonSerialized]protected bool updatedMaxStamina;
         internal event EventHandler MaxStaminaEvent;
            internal class MaxStaminaEventArgs:FloatEventArgs{
            }
            protected virtual void OnMaxStaminaEvent(MaxStaminaEventArgs e){
             MaxStaminaEvent?.Invoke(this,e);
            }
            internal virtual void MaxStaminaRaiseEvent(){
             MaxStaminaRaiseEvent("ManualRaise");
            }
            internal virtual void MaxStaminaRaiseEvent(string statEventFrom){
             var args=new MaxStaminaEventArgs(){
              statEventFrom=statEventFrom,
              value=maxStamina_value_stats+maxStamina_value_set
             };
             OnMaxStaminaEvent(args);
            }
         #endregion
         #endregion
         #endregion
         #endregion
        }
    }
}