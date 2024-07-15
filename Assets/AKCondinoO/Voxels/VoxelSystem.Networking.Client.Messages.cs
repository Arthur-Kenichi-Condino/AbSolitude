#if UNITY_EDITOR||DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
     [NonSerialized]float clientSendMessageDelay=0.05f;
      [NonSerialized]float clientSendMessageTimer=10f;
      [NonSerialized]internal readonly Dictionary<int,FastBufferWriter>clientVoxelTerrainChunkEditDataRequestsToSend=new Dictionary<int,FastBufferWriter>();
     [NonSerialized]internal int clientMaxVoxelTerrainChunkEditDataRequestsPerFrame=8;
      [NonSerialized]internal int clientVoxelTerrainChunkEditDataRequestsSent;
       [NonSerialized]readonly List<int>clientVoxelTerrainChunkEditDataRequestsSentToRemove=new List<int>();
        private void OnClientReceivedUnnamedMessage(ulong clientId,FastBufferReader reader){
         var messageType=(int)UnnamedMessageTypes.Undefined;
         reader.ReadValueSafe(out messageType);
        }
    }
}