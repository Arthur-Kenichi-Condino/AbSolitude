#if UNITY_EDITOR||DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
     float clientSendMessageDelay=1f;
     float clientSendMessageTimer=10f;
     internal readonly Dictionary<int,FastBufferWriter>clientVoxelTerrainChunkEditDataRequestsToSend=new Dictionary<int,FastBufferWriter>();
      internal int clientMaxVoxelTerrainChunkEditDataRequestsPerFrame=32;
       internal int clientVoxelTerrainChunkEditDataRequestsSent;
     internal readonly List<int>clientVoxelTerrainChunkEditDataRequestsSentToRemove=new List<int>();
        private void OnClientReceivedUnnamedMessage(ulong clientId,FastBufferReader reader){
         var messageType=(int)UnnamedMessageTypes.Undefined;
         reader.ReadValueSafe(out messageType);
         if(messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkEditDataSegment){
          //Log.DebugMessage("messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkEditDataSegment");
          if(Core.singleton.isClient){
           OnClientSideReceivedVoxelTerrainChunkEditDataSegment(clientId,reader);
          }
         }
        }
     internal static readonly ConcurrentQueue<Dictionary<int,FastBufferReader>>clientVoxelTerrainChunkEditDataSegmentsDictionaryPool=new ConcurrentQueue<Dictionary<int,FastBufferReader>>();
     internal readonly Dictionary<int,Dictionary<int,FastBufferReader>>clientVoxelTerrainChunkEditDataSegmentsReceivedFromServer=new Dictionary<int,Dictionary<int,FastBufferReader>>();
        void OnClientSideReceivedVoxelTerrainChunkEditDataSegment(ulong clientId,FastBufferReader reader){
         Log.DebugMessage("OnClientSideReceivedVoxelTerrainChunkEditDataSegment");
         FastBufferReader dataReceivedFromServer=new FastBufferReader(reader,Allocator.Persistent,-1,0,Allocator.Persistent);
         //  creating a buffer from a buffer puts the reading position on beginning again
         var messageType=(int)UnnamedMessageTypes.Undefined;
         reader.ReadValueSafe(out messageType);
         int cnkIdx;
         reader.ReadValueSafe(out cnkIdx);
         int segment;
         reader.ReadValueSafe(out segment);
        }
    }
}