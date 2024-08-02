#if UNITY_EDITOR||DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainSendEditDataToServerContainer;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal class VoxelTerrainSendEditDataToServerContainer:BackgroundContainer{
     [NonSerialized]internal bool DEBUG_FORCE_SEND_WHOLE_CHUNK_DATA=false;
     [NonSerialized]internal const int voxelDataToSendSplits=128;
     [NonSerialized]internal int segmentSize;
     [NonSerialized]internal int voxelsPerSegment;
      [NonSerialized]internal int lastSegmentSize;
      [NonSerialized]internal int voxelsInLastSegment;
     [NonSerialized]internal Vector2Int cCoord;
     [NonSerialized]internal Vector2Int cnkRgn;
     [NonSerialized]internal        int cnkIdx;
     [NonSerialized]internal static readonly ConcurrentQueue<Dictionary<int,FastBufferWriter>>dataToSendDictionaryPool=new ConcurrentQueue<Dictionary<int,FastBufferWriter>>();
      [NonSerialized]internal Dictionary<int,FastBufferWriter>dataToSendToServer;
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
    }
    internal class VoxelTerrainSendEditDataToServerMultithreaded:BaseMultithreaded<VoxelTerrainSendEditDataToServerContainer>{
     [NonSerialized]readonly Dictionary<Vector3Int,Voxel>editData=new();
        protected override void Cleanup(){
         editData.Clear();
        }
        protected override void Execute(){
         Log.DebugMessage("VoxelTerrainSendEditDataToServerMultithreaded:Execute:container.DEBUG_FORCE_SEND_WHOLE_CHUNK_DATA:"+container.DEBUG_FORCE_SEND_WHOLE_CHUNK_DATA);
         if(container.segmentSize<=0){
          Log.DebugMessage("'container.segmentSize<=0'");
          return;
         }
         if(!dataToSendDictionaryPool.TryDequeue(out container.dataToSendToServer)){
          container.dataToSendToServer=new Dictionary<int,FastBufferWriter>();
         }
         Vector2Int cnkRgn1=container.cnkRgn;
         Vector2Int cCoord1=container.cCoord;
         if(container.DEBUG_FORCE_SEND_WHOLE_CHUNK_DATA){
          Vector3Int vCoord1;
          for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
          for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
          for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
           if(!editData.ContainsKey(vCoord1)){
            int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
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
            editData.Add(vCoord1,voxel1);
           }
          }}}
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
          Voxel edit=voxelEdited.Value;
          if(writtenDataCount<=0){
           writer=new FastBufferWriter(1100,Allocator.Persistent,container.segmentSize);
           container.dataToSendToServer.Add(segment,writer);
           writer.WriteValueSafe((int)UnnamedMessageTypes.VoxelTerrainChunkEditDataSegment);//message type
           writer.WriteValueSafe(container.cnkIdx);//cnkIdx
           writer.WriteValueSafe((int)segment);//current segment
           writer.WriteValueSafe((int)totalSegments);//total segments (segment count)
           //  segment writes count
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
            segment++;
            writtenDataCount=0;
           }
          }
         }
        }
    }
}