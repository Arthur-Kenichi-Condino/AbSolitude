using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
        /// <summary>
        ///  DANGER ZONE: careful with sync: dead-locks/race-conditions
        /// </summary>
        internal class Concurrent{
         internal static ReaderWriterLockSlim terrain_rwl;
         internal static ReaderWriterLockSlim terrainFileData_rwl;
         internal static readonly Dictionary<int,Terrain.MarchingCubes.MarchingCubesTerrain.Voxel[]>terrainVoxels=new Dictionary<int,Terrain.MarchingCubes.MarchingCubesTerrain.Voxel[]>();
         internal static readonly Dictionary<Terrain.MarchingCubes.MarchingCubesTerrain.Voxel[],(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)>terrainVoxelsId=new Dictionary<Terrain.MarchingCubes.MarchingCubesTerrain.Voxel[],(Vector2Int,Vector2Int,int)>();
         internal static ReaderWriterLockSlim water_rwl;
         internal static readonly Dictionary<int,Water.MarchingCubes.MarchingCubesWater.VoxelWater[]>waterVoxels=new Dictionary<int,Water.MarchingCubes.MarchingCubesWater.VoxelWater[]>();
          internal static readonly Dictionary<int,Dictionary<Vector3Int,double>>absorbing=new();
          internal static readonly Dictionary<int,Dictionary<Vector3Int,double>>spreading=new();
         internal static readonly Dictionary<Water.MarchingCubes.MarchingCubesWater.VoxelWater[],(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)>waterVoxelsId=new Dictionary<Water.MarchingCubes.MarchingCubesWater.VoxelWater[],(Vector2Int,Vector2Int,int)>();
        }
    }
}