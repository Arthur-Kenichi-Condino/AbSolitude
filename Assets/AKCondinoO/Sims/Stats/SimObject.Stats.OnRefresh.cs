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
         [NonSerialized]protected bool pendingRefresh;
            internal void SetPendingRefresh(SimObject statsSim=null,bool forceRefresh=false){
             pendingRefresh=true;
             if(forceRefresh){
              OnRefresh(statsSim);
             }
            }
            internal void OnRefresh(SimObject statsSim=null){
             if(pendingRefresh){
              OnRefresh_IsTranscendent(statsSim);
               OnRefresh_Experience(statsSim);
               OnRefresh_SimLevel(statsSim);
               OnRefresh_AgeLevel(statsSim);
                 OnRefresh_Luck(statsSim);//  Luck must be applied at the start and then at the end so it adds random attributes
                  OnRefresh_Agility     (statsSim);
                  OnRefresh_Dexterity   (statsSim);
                  OnRefresh_Intelligence(statsSim);
                  OnRefresh_Strength    (statsSim);
                  OnRefresh_Vitality    (statsSim);
                   OnRefresh_Luck_Late(statsSim);
                    OnRefresh_MaxIntegrity(statsSim);
                     OnRefresh_Integrity(statsSim);
                     OnRefresh_PhysicalPowerFlatValue  (statsSim);
                     OnRefresh_PhysicalDefenseFlatValue(statsSim);
              updatedIsTranscendent=false;refreshedIsTranscendent=false;
               updatedExperience    =false;refreshedExperience    =false;
               updatedSimLevel      =false;refreshedSimLevel      =false;
               updatedAgeLevel      =false;refreshedAgeLevel      =false;
                updatedBodily_kinesthetic  =false;
                updatedInterpersonal       =false;
                updatedIntrapersonal       =false;
                updatedLinguistic          =false;
                updatedLogical_mathematical=false;
                updatedMusical             =false;
                updatedNaturalistic        =false;
                updatedSpatial             =false;
                  updatedAgility     =false;refreshedAgility     =false;
                  updatedDexterity   =false;refreshedDexterity   =false;
                  updatedIntelligence=false;refreshedIntelligence=false;
                  updatedStrength    =false;refreshedStrength    =false;
                  updatedVitality    =false;refreshedVitality    =false;
                    updatedMaxIntegrity=false;refreshedMaxIntegrity=false;
                     updatedIntegrity=false;refreshedIntegrity=false;
                     updatedPhysicalPowerFlatValue  =false;refreshedPhysicalPowerFlatValue  =false;
                     updatedPhysicalDefenseFlatValue=false;refreshedPhysicalDefenseFlatValue=false;
              onGeneration=false;
              pendingRefresh=false;
             }
            }
        }
    }
}