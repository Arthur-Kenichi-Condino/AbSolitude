using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
        /// <summary>
        ///  DANGER ZONE: careful with sync: dead-locks/race-conditions
        /// </summary>
        internal class Concurrent{
         internal static string terrainCachePath;
         internal static string terrainCacheFileFormat="{0}chunkCache.{1}.{2}.txt";
         internal static ReaderWriterLockSlim terrainCache_rwl;
         internal static ReaderWriterLockSlim terrainFiles_rwl;
         internal static readonly Dictionary<int,(FileStream stream,StreamWriter writer,StreamReader reader)>terrainCache   =new();
         internal static readonly Dictionary<FileStream,(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)>    terrainCacheIds=new();
         internal static ReaderWriterLockSlim waterCache_rwl;
         internal static ReaderWriterLockSlim waterFiles_rwl;
         //internal static readonly Dictionary<int,Water.MarchingCubes.MarchingCubesWater.VoxelWater?[]>waterVoxelsOutput=new Dictionary<int,Water.MarchingCubes.MarchingCubesWater.VoxelWater?[]>();
         //internal static readonly Dictionary<Water.MarchingCubes.MarchingCubesWater.VoxelWater?[],(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)>waterVoxelsId=new Dictionary<Water.MarchingCubes.MarchingCubesWater.VoxelWater?[],(Vector2Int,Vector2Int,int)>();
        }
    }
}