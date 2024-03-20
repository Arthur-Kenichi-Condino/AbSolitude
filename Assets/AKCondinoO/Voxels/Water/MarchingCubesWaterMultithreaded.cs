#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Water.MarchingCubes{
    internal class MarchingCubesWaterContainer:BackgroundContainer{
    }
    internal class MarchingCubesWaterMultithreaded:BaseMultithreaded<MarchingCubesWaterContainer>{
        protected override void Execute(){
        }
    }
}