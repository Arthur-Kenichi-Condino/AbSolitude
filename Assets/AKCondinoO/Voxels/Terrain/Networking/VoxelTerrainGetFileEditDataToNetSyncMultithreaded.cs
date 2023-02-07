#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using AKCondinoO.Voxels.Terrain.Editing;
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
     internal int segmentSize;
     internal int voxelsPerSegment;
     internal Vector2Int cCoord;
     internal Vector2Int cnkRgn;
     internal        int cnkIdx;
     internal string editsFileName;
     internal FileStream editsFileStream;
     internal StreamReader editsFileStreamReader;
     internal Dictionary<int,FastBufferWriter>dataToSendToClients;
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
        protected override void Execute(){
         //Log.DebugMessage("VoxelTerrainGetFileEditDataToNetSyncMultithreaded:Execute:container.voxelsPerSegment:"+container.voxelsPerSegment);
         if(!dataToSendDictionaryPool.TryDequeue(out container.dataToSendToClients)){
          container.dataToSendToClients=new Dictionary<int,FastBufferWriter>();
         }
         if(container.segmentSize<=0){
          Log.DebugMessage("container.segmentSize<=0");
          return;
         }
         VoxelSystem.Concurrent.terrainFileData_rwl.EnterReadLock();
         try{
          Vector2Int cCoord1=container.cCoord;
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
           if(!terrainEditOutputDataPool.TryDequeue(out Dictionary<Vector3Int,TerrainEditOutputData>editData)){
            editData=new Dictionary<Vector3Int,TerrainEditOutputData>();
           }
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
           int totalSegments=Mathf.CeilToInt((float)editData.Count/(float)container.voxelsPerSegment);
           int lastSegmentWriteDataCount=editData.Count%container.voxelsPerSegment;
           if(lastSegmentWriteDataCount<=0){
            lastSegmentWriteDataCount=container.voxelsPerSegment;
           }
           FastBufferWriter writer;
           int writtenDataCount=0;
           int segment=0;
           foreach(var voxelEdited in editData){
            Vector3Int vCoord=voxelEdited.Key;
            TerrainEditOutputData edit=voxelEdited.Value;
            if(writtenDataCount<=0){
             writer=new FastBufferWriter(1100,Allocator.Persistent,container.segmentSize);
             writer.WriteValueSafe((int)UnnamedMessageTypes.VoxelTerrainChunkEditDataSegment);//message type
             writer.WriteValueSafe(container.cnkIdx);//cnkIdx
             writer.WriteValueSafe((int)segment);//current segment
             writer.WriteValueSafe((int)totalSegments);//total segments (segment count)
             //segment writes count
             if(segment<(totalSegments-1)){
              writer.WriteValueSafe((int)container.voxelsPerSegment);
             }else{
              writer.WriteValueSafe((int)lastSegmentWriteDataCount);
             }
            }
            if(writtenDataCount++<container.voxelsPerSegment){
             BytePacker.WriteValuePacked(writer,GetvxlIdx(vCoord.x,vCoord.y,vCoord.z));
             BytePacker.WriteValuePacked(writer,(double)edit.density);
             BytePacker.WriteValuePacked(writer,(ushort)edit.material);
             if(writtenDataCount>=container.voxelsPerSegment){
              container.dataToSendToClients.Add(segment,writer);
              segment++;
              writtenDataCount=0;
             }
            }
           }
           editData.Clear();
           terrainEditOutputDataPool.Enqueue(editData);
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.terrainFileData_rwl.ExitReadLock();
         }
        }
    }
}