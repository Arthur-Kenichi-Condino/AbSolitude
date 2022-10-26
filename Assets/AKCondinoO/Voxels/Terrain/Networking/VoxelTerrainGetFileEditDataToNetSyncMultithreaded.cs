#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using AKCondinoO.Voxels.Terrain.Editing;
using System.Collections;
using System.Collections.Generic;
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
        protected override void Execute(){
         //Log.DebugMessage("VoxelTerrainGetFileEditDataToNetSyncMultithreaded:Execute:container.voxelsPerSegment:"+container.voxelsPerSegment);
         if(!dataToSendDictionaryPool.TryDequeue(out container.dataToSendToClients)){
          container.dataToSendToClients=new Dictionary<int,FastBufferWriter>();
         }
         if(container.segmentSize<=0){
          Log.DebugMessage("container.segmentSize<=0");
          return;
         }
         VoxelSystem.Concurrent.terrainFileDatarwl.EnterReadLock();
         try{
          Vector2Int cCoord1=container.cCoord;
          string editsFileName=string.Format(CultureInfoUtil.en_US,VoxelTerrainEditing.terrainEditingFileFormat,VoxelTerrainEditing.terrainEditingPath,cCoord1.x,cCoord1.y);
          if(container.editsFileStream==null||container.editsFileName!=editsFileName){
           container.editsFileName=editsFileName;
          }

          FastBufferWriter writer;
          int writtenDataCount=0;
          int segment=0;
          //testing, REMOVE:
             Vector3Int vCoord1;
             for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
             for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
             for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
              if(writtenDataCount<=0){
             //  //Log.DebugMessage("start writing to segment:"+segment);
             //  // sizeof(int) message type
             //  // sizeof(int) cnkIdx
             //  // sizeof(int) total segments (segment count)
             //  // sizeof(int) current segment
             //  // sizeof(int) segment writes count
               writer=new FastBufferWriter(1100,Allocator.Persistent,container.segmentSize);
               writer.WriteValueSafe((int)UnnamedMessageTypes.VoxelTerrainChunkEditDataSegment);
               writer.WriteValueSafe(container.cnkIdx);
               writer.WriteValueSafe((int)117);
               writer.WriteValueSafe((int)segment);
               writer.WriteValueSafe((int)278);
              }
              if(writtenDataCount++<container.voxelsPerSegment){
               BytePacker.WriteValuePacked(writer,GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z));
               //writer.WriteValueSafe(GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z));
               //writer.WriteValueSafe(vCoord1.x);
               //writer.WriteValueSafe(vCoord1.y);
               //writer.WriteValueSafe(vCoord1.z);
               BytePacker.WriteValuePacked(writer,(double)0.0d);
               //writer.WriteValueSafe((double)0.0d);
               BytePacker.WriteValuePacked(writer,(ushort)MaterialId.Air);
               //writer.WriteValueSafe((ushort)MaterialId.Air);
               if(writtenDataCount>=container.voxelsPerSegment){
                container.dataToSendToClients.Add(segment,writer);
                segment++;
                writtenDataCount=0;
               }
              }
             }}}

         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.terrainFileDatarwl.ExitReadLock();
         }
        }
    }
}