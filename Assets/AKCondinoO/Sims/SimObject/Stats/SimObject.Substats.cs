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
                 protected float physicalPowerFlatValue;
                  internal float PhysicalPowerFlatValueGet(SimObject statsSim=null){
                   OnRefresh(statsSim);
                   return physicalPowerFlatValue;
                  }
                 /// <summary>
                 ///  Substats
                 /// </summary>
                 protected float physicalDefenseFlatValue;
                  internal float PhysicalDefenseFlatValueGet(SimObject statsSim=null){
                   OnRefresh(statsSim);
                   return physicalDefenseFlatValue;
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