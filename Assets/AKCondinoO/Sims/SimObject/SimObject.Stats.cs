#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     internal PersistentStats persistentStats;
        internal struct PersistentStats{
            static readonly Dictionary<Type,PropertyInfo[]>propertiesByType=new Dictionary<Type,PropertyInfo[]>();
            internal void UpdateData(SimObject simObject){
             Log.DebugMessage("UpdateData");
             if(simObject==null){
              Log.DebugMessage("collecting persistent stats from destroyed sim on exit save");
             }
             simObject.stats.OnRefresh(simObject);
             lock(propertiesByType){
             }
            }
        }
     internal static readonly Dictionary<Type,Queue<Stats>>statsPool=new Dictionary<Type,Queue<Stats>>();
     internal Stats stats;
        internal virtual void RenewStats(){
         Log.DebugMessage("RenewStats");
         if(stats==null){
          if(statsPool.TryGetValue(typeof(Stats),out Queue<Stats>pool)&&pool.Count>0){
           stats=pool.Dequeue();
          }else{
           stats=new Stats();
          }
         }
        }
        internal virtual void ReleaseStats(){
         Log.DebugMessage("ReleaseStats");
         if(stats!=null){
          Type statsType=stats.GetType();
          if(!statsPool.TryGetValue(statsType,out Queue<Stats>pool)){
           statsPool.Add(statsType,pool=new Queue<Stats>());
          }
          pool.Enqueue(stats);
         }
         stats=null;
        }
        internal class Stats{
            internal virtual void InitFrom(PersistentStats persistentStats){
             Log.DebugMessage("Stats InitFrom");
            }
            internal virtual void Generate(){
             Log.DebugMessage("Stats Generate");
            }
            internal void OnAppliedSkillBuff(SkillBuff skillBuff){
             pendingRefresh=true;
            }
            internal void OnUnappliedSkillBuff(SkillBuff skillBuff){
             pendingRefresh=true;
            }
         bool pendingRefresh;
            internal void SetPendingRefresh(SimObject statsSim=null,bool forceRefresh=false){
             pendingRefresh=true;
             if(forceRefresh){
              OnRefresh(statsSim);
             }
            }
            internal void OnRefresh(SimObject statsSim=null){
             if(pendingRefresh){
              pendingRefresh=false;
             }
            }
         //
         #region Integrity
         /// <summary>
         ///  Fun��o get da integridade f�sica atual para o Sim
         /// </summary>
         internal float IntegrityGet(SimObject statsSim=null){
          OnRefresh(statsSim);
          return integrity_value;
         }
          /// <summary>
          ///  Fun��o set
          /// </summary>
          internal void IntegritySet(float value,SimObject statsSim=null,bool forceRefresh=false){
           integrity_value=value;
           updatedIntegrity=true;
           SetPendingRefresh(statsSim,forceRefresh);
          }
           float integrity_value;
           bool updatedIntegrity;
          #region MaxIntegrity
          /// <summary>
          ///  Fun��o get da integridade f�sica m�xima do Sim
          /// </summary>
          internal float MaxIntegrityGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return maxIntegrity_value_stats+maxIntegrity_value_set;
          }
           /// <summary>
           /// 
           /// </summary>
           internal void MaxIntegritySet(float value,SimObject statsSim=null,bool forceRefresh=false){
            OnRefresh(statsSim);
            maxIntegrity_value_set=value-maxIntegrity_value_stats;
            updatedMaxIntegrity=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
           /// <summary>
           /// 
           /// </summary>
           internal void MaxIntegrityReset(float value,SimObject statsSim=null,bool forceRefresh=false){
            OnRefresh(statsSim);
            maxIntegrity_value_set=0f;
            updatedMaxIntegrity=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            float maxIntegrity_value_stats;
            float maxIntegrity_value_set;
            float maxIntegrity_value_buffs;
            bool updatedMaxIntegrity;
             #region Stamina
             /// <summary>
             ///  
             /// </summary>
             internal float StaminaGet(SimObject statsSim=null){
              OnRefresh(statsSim);
              return stamina_value;
             }
              /// <summary>
              /// 
              /// </summary>
              internal void StaminaSet(float value,SimObject statsSim=null,bool forceRefresh=false){
               stamina_value=value;
               updatedStamina=true;
               SetPendingRefresh(statsSim,forceRefresh);
              }
               float stamina_value;
               bool updatedStamina;
              #region MaxStamina
              /// <summary>
              ///  
              /// </summary>
              internal float MaxStaminaGet(SimObject statsSim=null){
               OnRefresh(statsSim);
               return maxStamina_value_stats+maxStamina_value_set;
              }
               /// <summary>
               /// 
               /// </summary>
               internal void MaxStaminaSet(float value,SimObject statsSim=null,bool forceRefresh=false){
                OnRefresh(statsSim);
                maxStamina_value_set=value-maxStamina_value_stats;
                updatedMaxStamina=true;
                SetPendingRefresh(statsSim,forceRefresh);
               }
                float maxStamina_value_stats;
                float maxStamina_value_set;
                bool updatedMaxStamina;
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
          ///  Fun��o set
          /// </summary>
          internal void SanitySet(float value,SimObject statsSim=null,bool forceRefresh=false){
           sanity_value=value;
           updatedSanity=true;
           SetPendingRefresh(statsSim,forceRefresh);
          }
           float sanity_value;
           bool updatedSanity;
          #region MaxSanity
          /// <summary>
          ///  
          /// </summary>
          internal float MaxSanityGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return maxSanity_value_stats+maxSanity_value_set;
          }
           /// <summary>
           /// 
           /// </summary>
           internal void MaxSanitySet(float value,SimObject statsSim=null,bool forceRefresh=false){
            OnRefresh(statsSim);
            maxSanity_value_set=value-maxSanity_value_stats;
            updatedMaxSanity=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            float maxSanity_value_stats;
            float maxSanity_value_set;
            bool updatedMaxSanity;
             #region Focus
             /// <summary>
             ///  
             /// </summary>
             internal float FocusGet(SimObject statsSim=null){
              OnRefresh(statsSim);
              return focus_value;
             }
              /// <summary>
              /// 
              /// </summary>
              internal void FocusSet(float value,SimObject statsSim=null,bool forceRefresh=false){
               focus_value=value;
               pendingRefresh=true;
               if(forceRefresh){
                OnRefresh(statsSim);
               }
              }
               float focus_value;
              #region MaxFocus
              /// <summary>
              ///  
              /// </summary>
              internal float MaxFocusGet(SimObject statsSim=null){
               OnRefresh(statsSim);
               return maxFocus_value;
              }
               /// <summary>
               /// 
               /// </summary>
               internal void MaxFocusSet(float value,SimObject statsSim=null,bool forceRefresh=false){
                maxFocus_value=value;
                pendingRefresh=true;
                if(forceRefresh){
                 OnRefresh(statsSim);
                }
               }
                float maxFocus_value;
              #endregion
             #endregion
          #endregion
         #endregion
         //
         #region Bodily_kinesthetic
         /// <summary>
         ///  
         /// </summary>
         internal float Bodily_kinestheticGet(SimObject statsSim=null){
          OnRefresh(statsSim);
          return bodily_kinesthetic_value;
         }
          /// <summary>
          /// 
          /// </summary>
          internal void Bodily_kinestheticSet(float value,SimObject statsSim=null,bool forceRefresh=false){
           bodily_kinesthetic_value=value;
           pendingRefresh=true;
           if(forceRefresh){
            OnRefresh(statsSim);
           }
          }
           float bodily_kinesthetic_value;
         #endregion
             /// <summary>
             ///  
             /// </summary>
             float strength_value;
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