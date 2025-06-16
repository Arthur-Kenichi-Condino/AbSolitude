#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Terrain{
    internal class GetVoxelBackgroundContainer:BackgroundContainer{
    }
    internal class GetVoxelMultithreaded:BaseMultithreaded<GetVoxelBackgroundContainer>{
        protected override void Execute(){
        }
    }
}