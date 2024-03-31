#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
namespace AKCondinoO.Voxels.Biomes{
    internal partial class BaseBiome{
             internal void SetvxlWater(
              Vector3Int noiseInputRounded,
               double[][][]noiseCache1,//  ...terrain height cache
                int oftIdx,
                 int noiseIndex,
                  ref VoxelWater vxl
             ){
              Vector3 noiseInput=noiseInputRounded+deround;
              vxl=VoxelWater.air;
             }
    }
}