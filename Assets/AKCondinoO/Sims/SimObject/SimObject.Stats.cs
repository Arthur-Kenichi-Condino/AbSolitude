#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     internal PersistentStats persistentStats;
        internal struct PersistentStats{
         public ListWrapper<StatsFloatData>statsFloats;
            public struct StatsFloatData{
             public Type simStatsType;public string fieldName;public float fieldValue;
            }
            static readonly Dictionary<Type,Dictionary<Type,FieldInfo[]>>statsFloatFields=new Dictionary<Type,Dictionary<Type,FieldInfo[]>>();
            internal void UpdateData(SimObject simObject){
             //Log.DebugMessage("UpdateData");
             if(simObject==null){
              //Log.DebugMessage("collecting persistent stats from destroyed sim on exit save");
             }
             simObject.stats.OnRefresh(simObject);
             Type statsType=simObject.stats.GetType();
             Dictionary<Type,FieldInfo[]>floatFields;
             lock(statsFloatFields){
              if(!statsFloatFields.TryGetValue(statsType,out floatFields)){
               floatFields=new Dictionary<Type,FieldInfo[]>();
               Type derived=statsType;
               do{
                FieldInfo[]fieldsGotten=derived.GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Where(field=>{if(field.FieldType==typeof(float)&&(field.Name.EndsWith("_value")||field.Name.EndsWith("_stats"))){String.Intern(field.Name);Log.DebugMessage(derived+": got field:"+field.Name);return true;}return false;}).ToArray();
                floatFields.Add(derived,fieldsGotten);
                derived=derived.BaseType;
               }while(derived!=null&&derived!=typeof(object));
               statsFloatFields.Add(statsType,floatFields);
              }
             }
             statsFloats=new ListWrapper<StatsFloatData>(
              floatFields.SelectMany(
               kvp=>{
                return kvp.Value.Select(
                 field=>{
                  //Log.DebugMessage("simStatsType:"+kvp.Key+";fieldName:"+field.Name+";fieldValue:"+((float)field.GetValue(simObject.stats)));
                  return new StatsFloatData{
                   simStatsType=kvp.Key,
                   fieldName=field.Name,
                   fieldValue=(float)field.GetValue(simObject.stats),
                  };
                 }
                );
               }
              ).ToList()
             );
            }
        }
     internal static readonly Dictionary<Type,Queue<Stats>>statsPool=new Dictionary<Type,Queue<Stats>>();
     internal Stats stats;
        internal virtual void RenewStats(){
         //Log.DebugMessage("RenewStats");
         if(stats==null){
          if(statsPool.TryGetValue(typeof(Stats),out Queue<Stats>pool)&&pool.Count>0){
           stats=pool.Dequeue();
          }else{
           stats=new Stats();
          }
         }
        }
        internal virtual void ReleaseStats(){
         //Log.DebugMessage("ReleaseStats");
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
             //Log.DebugMessage("Stats InitFrom");
            }
            internal virtual void Generate(){
             //Log.DebugMessage("Stats Generate");
            }
            internal void OnAppliedSkillBuff(SkillBuff skillBuff){
             pendingRefresh=true;
            }
            internal void OnUnappliedSkillBuff(SkillBuff skillBuff){
             pendingRefresh=true;
            }
         protected bool pendingRefresh;
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
         #region Sanity
         /// <summary>
         ///  Sanidade mental ou noção existencial atual para o objeto Sim: se este valor chegar a 0f, o Sim irá desmaiar ou perder o controle das ações
         /// </summary>
         protected float sanity_value;
          internal float SanityGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return sanity_value;
          }
           internal void SanitySet(float value,SimObject statsSim=null,bool forceRefresh=false){
            sanity_value=value;
            updatedSanity=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedSanity;
          #region MaxSanity
          /// <summary>
          ///  Sanidade mental ou noção existencial máxima do objeto Sim: valor calculado com base em outros atributos
          /// </summary>
          protected float maxSanity_value_stats;
          protected float maxSanity_value_set;
          protected float maxSanity_value_buffs;
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
             protected bool updatedMaxSanity;
           #region Focus
           /// <summary>
           ///  Energia de concentração da mente disponível para realizar ações de valor mental, ou mágicas
           /// </summary>
           protected float focus_value;
            internal float FocusGet(SimObject statsSim=null){
             OnRefresh(statsSim);
             return focus_value;
            }
             internal void FocusSet(float value,SimObject statsSim=null,bool forceRefresh=false){
              focus_value=value;
              updatedFocus=true;
              SetPendingRefresh(statsSim,forceRefresh);
             }
              protected bool updatedFocus;
            #region MaxFocus
            /// <summary>
            ///  Energia de concentração da mente máxima para realizar ações de valor mental, ou mágicas: valor calculado com base em outros atributos
            /// </summary>
            protected float maxFocus_value_stats;
            protected float maxFocus_value_set;
            protected float maxFocus_value_buffs;
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
               protected bool updatedMaxFocus;
            #endregion
           #endregion
          #endregion
         #endregion
         //
         #region Bodily_kinesthetic
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float bodily_kinesthetic_value;
          internal float Bodily_kinestheticGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return bodily_kinesthetic_value;
          }
           internal void Bodily_kinestheticSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            bodily_kinesthetic_value=value;
            updatedBodily_kinesthetic=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedBodily_kinesthetic;
         #endregion
             #region Strength
             /// <summary>
             ///  Secondary Stats
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
         #region Spatial
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float spatial_value;
          internal float SpatialGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return spatial_value;
          }
           internal void SpatialSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            spatial_value=value;
            updatedSpatial=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedSpatial;
         #endregion
             #region Agility
             /// <summary>
             ///  Secondary Stats
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
         #region Naturalistic
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float naturalistic_value;
          internal float NaturalisticGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return naturalistic_value;
          }
           internal void NaturalisticSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            naturalistic_value=value;
            updatedNaturalistic=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedNaturalistic;
         #endregion
             #region Vitality
             /// <summary>
             ///  Secondary Stats
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
         #region Interpersonal
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float interpersonal_value;
          internal float InterpersonalGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return interpersonal_value;
          }
           internal void InterpersonalSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            interpersonal_value=value;
            updatedInterpersonal=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedInterpersonal;
         #endregion
         #region Intrapersonal
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float intrapersonal_value;
          internal float IntrapersonalGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return intrapersonal_value;
          }
           internal void IntrapersonalSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            intrapersonal_value=value;
            updatedIntrapersonal=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedIntrapersonal;
         #endregion
         #region Linguistic
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float linguistic_value;
          internal float LinguisticGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return linguistic_value;
          }
           internal void LinguisticSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            linguistic_value=value;
            updatedLinguistic=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedLinguistic;
         #endregion
         #region Logical_mathematical
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float logical_mathematical_value;
          internal float Logical_mathematicalGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return logical_mathematical_value;
          }
           internal void Logical_mathematicalSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            logical_mathematical_value=value;
            updatedLogical_mathematical=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedLogical_mathematical;
         #endregion
             #region Intelligence
             /// <summary>
             ///  Secondary Stats
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
         #region Musical
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float musical_value;
          internal float MusicalGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return musical_value;
          }
           internal void MusicalSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            musical_value=value;
            updatedMusical=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            protected bool updatedMusical;
         #endregion
             #region Dexterity
             /// <summary>
             ///  Secondary Stats
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
         //
         #region Luck
         /// <summary>
         ///  Primary Stats
         /// </summary>
         protected float luck_value;
          protected float lucky_stats_refresh_cooldown_value;
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
            protected bool updatedLuck;
         #endregion
                 //
                 /// <summary>
                 ///  Substats
                 /// </summary>
                 protected float physicalPowerFlatValue;
                 /// <summary>
                 ///  Substats
                 /// </summary>
                 protected float physicalDefenseFlatValue;
                 /// <summary>
                 ///  Substats
                 /// </summary>
                 protected float magicalPowerFlatValue;
                 /// <summary>
                 ///  Substats
                 /// </summary>
                 protected float magicalDefenseFlatValue;
        }
    }
}