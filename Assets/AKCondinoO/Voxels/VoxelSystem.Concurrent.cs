#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Voxels.Terrain.Editing;
using AKCondinoO.Voxels.Terrain.MarchingCubes;
using AKCondinoO.Voxels.Terrain.SimObjectsPlacing;
using AKCondinoO.Voxels.Water;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditingMultithreaded;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
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
         internal static string waterNeighbourhoodCachePath;
         internal static string waterNeighbourhoodCacheAbsorbingFileFormat="{0}absorbingWater.{1}.{2}.bin";
         internal static string waterNeighbourhoodCacheSpreadingFileFormat="{0}spreadingWater.{1}.{2}.bin";
         internal static ReaderWriterLockSlim   waterCache_rwl;
         internal static readonly Dictionary<int,(FileStream stream,BinaryWriter writer,BinaryReader reader)>waterCache   =new();
         internal static readonly Dictionary<FileStream,(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)>    waterCacheIds=new();
         internal static readonly Dictionary<int,HashSet<(FileStream stream,BinaryWriter writer,BinaryReader reader)>>waterNeighbourhoodSpreadingCache=new();
         internal static readonly Dictionary<int,HashSet<(FileStream stream,BinaryWriter writer,BinaryReader reader)>>waterNeighbourhoodAbsorbingCache=new();
         internal static readonly Dictionary<FileStream,(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)>             waterNeighbourhoodSpreadingCacheIds=new();
         internal static readonly Dictionary<FileStream,(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)>             waterNeighbourhoodAbsorbingCacheIds=new();
          internal static readonly ConcurrentQueue<HashSet<(FileStream stream,BinaryWriter writer,BinaryReader reader)>>waterNeighbourhoodCacheListPool=new();
         internal static ReaderWriterLockSlim   waterNeighbourhoodCache_rwl;
         internal static ReaderWriterLockSlim  spawnMapsData_rwl;
         internal static ReaderWriterLockSlim spawnMapsFiles_rwl;
         internal static string spawnMapsPath;
         internal static string spawnMapsFileFormat="{0}spawnMap.{1}.{2}.bin";
         internal static readonly Dictionary<int,(string fileName,FileStream stream,BinaryWriter writer,BinaryReader reader)>spawnMapsFiles=new();
         internal static readonly Dictionary<int,int>spawnMapsOpen=new();
         internal static readonly Dictionary<int,SpawnMapInfo[]>spawnMaps=new();
         internal static readonly Dictionary<int,SpawnMapInfo[]>spawnMaps3D=new();
         internal static readonly Dictionary<int,Dictionary<int,int>>surfaceDataOpen=new();
         internal static readonly Dictionary<int,Dictionary<int,Dictionary<Vector3Int,SpawnCandidateData>>>surfaceHasData=new();
          internal static readonly Dictionary<int,Dictionary<int,HashSet<Vector3Int>>>surfaceHasNoData=new();
         internal static readonly Dictionary<int,Dictionary<int,Dictionary<Vector3Int,bool>>>surfaceState=new();
         internal static readonly Dictionary<int,Dictionary<int,Dictionary<Vector3Int,Vector3>>>surfaceNormalsPredicted=new();
         internal static ReaderWriterLockSlim  surfaceSpawnData_rwl;
         internal static ReaderWriterLockSlim surfaceSpawnFiles_rwl;
         static readonly Dictionary<int,string      >editsFileCacheName        =new Dictionary<int,string      >();
         static readonly Dictionary<int,FileStream  >editsFileCacheStream      =new Dictionary<int,FileStream  >();
         static readonly Dictionary<int,StreamReader>editsFileCacheStreamReader=new Dictionary<int,StreamReader>();
         static readonly Dictionary<int,int         >editsFileCacheLifetime    =new Dictionary<int,int         >();
          internal static readonly ConcurrentQueue<Voxel[]>voxelsArrayPool=new ConcurrentQueue<Voxel[]>();
            //  TO DO: []vCoordsToGet, (create Binary cache file)
            internal static void GetVoxelsBG(Vector2Int cCoord,Vector2Int dis,Dictionary<int,Voxel[]>voxels,
             double[][][]noiseCache1,//  ...terrain height cache
              MaterialId[][][]materialIdCache1,
             Vector3Int?vCoordToGet=null
            ){
             for(int x=-dis.x;x<=dis.x;x++){
             for(int y=-dis.y;y<=dis.y;y++){
              Vector2Int cCoord1=cCoord+new Vector2Int(x,y);
              if(Math.Abs(cCoord1.x)>=MaxcCoordx||
                 Math.Abs(cCoord1.y)>=MaxcCoordy)
              {
               continue;
              }
              int cnkIdx=GetcnkIdx(cCoord1.x,cCoord1.y);
              if(!voxels.TryGetValue(cnkIdx,out Voxel[]voxelsArray)){
               if(!voxelsArrayPool.TryDequeue(out voxelsArray)){
                voxelsArray=new Voxel[VoxelsPerChunk];
               }
               voxels.Add(cnkIdx,voxelsArray);
              }
              //  TO DO: create Binary cache file
              VoxelSystem.Concurrent.terrainFiles_rwl.EnterReadLock();
              try{
               FileStream fileStream;
               StreamReader fileStreamReader;
               lock(editsFileCacheStream){
                if(!editsFileCacheName.TryGetValue(cnkIdx,out string editsFileName)){
                 editsFileName=string.Format(CultureInfoUtil.en_US,VoxelTerrainEditing.terrainEditingFileFormat,VoxelTerrainEditing.terrainEditingPath,cCoord1.x,cCoord1.y);
                 if(!File.Exists(editsFileName)){
                  goto _SkipFiles;
                 }
                 editsFileCacheStream      .Add(cnkIdx,fileStream=new FileStream(editsFileName,FileMode.Open,FileAccess.Read,FileShare.ReadWrite));
                 editsFileCacheStreamReader.Add(cnkIdx,fileStreamReader=new StreamReader(editsFileCacheStream[cnkIdx]));
                 editsFileCacheName        .Add(cnkIdx,editsFileName);
                }else{
                 fileStream=editsFileCacheStream[cnkIdx];
                 fileStreamReader=editsFileCacheStreamReader[cnkIdx];
                }
               }
               fileStream.Position=0L;
               fileStreamReader.DiscardBufferedData();
               string line;
               while((line=fileStreamReader.ReadLine())!=null){
                if(string.IsNullOrEmpty(line)){continue;}
                int vCoordStringStart=line.IndexOf("vCoord=(");
                if(vCoordStringStart>=0){
                   vCoordStringStart+=8;
                 int vCoordStringEnd=line.IndexOf(") , ",vCoordStringStart);
                 string vCoordString=line.Substring(vCoordStringStart,vCoordStringEnd-vCoordStringStart);
                 string[]xyzString=vCoordString.Split(',');
                 int vCoordx=int.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
                 int vCoordy=int.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
                 int vCoordz=int.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
                 Vector3Int vCoord=new Vector3Int(vCoordx,vCoordy,vCoordz);
                 int editStringStart=vCoordStringEnd+4;
                 editStringStart=line.IndexOf("terrainEditOutputData=",editStringStart);
                 if(editStringStart>=0){
                  int editStringEnd=line.IndexOf(" , }",editStringStart)+4;
                  string editString=line.Substring(editStringStart,editStringEnd-editStringStart);
                  TerrainEditOutputData edit=TerrainEditOutputData.Parse(editString);
                  voxelsArray[GetvxlIdx(vCoord.x,vCoord.y,vCoord.z)]=new Voxel(edit.density,Vector3.zero,edit.material);
                 }
                }
               }
               _SkipFiles:{}
              }catch{
               throw;
              }finally{
               VoxelSystem.Concurrent.terrainFiles_rwl.ExitReadLock();
              }
              Vector2Int cnkRgn1=cCoordTocnkRgn(cCoord1);
              if(vCoordToGet!=null){
               Vector3Int vCoord1=vCoordToGet.Value;
               GetVoxelAt(vCoord1);
               return;
              }else{
               Vector3Int vCoord1;
               for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
               for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
               for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
                GetVoxelAt(vCoord1);
               }}}
              }
              void GetVoxelAt(Vector3Int vCoord1){
               int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
               if(voxelsArray[vxlIdx1].isCreated){
                return;
               }
               Vector3Int noiseInput=vCoord1;noiseInput.x+=cnkRgn1.x;
                                             noiseInput.z+=cnkRgn1.y;
               VoxelSystem.biome.Setvxl(
                noiseInput,
                 noiseCache1,
                  materialIdCache1,
                   0,
                    vCoord1.z+vCoord1.x*Depth,
                     ref voxelsArray[vxlIdx1]
               );
              }
             }}
            }
            //  TO DO: this:
            internal static void ReleaseCacheAndDispose(){
             waterCache   .Clear();//  disposed in container
             waterCacheIds.Clear();
             foreach(var kvp in editsFileCacheStream){
              kvp.Value                          .Dispose();
              editsFileCacheStreamReader[kvp.Key].Dispose();
             }
             editsFileCacheName        .Clear();
             editsFileCacheStream      .Clear();
             editsFileCacheStreamReader.Clear();
             editsFileCacheLifetime    .Clear();
            }
        }
    }
}