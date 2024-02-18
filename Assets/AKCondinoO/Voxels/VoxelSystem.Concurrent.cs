using AKCondinoO.Voxels.Water;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
        //  DANGER ZONE: be careful with sync: dead-locks/race-conditions
        internal readonly GetVoxelWaterMultithreaded[]getVoxelWaterBGThreads=new GetVoxelWaterMultithreaded[Environment.ProcessorCount];
        internal readonly ConcurrentBag<GetVoxelWaterBackgroundContainer>getVoxelWaterBGBag=new ConcurrentBag<GetVoxelWaterBackgroundContainer>();
        internal class Concurrent{
         internal static ReaderWriterLockSlim terrainFiles_rwl;
         internal static ReaderWriterLockSlim   waterFiles_rwl;
         internal static string waterCachePath;
         internal static string waterCacheFileFormat="{0}waterChunkCache.{1}.{2}.bin";
         internal static ReaderWriterLockSlim   waterCache_rwl;
         internal static readonly Dictionary<int,(FileStream stream,BinaryWriter writer,BinaryReader reader)>waterCache   =new();
         internal static readonly Dictionary<FileStream,(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)>    waterCacheIds=new();
        }
    }
}