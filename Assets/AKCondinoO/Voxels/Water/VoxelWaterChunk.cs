using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.Water.MarchingCubesWater;
namespace AKCondinoO.Voxels.Water{
    //  handles drawing/rendering: gets data processed in background when finished
    //  also sets the data in VoxelSystemConcurrent so background data can be updated
    internal class VoxelWaterChunk:MonoBehaviour{
        internal void OnInstantiated(){
        }
        internal void ManualUpdate(){
        }
    }
}