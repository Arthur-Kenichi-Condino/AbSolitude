#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using AKCondinoO.Networking;
using AKCondinoO.Voxels.Terrain.Networking;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
        internal partial class ServerData{
         [NonSerialized]internal static readonly ConcurrentQueue<Dictionary<int,(int totalSegments,FastBufferReader segmentData)>>serverVoxelTerrainChunkEditDataSegmentsDictionaryPool=new ConcurrentQueue<Dictionary<int,(int,FastBufferReader)>>();
          [NonSerialized]internal readonly Dictionary<int,Dictionary<int,(int totalSegments,FastBufferReader segmentData)>>serverVoxelTerrainChunkEditDataSegmentsReceivedFromClient=new Dictionary<int,Dictionary<int,(int,FastBufferReader)>>();
        }
     //   private void OnServerReceivedUnnamedMessage(ulong clientId,FastBufferReader reader){
     //    var messageType=(int)UnnamedMessageTypes.Undefined;
     //    reader.ReadValueSafe(out messageType);
     //    if(messageType==(int)UnnamedMessageTypes.FromClientVoxelTerrainChunkEditDataRequest){
     //     //Log.DebugMessage("'messageType==(int)UnnamedMessageTypes.FromClientVoxelTerrainChunkEditDataRequest'");
     //     if(Core.singleton.isServer){
     //      OnServerSideReceivedVoxelTerrainChunkEditDataRequest(clientId,reader);
     //     }
     //    }else if(messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkEditDataSegment){
     //     //Log.DebugMessage("'messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkEditDataSegment'");
     //     if(Core.singleton.isServer){
     //      OnServerSideReceivedVoxelTerrainChunkEditDataSegment(clientId,reader);
     //     }
     //    }
     //   }
     //   void OnServerSideReceivedVoxelTerrainChunkEditDataRequest(ulong clientId,FastBufferReader reader){
     //    //Log.DebugMessage("OnServerSideReceivedVoxelTerrainChunkEditDataRequest:clientId:"+clientId);
     //    int cnkIdx;
     //    reader.ReadValueSafe(out cnkIdx);
     //    //Log.DebugMessage("OnServerSideReceivedVoxelTerrainChunkEditDataRequest:cnkIdx:"+cnkIdx);
     //    if(terrainMessageHandlersAssigned.TryGetValue(cnkIdx,out VoxelTerrainChunkUnnamedMessageHandler cnkMsgr)){
     //     cnkMsgr.OnReceivedVoxelTerrainChunkEditDataRequest(clientId);
     //    }
     //   }
     //   void OnServerSideReceivedVoxelTerrainChunkEditDataSegment(ulong clientId,FastBufferReader reader){
     //    //Log.DebugMessage("OnServerSideReceivedVoxelTerrainChunkEditDataSegment");
     //    FastBufferReader dataReceivedFromClient=new FastBufferReader(reader,Allocator.Persistent,-1,0,Allocator.Persistent);
     //    //  creating a buffer from a buffer puts the reading position on beginning again
     //    var messageType=(int)UnnamedMessageTypes.Undefined;
     //    dataReceivedFromClient.ReadValueSafe(out messageType);
     //    int cnkIdx       ;dataReceivedFromClient.ReadValueSafe(out cnkIdx       );
     //    int segment      ;dataReceivedFromClient.ReadValueSafe(out segment      );
     //    int totalSegments;dataReceivedFromClient.ReadValueSafe(out totalSegments);
     //    if(!serverVoxelTerrainChunkEditDataSegmentsReceivedFromClient.TryGetValue(cnkIdx,out Dictionary<int,(int totalSegments,FastBufferReader segmentData)>segmentsReceivedFromServer)){
     //     if(!serverVoxelTerrainChunkEditDataSegmentsDictionaryPool.TryDequeue(out segmentsReceivedFromServer)){
     //      segmentsReceivedFromServer=new Dictionary<int,(int totalSegments,FastBufferReader segmentData)>();
     //     }
     //     serverVoxelTerrainChunkEditDataSegmentsReceivedFromClient[cnkIdx]=segmentsReceivedFromServer;
     //    }
     //    if(segmentsReceivedFromServer.TryGetValue(segment,out(int totalSegments,FastBufferReader segmentData)oldEditDataSegment)){
     //     oldEditDataSegment.segmentData.Dispose();
     //     segmentsReceivedFromServer.Remove(segment);
     //    }
     //    segmentsReceivedFromServer[segment]=(totalSegments,dataReceivedFromClient);
     //   }
    }
}