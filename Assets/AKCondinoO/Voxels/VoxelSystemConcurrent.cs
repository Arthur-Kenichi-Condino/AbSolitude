using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace AKCondinoO.Voxels{
    /// <summary>
    ///  DANGER ZONE: careful with sync: dead-locks/race-conditions
    /// </summary>
    internal class VoxelSystemConcurrent{
     internal static ReaderWriterLockSlim terrainrwl;
     internal static readonly Dictionary<int,Terrain.MarchingCubes.MarchingCubesTerrain.Voxel[]>terrainVoxels=new Dictionary<int,Terrain.MarchingCubes.MarchingCubesTerrain.Voxel[]>();
     internal static readonly Dictionary<Terrain.MarchingCubes.MarchingCubesTerrain.Voxel[],(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)>terrainVoxelsId=new Dictionary<Terrain.MarchingCubes.MarchingCubesTerrain.Voxel[],(Vector2Int,Vector2Int,int)>();
     internal static ReaderWriterLockSlim waterrwl;
     internal static readonly Dictionary<int,Water.MarchingCubes.MarchingCubesWater.VoxelWater[]>waterVoxels=new Dictionary<int,Water.MarchingCubes.MarchingCubesWater.VoxelWater[]>();
     internal static readonly Dictionary<Water.MarchingCubes.MarchingCubesWater.VoxelWater[],(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)>waterVoxelsId=new Dictionary<Water.MarchingCubes.MarchingCubesWater.VoxelWater[],(Vector2Int,Vector2Int,int)>();
    }
}