#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     internal Stats?stats;
        internal virtual void StatsInit(){
         stats=new Stats();
        }
        internal struct Stats{
            internal void OnAppliedSkillBuff(SkillBuff skillBuff){
             pendingRefresh=true;
            }
            internal void OnUnappliedSkillBuff(SkillBuff skillBuff){
             pendingRefresh=true;
            }
         bool pendingRefresh;
            void OnRefresh(SimObject statsSim=null){
             if(pendingRefresh){
              pendingRefresh=false;
             }
            }
         //
         #region Integrity
         /// <summary>
         ///  Função get da integridade física atual para o Sim
         /// </summary>
         internal float IntegrityGet(SimObject statsSim=null){
          OnRefresh(statsSim);
          return integrity_value;
         }
         /// <summary>
         ///  Função set
         /// </summary>
         internal void IntegritySet(float value,SimObject statsSim=null,bool forceRefresh=false){
          integrity_value=value;
          pendingRefresh=true;
          if(forceRefresh){
           OnRefresh(statsSim);
          }
         }
          float integrity_value;
          #region MaxIntegrity
          /// <summary>
          ///  Função get da integridade física máxima do Sim
          /// </summary>
          internal float MaxIntegrityGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return maxIntegrity_value;
          }
          internal void MaxIntegritySet(float value,SimObject statsSim=null,bool forceRefresh=false){
           maxIntegrity_value=value;
           pendingRefresh=true;
           if(forceRefresh){
            OnRefresh(statsSim);
           }
          }
           float maxIntegrity_value;
             #region Stamina
             /// <summary>
             ///  
             /// </summary>
             internal float StaminaGet(SimObject statsSim=null){
              OnRefresh(statsSim);
              return stamina_value;
             }
             internal void StaminaSet(float value,SimObject statsSim=null,bool forceRefresh=false){
              stamina_value=value;
              pendingRefresh=true;
              if(forceRefresh){
               OnRefresh(statsSim);
              }
             }
              float stamina_value;
              #region MaxStamina
              /// <summary>
              ///  
              /// </summary>
              internal float MaxStaminaGet(SimObject statsSim=null){
               OnRefresh(statsSim);
               return maxStamina_value;
              }
              internal void MaxStaminaSet(float value,SimObject statsSim=null,bool forceRefresh=false){
               maxStamina_value=value;
               pendingRefresh=true;
               if(forceRefresh){
                OnRefresh(statsSim);
               }
              }
              float maxStamina_value;
              #endregion
             #endregion
          #endregion
         #endregion
         #region Sanity
         /// <summary>
         ///  
         /// </summary>
         internal float SanityGet(SimObject statsSim=null){
          OnRefresh(statsSim);
          return sanity_value;
         }
         /// <summary>
         ///  Função set
         /// </summary>
         internal void SanitySet(float value,SimObject statsSim=null,bool forceRefresh=false){
          sanity_value=value;
          pendingRefresh=true;
          if(forceRefresh){
           OnRefresh(statsSim);
          }
         }
          float sanity_value;
          /// <summary>
          ///  
          /// </summary>
          internal float MaxSanityGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return maxSanity_value;
          }
           float maxSanity_value;
             /// <summary>
             ///  
             /// </summary>
             public float focus;
              /// <summary>
              ///  
              /// </summary>
              public float maxFocus;
         #endregion
         //
         /// <summary>
         ///  
         /// </summary>
         public float bodily_kinesthetic;
             /// <summary>
             ///  
             /// </summary>
             public float strength;
         /// <summary>
         ///  
         /// </summary>
         public float spatial;
             /// <summary>
             ///  
             /// </summary>
             public float agility;
         /// <summary>
         ///  
         /// </summary>
         public float naturalistic;
             /// <summary>
             ///  
             /// </summary>
             public float vitality;
         /// <summary>
         ///  
         /// </summary>
         public float interpersonal;
         /// <summary>
         ///  
         /// </summary>
         public float intrapersonal;
         /// <summary>
         ///  
         /// </summary>
         public float linguistic;
         /// <summary>
         ///  
         /// </summary>
         public float logical_mathematical;
             /// <summary>
             ///  
             /// </summary>
             public float intelligence;
         /// <summary>
         ///  
         /// </summary>
         public float musical;
             /// <summary>
             ///  
             /// </summary>
             public float dexterity;
         //
         /// <summary>
         ///  
         /// </summary>
         public float luck;
                 //
                 /// <summary>
                 ///  
                 /// </summary>
                 public float physicalPowerFlatValue;
                 /// <summary>
                 ///  
                 /// </summary>
                 public float physicalDefenseFlatValue;
                 /// <summary>
                 ///  
                 /// </summary>
                 public float magicalPowerFlatValue;
                 /// <summary>
                 ///  
                 /// </summary>
                 public float magicalDefenseFlatValue;
        }
    }
}