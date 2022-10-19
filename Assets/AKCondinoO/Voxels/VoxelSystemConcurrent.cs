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
     internal static readonly ConcurrentDictionary<int,Terrain.MarchingCubes.MarchingCubesTerrain.Voxel[]>terrainVoxels      =new ConcurrentDictionary<int,Terrain.MarchingCubes.MarchingCubesTerrain.Voxel[]>();
     internal static readonly ConcurrentDictionary<Terrain.MarchingCubes.MarchingCubesTerrain.Voxel[],int>terrainVoxelscnkIdx=new ConcurrentDictionary<Terrain.MarchingCubes.MarchingCubesTerrain.Voxel[],int>();
     internal static ReaderWriterLockSlim waterrwl;
     internal static readonly ConcurrentDictionary<int,Water.MarchingCubes.MarchingCubesWater.VoxelWater[]>waterVoxels      =new ConcurrentDictionary<int,Water.MarchingCubes.MarchingCubesWater.VoxelWater[]>();
     internal static readonly ConcurrentDictionary<Water.MarchingCubes.MarchingCubesWater.VoxelWater[],int>waterVoxelscnkIdx=new ConcurrentDictionary<Water.MarchingCubes.MarchingCubesWater.VoxelWater[],int>();
    }
}