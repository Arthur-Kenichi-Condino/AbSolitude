#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditing;
namespace AKCondinoO.Voxels.Terrain.Editing{
    internal class VoxelTerrainEditingContainer:BackgroundContainer{
     internal readonly Queue<TerrainEditRequest>requests=new Queue<TerrainEditRequest>();
    }
    internal class VoxelTerrainEditingMultithreaded:BaseMultithreaded<VoxelTerrainEditingContainer>{
        protected override void Execute(){
         Log.DebugMessage("VoxelTerrainEditingMultithreaded:Execute()");
        }
    }
}