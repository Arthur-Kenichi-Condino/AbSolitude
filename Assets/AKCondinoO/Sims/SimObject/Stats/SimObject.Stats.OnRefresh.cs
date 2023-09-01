#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
        internal partial class Stats{
         protected bool pendingRefresh;
            internal void SetPendingRefresh(SimObject statsSim=null,bool forceRefresh=false){
             pendingRefresh=true;
             if(forceRefresh){
              OnRefresh(statsSim);
             }
            }
            internal void OnRefresh(SimObject statsSim=null){
             if(pendingRefresh){
              OnRefresh_IsTranscendent(statsSim);
              OnRefresh_SimLevel      (statsSim);
              //  Luck must be applied at the start and then at the end so it adds random attributes
              OnRefresh_Luck          (statsSim);
              OnRefresh_Agility       (statsSim);
              OnRefresh_Dexterity     (statsSim);
              OnRefresh_Intelligence  (statsSim);
              OnRefresh_Strength      (statsSim);
              OnRefresh_Vitality      (statsSim);
              OnRefresh_Luck_Late     (statsSim);
              updatedIsTranscendent=false;refreshedIsTranscendent=false;
              updatedSimLevel      =false;refreshedSimLevel      =false;
              updatedBodily_kinesthetic  =false;
              updatedInterpersonal       =false;
              updatedIntrapersonal       =false;
              updatedLinguistic          =false;
              updatedLogical_mathematical=false;
              updatedMusical             =false;
              updatedNaturalistic        =false;
              updatedSpatial             =false;
              pendingRefresh=false;
             }
            }
        }
    }
}