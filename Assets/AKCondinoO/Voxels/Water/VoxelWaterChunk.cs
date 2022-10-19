#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Water{
    internal class VoxelWaterChunk:MonoBehaviour{
     internal WaterSpreadingContainer waterSpreadingBG=new WaterSpreadingContainer();
        internal void OnInstantiated(){
        }
        internal void ManualUpdate(){
         //Log.DebugMessage("ManualUpdate");
        }
    }
}