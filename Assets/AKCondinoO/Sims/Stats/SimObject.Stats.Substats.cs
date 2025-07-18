#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
        internal partial class Stats{
         //
         /// <summary>
         ///  Substats
         /// </summary>
         [NonSerialized]protected float physicalPowerFlatValue_value_stats;
         [NonSerialized]protected float physicalPowerFlatValue_value_set;
         [NonSerialized]protected float physicalPowerFlatValue_value_buffs;
          internal float PhysicalPowerFlatValueGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return physicalPowerFlatValue_value_stats+physicalPowerFlatValue_value_set;
          }
           internal void PhysicalPowerFlatValueSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            OnRefresh(statsSim);
            physicalPowerFlatValue_value_set=value-physicalPowerFlatValue_value_stats;
            updatedPhysicalPowerFlatValue=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            [NonSerialized]protected bool updatedPhysicalPowerFlatValue;
             internal void OnRefresh_PhysicalPowerFlatValue(SimObject statsSim=null){
              if(onGeneration||
                 refreshedSimLevel||
                 refreshedAgeLevel||
                 refreshedDexterity||
                 refreshedStrength||
                 refreshedVitality
              ){
               physicalPowerFlatValue_value_stats=
                (simLevel_value/4f)+
                 ((dexterity_value_stats+dexterity_value_set)/3f)+
                  ((strength_value_stats+strength_value_set)/1f)+
                   ((vitality_value_stats+vitality_value_set)/5f);
               //Log.DebugMessage("OnRefresh_PhysicalPowerFlatValue:physicalPowerFlatValue_value_stats:"+physicalPowerFlatValue_value_stats);
               refreshedPhysicalPowerFlatValue=true;
              }
              if(updatedPhysicalPowerFlatValue){
               refreshedPhysicalPowerFlatValue=true;
              }
             }
              [NonSerialized]protected bool refreshedPhysicalPowerFlatValue;
         /// <summary>
         ///  Substats
         /// </summary>
         [NonSerialized]protected float physicalDefenseFlatValue_value_stats;
         [NonSerialized]protected float physicalDefenseFlatValue_value_set;
         [NonSerialized]protected float physicalDefenseFlatValue_value_buffs;
          internal float PhysicalDefenseFlatValueGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return physicalDefenseFlatValue_value_stats+physicalDefenseFlatValue_value_set;
          }
           internal void PhysicalDefenseFlatValueSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            OnRefresh(statsSim);
            physicalDefenseFlatValue_value_set=value-physicalDefenseFlatValue_value_stats;
            updatedPhysicalDefenseFlatValue=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            [NonSerialized]protected bool updatedPhysicalDefenseFlatValue;
             internal void OnRefresh_PhysicalDefenseFlatValue(SimObject statsSim=null){
              if(onGeneration||
                 refreshedSimLevel||
                 refreshedAgeLevel||
                 refreshedAgility||
                 refreshedStrength||
                 refreshedVitality
              ){
               physicalDefenseFlatValue_value_stats=
                (simLevel_value/2f)+
                 ((agility_value_stats+agility_value_set)/5f)+
                  ((strength_value_stats+strength_value_set)/3f)+
                   ((vitality_value_stats+vitality_value_set)/1f);
               //Log.DebugMessage("OnRefresh_PhysicalDefenseFlatValue:physicalDefenseFlatValue_value_stats:"+physicalDefenseFlatValue_value_stats);
               refreshedPhysicalDefenseFlatValue=true;
              }
              if(updatedPhysicalDefenseFlatValue){
               refreshedPhysicalDefenseFlatValue=true;
              }
             }
              [NonSerialized]protected bool refreshedPhysicalDefenseFlatValue;
            internal static float ProcessStatPhysicalDamageOn(SimObject simObject,SimObject fromSimObject){
             var stats=simObject.stats;
             if(stats!=null){
              float integrity=stats.IntegrityGet(simObject);
              //Log.DebugMessage("ProcessStatPhysicalDamageOn:'current integrity':integrity:"+integrity);
              float damageFromSimObject=1f;
              var fromSimObjectStats=fromSimObject.stats;
              if(fromSimObjectStats!=null){
               float fromSimObjectPhysicalPowerFlatValue=fromSimObjectStats.PhysicalPowerFlatValueGet(fromSimObject);
               //Log.DebugMessage("ProcessStatPhysicalDamageOn:fromSimObjectPhysicalPowerFlatValue:"+fromSimObjectPhysicalPowerFlatValue);
               float physicalDefenseFlatValue=stats.PhysicalDefenseFlatValueGet(simObject);
               //Log.DebugMessage("ProcessStatPhysicalDamageOn:physicalDefenseFlatValue:"+physicalDefenseFlatValue);
               damageFromSimObject=fromSimObjectPhysicalPowerFlatValue*((4000f+physicalDefenseFlatValue)/(4000f+physicalDefenseFlatValue*10f));
               //Log.DebugMessage("ProcessStatPhysicalDamageOn:'hard damageFromSimObject':damageFromSimObject:"+damageFromSimObject);
               if(simObject is BaseAI baseAI){
                double randomMultiplier=baseAI.math_random.NextDouble(.5d,1.5d);
                damageFromSimObject*=(float)randomMultiplier;
                //Log.DebugMessage("ProcessStatPhysicalDamageOn:'random damageFromSimObject':damageFromSimObject:"+damageFromSimObject);
               }
              }
              integrity-=damageFromSimObject;
              stats.IntegritySet(integrity,simObject);
              return integrity;
             }
             return 0f;
            }
            internal static float ProcessStatPhysicalDamageOn(SimObject simObject,Hurtboxes hurtbox,SimWeapon fromSimWeapon){
             var stats=simObject.stats;
             if(stats!=null){
              float integrity=stats.IntegrityGet(simObject);
              //Log.DebugMessage("ProcessStatPhysicalDamageOn:'current integrity':integrity:"+integrity);
              float maxIntegrity=stats.MaxIntegrityGet(simObject);
              float damageFromSimWeapon=Mathf.Max(1f,hurtbox.bodyPartDamageMultiplierForFirearm*maxIntegrity);
              integrity-=damageFromSimWeapon;
              stats.IntegritySet(integrity,simObject);
              return integrity;
             }
             return 0f;
            }
         /// <summary>
         ///  Substats
         /// </summary>
         [NonSerialized]protected float magicalPowerFlatValue;
          internal float MagicalPowerFlatValueGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return magicalPowerFlatValue;
          }
         /// <summary>
         ///  Substats
         /// </summary>
         [NonSerialized]protected float magicalDefenseFlatValue;
          internal float MagicalDefenseFlatValueGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return magicalDefenseFlatValue;
          }
        }
    }
}