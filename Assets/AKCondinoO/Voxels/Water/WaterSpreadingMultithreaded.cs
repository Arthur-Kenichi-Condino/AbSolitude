using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.Water.MarchingCubesWater;
namespace AKCondinoO.Voxels.Water{
    //  handles data processing in background;
    //  passively gets data from VoxelSystemConcurrent
    internal class WaterSpreadingContainer:BackgroundContainer{
     internal readonly object synchronizer=new object();
     internal readonly ConcurrentDictionary<int,VoxelWater>voxels=new ConcurrentDictionary<int,VoxelWater>();
    }
    internal class WaterSpreadingMultithreaded:BaseMultithreaded<WaterSpreadingContainer>{
        protected override void Execute(){
        }
    }
}