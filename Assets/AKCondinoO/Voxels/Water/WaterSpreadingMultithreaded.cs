using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Water{
    //  handles data processing in background;
    //  passively gets data from VoxelSystemConcurrent
    internal class WaterSpreadingContainer:BackgroundContainer{
     internal readonly VoxelWater[]voxelsOutput=new VoxelWater[VoxelsPerChunk];
    }
    internal class WaterSpreadingMultithreaded:BaseMultithreaded<WaterSpreadingContainer>{
        protected override void Cleanup(){
        }
        protected override void Execute(){
        }
    }
}