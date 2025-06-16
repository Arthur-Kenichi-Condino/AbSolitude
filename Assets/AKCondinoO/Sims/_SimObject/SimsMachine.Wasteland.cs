#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Voxels.Biomes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimsMachine{
        internal void SetWastelandSpawnSettings(){
         //Log.DebugMessage("SetWastelandSpawnSettings()");
         spawnSettingsByBiome.Add(Biomes.Wasteland,
          new SimsMachineSpawnSettings{
           spawns=new Dictionary<Type,SimsMachineSpawnSettings.SimObjectSettings>{
           },
          }
         );
        }
        internal void OnWastelandSpawning(){
         //Log.DebugMessage("OnWastelandSpawning()");
         OnDefaultSpawning();
        }
    }
}