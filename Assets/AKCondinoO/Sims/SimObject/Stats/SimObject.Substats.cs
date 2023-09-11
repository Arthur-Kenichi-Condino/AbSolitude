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
                 /// <summary>
                 ///  Substats
                 /// </summary>
                 protected float physicalPowerFlatValue_value_stats;
                 protected float physicalPowerFlatValue_value_set;
                 protected float physicalPowerFlatValue_value_buffs;
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
                    protected bool updatedPhysicalPowerFlatValue;
                     internal void OnRefresh_PhysicalPowerFlatValue(SimObject statsSim=null){
                      if(refreshedSimLevel||
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
                      protected bool refreshedPhysicalPowerFlatValue;
                 /// <summary>
                 ///  Substats
                 /// </summary>
                 protected float physicalDefenseFlatValue_value_stats;
                 protected float physicalDefenseFlatValue_value_set;
                 protected float physicalDefenseFlatValue_value_buffs;
                  internal float PhysicalDefenseFlatValueGet(SimObject statsSim=null){
                   OnRefresh(statsSim);
                   return physicalDefenseFlatValue_value_stats+physicalDefenseFlatValue_value_set;
                  }
                 /// <summary>
                 ///  Substats
                 /// </summary>
                 protected float magicalPowerFlatValue;
                  internal float MagicalPowerFlatValueGet(SimObject statsSim=null){
                   OnRefresh(statsSim);
                   return magicalPowerFlatValue;
                  }
                 /// <summary>
                 ///  Substats
                 /// </summary>
                 protected float magicalDefenseFlatValue;
                  internal float MagicalDefenseFlatValueGet(SimObject statsSim=null){
                   OnRefresh(statsSim);
                   return magicalDefenseFlatValue;
                  }
        }
    }
}