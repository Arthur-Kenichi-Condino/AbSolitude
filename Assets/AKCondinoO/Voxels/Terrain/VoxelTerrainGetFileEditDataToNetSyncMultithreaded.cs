#if UNITY_EDITOR
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
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainChunkUnnamedMessageHandler;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal class VoxelTerrainGetFileEditDataToNetSyncContainer:BackgroundContainer{
     internal bool DEBUG_FORCE_SEND_ALL_VOXEL_DATA=false;
     //internal int segmentSize;
     //internal int voxelsPerSegment;
     internal Vector2Int cCoord;
     internal Vector2Int cnkRgn;
     internal        int cnkIdx;
     internal string editsFileName;
     internal FileStream editsFileStream;
     internal StreamReader editsFileStreamReader;
     //internal Dictionary<int,FastBufferWriter>dataToSendToClients;
     internal readonly NetVoxel[][]voxels=new NetVoxel[Splits+1][];
        internal VoxelTerrainGetFileEditDataToNetSyncContainer(){
         for(int i=0;i<voxels.Length;++i){
          voxels[i]=new NetVoxel[i<Splits?VoxelsPerChunk/Splits:VoxelsPerChunk%Splits];
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
     internal static readonly ConcurrentQueue<Dictionary<Vector3Int,TerrainEditOutputData>>terrainEditOutputDataPool=new ConcurrentQueue<Dictionary<Vector3Int,TerrainEditOutputData>>();
        protected override void Cleanup(){
        }
        protected override void Execute(){
         Log.DebugMessage("VoxelTerrainGetFileEditDataToNetSyncMultithreaded:Execute:container.DEBUG_FORCE_SEND_ALL_VOXEL_DATA:"+container.DEBUG_FORCE_SEND_ALL_VOXEL_DATA);
         //if(!dataToSendDictionaryPool.TryDequeue(out container.dataToSendToClients)){
         // container.dataToSendToClients=new Dictionary<int,FastBufferWriter>();
         //}
         //if(container.segmentSize<=0){
         // Log.DebugMessage("container.segmentSize<=0");
         // return;
         //}
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
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.terrainFiles_rwl.ExitReadLock();
         }
         //if(container.DEBUG_SEND_UNCHANGED_VOXEL_DATA){
         // Vector3Int vCoord1;
         // for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
         // for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
         // for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
         //  if(!editData.ContainsKey(vCoord1)){
         //   int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
         //   Voxel voxel1=new Voxel();
         //   Vector3Int noiseInput=vCoord1;noiseInput.x+=cnkRgn1.x;
         //                                 noiseInput.z+=cnkRgn1.y;
         //   VoxelSystem.biome.Setvxl(
         //    noiseInput,
         //     null,
         //      null,
         //       0,
         //        vCoord1.z+vCoord1.x*Depth,
         //         ref voxel1
         //   );
         //   editData.Add(vCoord1,new TerrainEditOutputData(voxel1.density,voxel1.material));
         //  }
         // }}}
         //}
         int segment=0;
         int writtenDataCount=0;
         Vector3Int vCoord1;
         for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
         for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
         for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
          int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
          int voxelsInThisSegment=segment<Splits?VoxelsPerChunk/Splits:VoxelsPerChunk%Splits;
          if(writtenDataCount<voxelsInThisSegment){
           if(editData.TryGetValue(vCoord1,out TerrainEditOutputData edit)){
            container.voxels[segment][writtenDataCount]=new NetVoxel(vxlIdx1,edit.density,edit.material);
           }else{
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
            container.voxels[segment][writtenDataCount]=new NetVoxel(vxlIdx1,voxel1.density,voxel1.material);
           }
          }
          if(++writtenDataCount>=voxelsInThisSegment){
           segment++;
           writtenDataCount=0;
          }
          if(segment>=container.voxels.Length){
           Log.Warning("'segment>=container.voxels.Length':'this should not happen'");
           goto _Break;
          }
         }}}
         _Break:{}
         editData.Clear();
         terrainEditOutputDataPool.Enqueue(editData);
         //VoxelSystem.Concurrent.terrainFiles_rwl.EnterReadLock();
         //try{
         // Vector2Int cnkRgn1=container.cnkRgn;
         // Vector2Int cCoord1=container.cCoord;
         // string editsFileName=string.Format(CultureInfoUtil.en_US,VoxelTerrainEditing.terrainEditingFileFormat,VoxelTerrainEditing.terrainEditingPath,cCoord1.x,cCoord1.y);
         // if(container.editsFileStream==null||container.editsFileName!=editsFileName){
         //  container.editsFileName=editsFileName;
         //  if(container.editsFileStream!=null){
         //   FileStream fStream=container.editsFileStream;
         //   fStream                        .Dispose();
         //   container.editsFileStreamReader.Dispose();
         //   container.editsFileStream      =null;
         //   container.editsFileStreamReader=null;
         //  }
         //  if(File.Exists(editsFileName)){
         //   container.editsFileStream=new FileStream(editsFileName,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
         //   container.editsFileStreamReader=new StreamReader(container.editsFileStream);
         //  }
         // }
         // if(container.editsFileStream!=null||container.DEBUG_SEND_UNCHANGED_VOXEL_DATA){
         //  if(!terrainEditOutputDataPool.TryDequeue(out Dictionary<Vector3Int,TerrainEditOutputData>editData)){
         //   editData=new Dictionary<Vector3Int,TerrainEditOutputData>();
         //  }
         //  if(container.editsFileStream!=null){
         //   FileStream fileStream=container.editsFileStream;
         //   StreamReader fileStreamReader=container.editsFileStreamReader;
         //   fileStream.Position=0L;
         //   fileStreamReader.DiscardBufferedData();
         //   string line;
         //   while((line=fileStreamReader.ReadLine())!=null){
         //    if(string.IsNullOrEmpty(line)){continue;}
         //    int vCoordStringStart=line.IndexOf("vCoord=(");
         //    if(vCoordStringStart>=0){
         //       vCoordStringStart+=8;
         //     int vCoordStringEnd=line.IndexOf(") , ",vCoordStringStart);
         //     string vCoordString=line.Substring(vCoordStringStart,vCoordStringEnd-vCoordStringStart);
         //     string[]xyzString=vCoordString.Split(',');
         //     int vCoordx=int.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
         //     int vCoordy=int.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
         //     int vCoordz=int.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
         //     Vector3Int vCoord=new Vector3Int(vCoordx,vCoordy,vCoordz);
         //     int editStringStart=vCoordStringEnd+4;
         //     editStringStart=line.IndexOf("terrainEditOutputData=",editStringStart);
         //     if(editStringStart>=0){
         //      int editStringEnd=line.IndexOf(" , }",editStringStart)+4;
         //      string editString=line.Substring(editStringStart,editStringEnd-editStringStart);
         //      TerrainEditOutputData edit=TerrainEditOutputData.Parse(editString);
         //      editData.Add(vCoord,edit);
         //     }
         //    }
         //   }
         //  }
         //  if(container.DEBUG_SEND_UNCHANGED_VOXEL_DATA){
         //   Vector3Int vCoord1;
         //   for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
         //   for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
         //   for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
         //    if(!editData.ContainsKey(vCoord1)){
         //     int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
         //     Voxel voxel1=new Voxel();
         //     Vector3Int noiseInput=vCoord1;noiseInput.x+=cnkRgn1.x;
         //                                   noiseInput.z+=cnkRgn1.y;
         //     VoxelSystem.biome.Setvxl(
         //      noiseInput,
         //       null,
         //        null,
         //         0,
         //          vCoord1.z+vCoord1.x*Depth,
         //           ref voxel1
         //     );
         //     editData.Add(vCoord1,new TerrainEditOutputData(voxel1.density,voxel1.material));
         //    }
         //   }}}
         //  }
         //  int totalSegments=Mathf.CeilToInt((float)editData.Count/(float)container.voxelsPerSegment);
         //  int lastSegmentWriteDataCount=editData.Count%container.voxelsPerSegment;
         //  if(lastSegmentWriteDataCount<=0){
         //   lastSegmentWriteDataCount=container.voxelsPerSegment;
         //  }
         //  FastBufferWriter writer;
         //  int writtenDataCount=0;
         //  int segment=0;
         //  foreach(var voxelEdited in editData){
         //   Vector3Int vCoord=voxelEdited.Key;
         //   TerrainEditOutputData edit=voxelEdited.Value;
         //   if(writtenDataCount<=0){
         //    writer=new FastBufferWriter(1100,Allocator.Persistent,container.segmentSize);
         //    writer.WriteValueSafe((int)UnnamedMessageTypes.VoxelTerrainChunkEditDataSegment);//message type
         //    writer.WriteValueSafe(container.cnkIdx);//cnkIdx
         //    writer.WriteValueSafe((int)segment);//current segment
         //    writer.WriteValueSafe((int)totalSegments);//total segments (segment count)
         //    //segment writes count
         //    if(segment<(totalSegments-1)){
         //     writer.WriteValueSafe((int)container.voxelsPerSegment);
         //    }else{
         //     writer.WriteValueSafe((int)lastSegmentWriteDataCount);
         //    }
         //   }
         //   if(writtenDataCount++<container.voxelsPerSegment){
         //    BytePacker.WriteValuePacked(writer,GetvxlIdx(vCoord.x,vCoord.y,vCoord.z));
         //    BytePacker.WriteValuePacked(writer,(double)edit.density);
         //    BytePacker.WriteValuePacked(writer,(ushort)edit.material);
         //    if(writtenDataCount>=container.voxelsPerSegment){
         //     container.dataToSendToClients.Add(segment,writer);
         //     segment++;
         //     writtenDataCount=0;
         //    }
         //   }
         //  }
         //  editData.Clear();
         //  terrainEditOutputDataPool.Enqueue(editData);
         // }
         //}catch{
         // throw;
         //}finally{
         // VoxelSystem.Concurrent.terrainFiles_rwl.ExitReadLock();
         //}
        }
    }
}