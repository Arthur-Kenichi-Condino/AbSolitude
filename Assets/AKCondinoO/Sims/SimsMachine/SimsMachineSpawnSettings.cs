#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimsMachineSpawnSettings{
        internal struct SimObjectSettings{
         internal int count;
         internal float chance;
        }
     internal Dictionary<Type,SimObjectSettings>spawns;
    }
}