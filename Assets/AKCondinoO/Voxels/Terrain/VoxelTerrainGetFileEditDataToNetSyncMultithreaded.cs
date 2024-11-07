#if UNITY_EDITOR||DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using AKCondinoO.Voxels.Terrain.Editing;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditingMultithreaded;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainChunkArraySync;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainGetFileEditDataToNetSyncContainer;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal class VoxelTerrainGetFileEditDataToNetSyncContainer:BackgroundContainer{
     [NonSerialized]internal bool DEBUG_FORCE_SEND_ALL_VOXEL_DATA=false;
     [NonSerialized]internal const int chunkVoxelArraySplits=128;
        internal static int VoxelsInSegment(int segment){
         return segment<(chunkVoxelArraySplits-1)?VoxelsPerChunk/chunkVoxelArraySplits:(VoxelsPerChunk/chunkVoxelArraySplits)+VoxelsPerChunk%chunkVoxelArraySplits;
        }
     [NonSerialized]internal Vector2Int cCoord;
     [NonSerialized]internal Vector2Int cnkRgn;
     [NonSerialized]internal        int cnkIdx;
     [NonSerialized]internal string editsFileName;
     [NonSerialized]internal FileStream editsFileStream;
     [NonSerialized]internal StreamReader editsFileStreamReader;
     [NonSerialized]internal readonly NetVoxel[][]voxels=new NetVoxel[chunkVoxelArraySplits][];
     [NonSerialized]internal readonly bool[]changes   =new bool[chunkVoxelArraySplits];
     [NonSerialized]internal readonly bool[]changesSet=new bool[chunkVoxelArraySplits];
        internal VoxelTerrainGetFileEditDataToNetSyncContainer(){
         for(int i=0;i<voxels.Length;++i){
          voxels[i]=new NetVoxel[VoxelsInSegment(i)];
         }
        }
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
          if(editsFileStream      !=null){editsFileStream      .Dispose();}
          if(editsFileStreamReader!=null){editsFileStreamReader.Dispose();}
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
    }
    internal class VoxelTerrainGetFileEditDataToNetSyncMultithreaded:BaseMultithreaded<VoxelTerrainGetFileEditDataToNetSyncContainer>{
     [NonSerialized]internal static readonly ConcurrentQueue<Dictionary<Vector3Int,TerrainEditOutputData>>terrainEditOutputDataPool=new ConcurrentQueue<Dictionary<Vector3Int,TerrainEditOutputData>>();
        protected override void Cleanup(){
        }
        protected override void Execute(){
         //Log.DebugMessage("VoxelTerrainGetFileEditDataToNetSyncMultithreaded:Execute:container.DEBUG_FORCE_SEND_ALL_VOXEL_DATA:"+container.DEBUG_FORCE_SEND_ALL_VOXEL_DATA);
         Array.Clear(container.changes   ,0,container.changes   .Length);
         Array.Clear(container.changesSet,0,container.changesSet.Length);
         for(int i=0;i<container.voxels.Length;++i){
          Array.Clear(container.voxels[i],0,container.voxels[i].Length);
         }
         Vector2Int cnkRgn1=container.cnkRgn;
         Vector2Int cCoord1=container.cCoord;
         if(!terrainEditOutputDataPool.TryDequeue(out Dictionary<Vector3Int,TerrainEditOutputData>editData)){
          editData=new Dictionary<Vector3Int,TerrainEditOutputData>();
         }
         VoxelSystem.Concurrent.terrainFiles_rwl.EnterReadLock();
         try{
          string editsFileName=string.Format(CultureInfoUtil.en_US,VoxelTerrainEditing.terrainEditingFileFormat,VoxelTerrainEditing.terrainEditingPath,cCoord1.x,cCoord1.y);
          if(container.editsFileStream==null||container.editsFileName!=editsFileName){
           container.editsFileName=editsFileName;
           if(container.editsFileStream!=null){
            FileStream fStream=container.editsFileStream;
            fStream                        .Dispose();
            container.editsFileStreamReader.Dispose();
            container.editsFileStream      =null;
            container.editsFileStreamReader=null;
           }
           if(File.Exists(editsFileName)){
            container.editsFileStream=new FileStream(editsFileName,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
            container.editsFileStreamReader=new StreamReader(container.editsFileStream);
           }
          }
          if(container.editsFileStream!=null){
           FileStream fileStream=container.editsFileStream;
           StreamReader fileStreamReader=container.editsFileStreamReader;
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
              editData.Add(vCoord,edit);
             }
            }
           }
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.terrainFiles_rwl.ExitReadLock();
         }
         int segment=0;
         int writtenDataCount=0;
         int voxelsInThisSegment=VoxelsInSegment(segment);
         Vector3Int vCoord1;
         for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
         for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
         for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
          int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
          if(writtenDataCount<voxelsInThisSegment){
           bool biomeVoxel=false;
           NetVoxel netVoxel;
           if(editData.TryGetValue(vCoord1,out TerrainEditOutputData edit)){
            netVoxel=new NetVoxel(vxlIdx1,edit.density,edit.material);
           }else{
            biomeVoxel=true;
            Voxel voxel1=new Voxel();
            Vector3Int noiseInput=vCoord1;noiseInput.x+=cnkRgn1.x;
                                          noiseInput.z+=cnkRgn1.y;
            VoxelSystem.biome.Setvxl(
             noiseInput,
              null,
               null,
                0,
                 vCoord1.z+vCoord1.x*Depth,
                  ref voxel1
            );
            netVoxel=new NetVoxel(vxlIdx1,voxel1.density,voxel1.material);
           }
           container.changes[segment]|=container.DEBUG_FORCE_SEND_ALL_VOXEL_DATA||(!biomeVoxel&&container.voxels[segment][writtenDataCount]!=netVoxel);
           container.voxels[segment][writtenDataCount]=netVoxel;
          }
          if(++writtenDataCount>=voxelsInThisSegment){
           //Log.DebugMessage("'++writtenDataCount>=voxelsInThisSegment':segment:"+segment+":voxelsInThisSegment:"+voxelsInThisSegment);
           segment++;
           if(segment>=container.voxels.Length){
            //Log.DebugMessage("'segment>=container.voxels.Length':container.voxels.Length:"+container.voxels.Length);
            goto _Break;
           }
           voxelsInThisSegment=VoxelsInSegment(segment);
           writtenDataCount=0;
          }
         }}}
         _Break:{}
         editData.Clear();
         terrainEditOutputDataPool.Enqueue(editData);
        }
    }
}