#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Water{
    internal class GetVoxelWaterBackgroundContainer:BackgroundContainer{
    }
    internal class GetVoxelWaterMultithreaded:BaseMultithreaded<GetVoxelWaterBackgroundContainer>{
        protected override void Execute(){
        }
    }
}