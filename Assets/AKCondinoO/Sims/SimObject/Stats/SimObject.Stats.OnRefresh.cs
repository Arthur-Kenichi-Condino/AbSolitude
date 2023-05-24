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
              OnRefresh_Agility     (statsSim);
              OnRefresh_Dexterity   (statsSim);
              OnRefresh_Intelligence(statsSim);
              OnRefresh_Strength    (statsSim);
              OnRefresh_Vitality    (statsSim);
              pendingRefresh=false;
             }
            }
        }
    }
}