#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Water.MarchingCubes{
    internal class MarchingCubesWaterBackgroundContainer:BackgroundContainer{
    }
    internal class MarchingCubesWaterMultithreaded:BaseMultithreaded<MarchingCubesWaterBackgroundContainer>{
        protected override void Execute(){
        }
    }
}